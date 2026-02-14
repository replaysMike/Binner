import React, { useState, useEffect, useRef, useCallback } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from 'react-i18next';
import _ from "underscore";
import { Confirm, Icon, ButtonGroup, ButtonOr, Button, Checkbox, Form, Modal, Popup, Table, Flag, Dimmer, Loader, Image } from "semantic-ui-react";
import { getLocalData, setLocalData, removeLocalData } from "../../common/storage";
import ProtectedInput from "../ProtectedInput";
import PropTypes from "prop-types";
import { toast } from "react-toastify";
import { Clipboard } from "../Clipboard";
import { formatCurrency, formatNumber, isNumeric } from "../../common/Utils";
import "./BulkScanModal.css";
// overrides BarcodeScannerInput audio support
const enableSound = true;
const soundSuccess = new Audio('/audio/scan-success.mp3');
const soundWarning = new Audio('/audio/scan-failure.mp3');
const soundError = new Audio('/audio/scan-error.mp3');

const QuantityMode = {
  Increment: 0,
  Decrement: 1
};

/**
 * Bulk scan parts using a barcode scanner or manual entry
 */
export function BulkScanModal({ onBarcodeLookup, onGetPartMetadata, onInventoryPartSearch, ...rest }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const LocalStorageKey = "scannedPartsSerialized";
  const SettingsContainer = "BulkScanModal";

  const getFromLocalStorage = () => {
    const val = getLocalData(LocalStorageKey, { settingsName: SettingsContainer })
    // on load, force all items to be editable
    if (val) {
      for (var i = 0; i < val.length; i++) {
        val[i].isEditable = true;
      }
    }
    return val;
  };

  const saveToLocalStorage = (items) => {
    setLocalData(LocalStorageKey, items, { settingsName: SettingsContainer });
  };

  const scannedPartsSerialized = getFromLocalStorage() || [];
  const [scannedParts, setScannedParts] = useState(scannedPartsSerialized);
  const scannedPartsRef = useRef(null);
  const [highlightScannedPart, setHighlightScannedPart] = useState(null);
  const [isDirty, setIsDirty] = useState(false);
  const [isOpen, setIsOpen] = useState(rest.isOpen);
  const [bulkScanSaving, setBulkScanSaving] = useState(true);
  const [showBarcodeBeingScanned, setShowBarcodeBeingScanned] = useState(false);
  const [barcodeInput, setBarcodeInput] = useState(rest.barcodeInput);
  const [autoIncrementBinNumber2, setAutoIncrementBinNumber2] = useState(getLocalData('autoIncrementBinNumber2', { settingsName: SettingsContainer }));
  const [rememberLocation, setRememberLocation] = useState(getLocalData('rememberLocation', { settingsName: SettingsContainer }));
  const [confirmClearIsOpen, setConfirmClearIsOpen] = useState(false);
  const [confirmClearContent, setConfirmClearContent] = useState(null);
  const [focusTimerHandle, setFocusTimerHandle] = useState(null);
  const [quantityMode, setQuantityMode] = useState(getLocalData('quantityMode', { settingsName: SettingsContainer }) || QuantityMode.Increment);
  const [focusCheckEnabled, setFocusCheckEnabled] = useState(false);

  useEffect(() => {
    setIsOpen(rest.isOpen);
    if (rest.isOpen) {
      const scannedPartsSerialized = getFromLocalStorage() || [];
      setScannedParts(scannedPartsSerialized);
      setFocusCheckEnabled(true);
      setTimeout(() => {
        document.getElementById('bulkScanModal')?.focus();
      }, 100);
    }
  }, [rest.isOpen]);

  useEffect(() => {
    setBulkScanSaving(rest.isBulkScanSaving);
  }, [rest.isBulkScanSaving]);

  const updateQuantityOnly = useCallback((newQuantity) => {
    const lastPartScanned = _.last(scannedParts);
    if (lastPartScanned) {
      toast.info(`Updated quantity of part '${lastPartScanned.partNumber}' from ${formatNumber(lastPartScanned.quantity)} to ${formatNumber(newQuantity)}`, { autoClose: 10000 });
      setScannedParts(prev => prev.map(part => {
        if (part.partNumber === lastPartScanned.partNumber)
          part.quantity = newQuantity;
        return part;
      }));
    }
  }, [scannedParts]);

  useEffect(() => {
    // perform a quantity update of the last part added
    if (rest.quantityInput > 0) {
      updateQuantityOnly(rest.quantityInput);
    }
  }, [rest.quantityInput]);

  useEffect(() => {
    toast.dismiss();
  }, [rest.isBarcodeReceiving]);


  useEffect(() => {
    const doAutoIncrement = (str) => {
      const parsedNum = parseInt(str) || 0;
      return parsedNum + 1;
    };

    const doAutoDecrement = (str) => {
      const parsedNum = parseInt(str) || 0;
      return parsedNum - 1;
    };

    const updateScannedPartFromInventory = (scannedPart, localInventoryResponse) => {
      const localPart = localInventoryResponse.data;
      scannedPart.existsInInventory = localInventoryResponse.exists;
      scannedPart.description = localPart.description;
      scannedPart.location = localPart.location;
      scannedPart.binNumber = localPart.binNumber;
      if (localPart.binNumber2)
        scannedPart.binNumber2 = localPart.binNumber2; // only overwrite if a value exists
      if (localPart.manufacturer)
        scannedPart.manufacturer = localPart.manufacturer;
      if (localPart.manufacturerPartNumber)
        scannedPart.manufacturerPartNumber = localPart.manufacturerPartNumber;
      const anySupplierNumber = localPart.supplierPartNumber || localPart.digiKeyPartNumber || localPart.mouserPartNumber || localPart.tmePartNumber || localPart.arrowPartNumber || localPart.element14PartNumber;
      if (anySupplierNumber) // only overwrite value if it doesn't exist
        scannedPart.supplierPartNumber = anySupplierNumber;
      scannedPart.originalQuantity = localPart.quantity;
      switch (quantityMode) {
        case QuantityMode.Increment:
          scannedPart.quantity = localPart.quantity + scannedPart.scannedQuantity;
          console.debug(`added quantity ${scannedPart.scannedQuantity} to inventory ${localPart.quantity}`);
          break;
        case QuantityMode.Decrement:
          scannedPart.quantity = localPart.quantity - scannedPart.scannedQuantity;
          if (scannedPart.quantity < 0) {
            scannedPart.quantity = 0;
            scannedPart.warning = `Attempted to reduce quantity by more than is available. Available: ${localPart.quantity} Deduction: -${scannedPart.scannedQuantity}`;
          }
          console.debug(`subtracted quantity ${scannedPart.scannedQuantity} from inventory ${localPart.quantity}`);
          break;
      }
      return scannedPart;
    };

    const handleBarcodeInput = (barcodeparams) => {
      // process barcode input data
      toast.dismiss();

      if (typeof barcodeparams !== 'object') {
        console.error('handleBarcodeInput non-object value!', barcodeparams);
        return;
      }

      const { cleanPartNumber, input } = barcodeparams;
      if (input === undefined) {
        console.error('handleBarcodeInput received empty value!', barcodeparams);
        return;
      }

      // bulk scan add part
      const lastPart = _.last(scannedParts);
      const scannedPart = {
        id: scannedParts.length + 1,
        partNumber: cleanPartNumber,
        originalQuantity: 0,
        quantity: parseInt(input.value.quantity),
        scannedQuantity: parseInt(input.value.quantity),
        location: (rememberLocation && lastPart && lastPart.location) || "",
        binNumber: (rememberLocation && lastPart && lastPart.binNumber) || "",
        binNumber2: (rememberLocation && lastPart && lastPart.binNumber2) || "",
        origin: (input.value.countryOfOrigin && input.value.countryOfOrigin.toLowerCase()) || "",
        supplierPartNumber: input.value.supplierPartNumber || "",
        manufacturerPartNumber: input.value.mfgPartNumber || "",
        description: input.value.description || "",
        salesOrder: input.value.salesOrder || "",
        barcode: input.correctedValue,
        isMetadataFound: false,
        isEditable: false,
        existsInInventory: false,
        barcodeObject: input
      };
      console.debug('part information', scannedPart);
      // stupid hack
      const els = document.getElementsByClassName('noMoreAnimation');
      for (var i = 0; i < els.length; i++)
        els[i].classList.remove('noMoreAnimation');

      if (autoIncrementBinNumber2) {
        // get the last part with a binNumber2 value
        const validParts = _.filter(scannedParts, i => i.binNumber2 === null || typeof i.binNumber2 === 'number' || i.binNumber2.length === 0 || isNumeric(i.binNumber2));
        const lastPartAdded = _.last(validParts);
        if (lastPartAdded)
          scannedPart.binNumber2 = doAutoIncrement(lastPartAdded.binNumber2);
        else
          scannedPart.binNumber2 = 1;
      }

      const existingScannedPart = _.find(scannedParts, { partNumber: cleanPartNumber });
      if (existingScannedPart) {
        //existingPartNumber.quantity += existingPartNumber.scannedQuantity || 1; // add to quantity
        //existingPartNumber.quantity = existingPartNumber.scannedQuantity || 1; // set quantity
        existingScannedPart.isEditable = true;
        saveToLocalStorage(scannedParts);
        setShowBarcodeBeingScanned(false);
        setHighlightScannedPart(existingScannedPart);
        setIsDirty(!isDirty);
        toast.error(t('comp.bulkScanModal.alreadyScanned', "Part {{partNumber}} was already scanned. (qty: {{quantity}})", { partNumber: cleanPartNumber, quantity: existingScannedPart.scannedQuantity }), { autoClose: 60000 });
        if (enableSound) soundError.play();
      } else {
        // fetch metadata on the barcode, don't await, do a background update
        const newScannedParts = [...scannedParts, scannedPart];
        saveToLocalStorage(newScannedParts);
        setShowBarcodeBeingScanned(false);
        setHighlightScannedPart(scannedPart);
        setScannedParts(newScannedParts);
        setIsDirty(!isDirty);
        scannedPartsRef.current = newScannedParts;

        onBarcodeLookup(scannedPart, async (partInfo) => {
          //
          // barcode info found via api
          //

          // update scannedPart with barcode info
          setScannedParts(prevScannedParts => prevScannedParts.map((p) => {
            if (p.partNumber === scannedPart.partNumber) {
              p.basePartNumber = partInfo.basePartNumber;
              p.partTypeId = partInfo.partTypeId;
              p.partType = partInfo.partType;
              p.cost = partInfo.cost;
              p.currency = partInfo.currency;
              p.manufacturer = partInfo.manufacturer;
              p.manufacturerPartNumber = partInfo.manufacturerPartNumber;
              p.supplierPartNumber = partInfo.supplierPartNumber;
              p.status = partInfo.status;
              p.series = partInfo.series;
              p.totalCost = partInfo.totalCost;
              p.productUrl = partInfo.productUrl;
              p.datasheetUrls = partInfo.datasheetUrls;
              p.imageUrl = partInfo.imageUrl;
              p.additionalPartNumbers = partInfo.additionalPartNumbers;
              p.rohsStatus = partInfo.rohsStatus;
              p.reachStatus = partInfo.reachStatus;
              p.packageType = partInfo.packageType;
              p.keywords = partInfo.keywords;
            }
            return p;
          }));

          // does it already exist in inventory?
          const localInventoryResponse = await onInventoryPartSearch(partInfo.basePartNumber);
          const newScannedParts = [...scannedPartsRef.current];
          const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === partInfo.manufacturerPartNumber || i.barcode === scannedPart.barcode);
          if (scannedPartIndex >= 0) {
            const scannedPart = newScannedParts[scannedPartIndex];
            scannedPart.isMetadataFound = true;
            scannedPart.isEditable = true;
            if (localInventoryResponse.exists) {
              // part exists in inventory
              updateScannedPartFromInventory(scannedPart, localInventoryResponse);
            } else {
              // new part
              if (quantityMode === QuantityMode.Decrement) {
                // new parts not valid in decrement mode
              }
              scannedPart.originalQuantity = 0;
              scannedPart.description = partInfo.description;
              if (partInfo.basePartNumber && partInfo.basePartNumber.length > 0)
                scannedPart.partNumber = partInfo.basePartNumber;
            }
            newScannedParts[scannedPartIndex] = scannedPart;
            setScannedParts(newScannedParts);
            setIsDirty(!isDirty);
            saveToLocalStorage(newScannedParts);
            if (enableSound) {
              if (scannedPart.warning)
                soundWarning.play();
              else
                soundSuccess.play();
            }
          }
        }, async (scannedPart) => {
          //
          // no barcode info found
          // 
          const localInventoryResponse = await onInventoryPartSearch(scannedPart.partNumber);
          if (localInventoryResponse.exists) {
            // part exists in inventory
            scannedPart.isEditable = true;
            updateScannedPartFromInventory(scannedPart, localInventoryResponse);
            const newScannedParts = [...scannedPartsRef.current];
            const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === localInventoryResponse.manufacturerPartNumber);
            newScannedParts[scannedPartIndex] = scannedPart;
            setScannedParts(newScannedParts);
            setIsDirty(!isDirty);
            saveToLocalStorage(newScannedParts);
          } else {
            // try searching the part number
            const includeInventorySearch = true;
            await onGetPartMetadata(scannedPart.partNumber, scannedPart, includeInventorySearch).then((response) => {
              const { data } = response;
              if (data.response.parts?.length > 0) {
                // metadata is available, choose the first part
                const firstPart = data.response.parts[0];
                // console.debug('adding part', firstPart);
                const newScannedParts = [...scannedPartsRef.current];
                const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === firstPart.manufacturerPartNumber || i.barcode === scannedPart.barcode);
                if (scannedPartIndex >= 0) {
                  const scannedPart = newScannedParts[scannedPartIndex];
                  scannedPart.isMetadataFound = true;
                  scannedPart.isEditable = true;
                  scannedPart.description = firstPart.description;
                  if (quantityMode === QuantityMode.Decrement) {
                    // new parts not valid in decrement mode
                  }
                  if (firstPart.basePartNumber && firstPart.basePartNumber.length > 0)
                    scannedPart.partNumber = firstPart.basePartNumber;
                  newScannedParts[scannedPartIndex] = scannedPart;
                  setScannedParts(newScannedParts);
                  setIsDirty(!isDirty);
                  saveToLocalStorage(newScannedParts);
                }
              } else {
                const newScannedParts = [...scannedPartsRef.current];
                const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === scannedPart.partNumber);
                if (scannedPartIndex >= 0) {
                  const scannedPart = newScannedParts[scannedPartIndex];
                  scannedPart.isEditable = true;
                  setScannedParts(newScannedParts);
                }
              }
              if (enableSound) soundSuccess.play();
            });
          }
        });
      }
    };
    setBarcodeInput(rest.barcodeInput);
    if (rest.barcodeInput) handleBarcodeInput(rest.barcodeInput);
  }, [rest.barcodeInput]);

  const handleScannedPartChange = (e, control, id) => {
    const newScannedParts = scannedParts.map((c, i) => {
      if (c.id === id) {
        c[control.name] = control.value;
      }
      return c;
    });
    setScannedParts(newScannedParts);
    setIsDirty(!isDirty);
  };

  const ensureNumeric = (e, name, part) => {
    const newScannedParts = scannedParts.map((c, i) => {
      if (c.id === part.id) {
        var val = parseInt(part[name]);
        if (isNaN(val)) c[name] = 0;
        else c[name] = val;
      }
      return c;
    });
    setScannedParts(newScannedParts);
    setIsDirty(!isDirty);
  };

  const deleteScannedPart = useCallback((e, control, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    const scannedPartsExceptDeleted = _.without(scannedParts, _.findWhere(scannedParts, { partNumber: scannedPart.partNumber }));
    saveToLocalStorage(scannedPartsExceptDeleted);
    setScannedParts(scannedPartsExceptDeleted);
    setIsDirty(!isDirty);
    handleFocusTimer(e, control, 100);
  }, [scannedParts]);

  const handleAddBulkScanRow = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    const newRow = {
      id: scannedParts.length + 1,
      basePartNumber: '',
      partNumber: '',
      originalQuantity: 0,
      scannedQuantity: 0,
      cost: 0,
      currency: null,
      lowStockThreshold: 0,
      quantity: 1,
      description: '',
      supplierPartNumber: '',
      manufacturer: '',
      manufacturerPartNumber: '',
      salesOrder: '',
      origin: '',
      location: '',
      binNumber: '',
      binNumber2: '',
      datasheetUrls: [],
      keywords: '',
      productUrl: '',
      imageUrl: '',
      isMetadataFound: false,
      isEditable: true,
      existsInInventory: false,
      partType: 'IC',
      partTypeId: '14' // set a default
    };
    setScannedParts([...scannedParts, newRow]);
    setIsDirty(!isDirty);
    handleFocusTimer(e, control, 1000);
  };

  const handleBulkScanClose = (e) => {
    if (rest.onClose) rest.onClose(e);
    setScannedParts([]);
    setIsDirty(!isDirty);
  };

  const handleBulkScanClear = () => {
    localStorage.removeItem(LocalStorageKey);
    removeLocalData(LocalStorageKey, { settingsName: SettingsContainer });
    setScannedParts([]);
    setIsDirty(!isDirty);
    setConfirmClearIsOpen(false);
    const el = document.getElementById('bulkScanModal');
    if (el) el.focus();
  };

  const handleRememberLocationChange = (e, control) => {
    setRememberLocation(control.checked);
    setLocalData('rememberLocation', control.checked, { settingsName: SettingsContainer });
    if (!control.checked) {
      setAutoIncrementBinNumber2(false);
      setLocalData('autoIncrementBinNumber2', false, { settingsName: SettingsContainer });
    }
  };

  const handleAutoIncrementChange = (e, control) => {
    setAutoIncrementBinNumber2(control.checked);
    setLocalData('autoIncrementBinNumber2', control.checked, { settingsName: SettingsContainer });
    if (control.checked) {
      setRememberLocation(true);
      setLocalData('rememberLocation', true, { settingsName: SettingsContainer });
    }
  };

  const handleSave = async (e) => {
    if (rest.onSave) {
      setFocusCheckEnabled(false);
      let filteredScannedParts = scannedParts;

      // in decrement mode, we won't update parts if they aren't in inventory
      if (quantityMode === QuantityMode.Decrement)
        filteredScannedParts = filteredScannedParts.filter(i => i.existsInInventory === true);

      const mappedParts = filteredScannedParts.map((p) => ({
        ...p,
        // ensure these are numeric
        quantity: Number.isInteger(p.quantity) ? p.quantity : parseInt(p.quantity),
        // ensure these aren't numeric
        binNumber: p.binNumber?.toString() || '',
        binNumber2: p.binNumber2?.toString() || '',
        partTypeId: p.partTypeId?.toString() || '', // expects a string type, but its a number
        keywords: p.keywords?.length > 0 ? p.keywords.join(',') : ''
        }))
      const isSuccess = await rest.onSave(e, mappedParts);
      setFocusCheckEnabled(true);
      if (isSuccess) {
        handleBulkScanClear();
      }
    }
  };

  const confirmClearOpen = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmClearIsOpen(true);
    setConfirmClearContent(
      <p>
        <Trans i18nKey="confirm.clearScan">
          Are you sure you want to clear your scanned results?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmClearClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmClearIsOpen(false);
  };

  const beginAnimation = (highlightScannedPart) => {
    const highlightElement = document.querySelectorAll(`[data-partnumber="${highlightScannedPart.partNumber}"]`);
    if (highlightElement.length > 0) {
      if (!highlightElement[0].classList.contains('noMoreAnimation')) {
        highlightElement[0].classList.add('lastScannedPart');
        highlightElement[0].classList.add('scannedPartAnimation');
        setTimeout(() => {
          highlightElement[0].classList.remove("scannedPartAnimation");
          highlightElement[0].classList.add("noMoreAnimation");
        }, 1000);
      }
    }
  };

  const handleFocusTimer = (e, control, delay = 2000) => {
    const handle = setTimeout(() => {
      const el = document.getElementById('bulkScanModal');
      if (el) el.focus();
    }, delay);
    setFocusTimerHandle(handle);
  };

  const cancelFocusTimer = () => {
    clearTimeout(focusTimerHandle);
  };

  const getQuantitySymbol = () => {
    switch (quantityMode) {
      case QuantityMode.Increment:
        return "+";
      case QuantityMode.Decrement:
        return "-";
    }
    return "?";
  };

  const getQuantityLabel = () => {
    switch (quantityMode) {
      case QuantityMode.Increment:
        return "Added";
      case QuantityMode.Decrement:
        return "Subtracted";
    }
    return "?";
  };

  const handleSetQuantityMode = (e, newQuantityMode) => {
    setQuantityMode(newQuantityMode);
    setLocalData('quantityMode', newQuantityMode, { settingsName: SettingsContainer });
    for (let i = 0; i < scannedParts.length; i++) {
      // iterate all scanned parts and calculate new quantity as increment mode
      const scannedPart = scannedParts[i];
      switch (newQuantityMode) {
        case QuantityMode.Increment:
          scannedPart.quantity = scannedPart.originalQuantity + scannedPart.scannedQuantity;
          break;
        case QuantityMode.Decrement:
          scannedPart.quantity = scannedPart.originalQuantity - scannedPart.scannedQuantity;
          if (scannedPart.quantity < 0) {
            scannedPart.quantity = 0;
            scannedPart.warning = `Attempted to reduce quantity by more than is available. Available: ${scannedPart.originalQuantity} Deduction: -${scannedPart.scannedQuantity}`;
          }
          break;
      }
    }
    setScannedParts([...scannedParts]);
    saveToLocalStorage(scannedParts);
  };

  const getInventoryState = (p) => {
    if (p.existsInInventory) {
      return <Popup wide content={<p>{t('message.scanPartExists', "Part exists in inventory, quantity specified by the label will be added to your inventory.")}</p>} trigger={<Icon name="check circle" color="blue" size="big" />} />;
    }
    if (quantityMode == QuantityMode.Decrement)
      return <Popup wide content={<p>{t('message.scanPartNotExists', "Part is not in inventory.")}</p>} trigger={<Icon name="times circle" color="red" size="big" />} />;
    
    return <Popup wide content={<p>{t('message.scanPartNew', "New part will be added to your inventory.")}</p>} trigger={<Icon name="plus circle" color="green" size="big" />} />;
  };

  const getPartState = (p) => {
    if (!p.isEditable) return <div className="ui loader inline static small" />;
    if (p.existsInInventory) {
      // exists in inventory
      if (p.warning) {
        return <Popup wide content={<p>{p.warning}</p>} trigger={<Icon name="warning sign" color="yellow" size="big" />} />;
      }

      return (<></>);
    } else {
      if (quantityMode === QuantityMode.Decrement) {
        // invalid, can't decrement the quantity if it doesn't exist in inventory
        return <Popup wide content={<p>{t('message.scanPartInvalid', "Part does not exist in inventory so quantity will be ignored (quantity decrement mode only).")}</p>} trigger={<Icon name="times circle" color="red" size="big" />} />;
      }
    }

    if (!p.isMetadataFound) {
      // no metadata found
      return <Popup wide content={<p>{t('message.noPartMetadata', "No part metadata found!")}</p>} trigger={<Icon name="warning sign" color="yellow" size="big" />} />;
    }
  };

  const handleViewPart = (e, part) => {
    e.preventDefault();
    e.stopPropagation();

    if (part.existsInInventory) {
      window.open(`/inventory/${part.partNumber}`, '_blank', 'noopener noreferrer');
      return;
    }

    toast.error('Part not in inventory.');
  };

  const handleViewProduct = (e, part) => {
    e.preventDefault();
    e.stopPropagation();

    if (part.productUrl) {
      window.open(part.productUrl, '_blank', 'noopener noreferrer');
      return;
    }

    toast.error('No product page available.');
  };

  const renderScannedParts = (highlightScannedPart) => {
    if (highlightScannedPart) {
      // reset the css highlight animation
      if (highlightScannedPart.isEditable) {
        beginAnimation(highlightScannedPart);
      }
    }
    return (
      <Table compact celled striped size="small">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.status', "Status")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.existsInInventory', "In Inventory")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.preview', "Preview")}</Table.HeaderCell>
            <Table.HeaderCell><Popup content={<p>{t('comp.bulkScanModal.popup.partAddedUpdated', "The part number being added/updated.")}</p>} trigger={<span>{t('label.part', "Part")}</span>} /></Table.HeaderCell>
            <Table.HeaderCell><Popup content={<p>{t('comp.bulkScanModal.popup.currentQuantity', "The quantity currently in your inventory.")}</p>} trigger={<span>{t('label.stock', "Stock")}</span>} /></Table.HeaderCell>
            <Table.HeaderCell><Popup content={<p>{t('comp.bulkScanModal.popup.newQuantity', "The quantity your inventory will be set to.")}</p>} trigger={<span>{t('label.newQuantityShort', "New Qty")}</span>} /></Table.HeaderCell>
            <Table.HeaderCell>{t('label.description', "Description")}</Table.HeaderCell>
            <Table.HeaderCell><Popup content={<p>{t('comp.bulkScanModal.popup.partOrigin', "The country of origin of the manufactured part.")}</p>} trigger={<span>{t('label.origin', "Origin")}</span>} /></Table.HeaderCell>
            <Table.HeaderCell>{t('label.location', "Location")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber', "Bin Number")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber2', "Bin Number 2")}</Table.HeaderCell>
            <Table.HeaderCell></Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {_.sortBy(scannedParts, (num) => num).reverse().map((p, index) => (
            <React.Fragment key={index}>
              <Table.Row
                key={index}
                data-partnumber={p.partNumber}
                className={`scannedRow ${p.existsInInventory ? 'exists' : ''} ${(!p.existsInInventory && quantityMode === QuantityMode.Decrement) ? 'invalid' : ''}`}
              >
                <Table.Cell textAlign="center" style={{ verticalAlign: 'middle', width: '50px' }}>
                  {getPartState(p)}
                </Table.Cell>
                <Table.Cell textAlign="center" style={{ verticalAlign: 'middle', width: '50px' }}>
                  {getInventoryState(p)}
                </Table.Cell>
                <Table.Cell textAlign="center">
                  {!((!p.existsInInventory && quantityMode === QuantityMode.Decrement)) &&
                  <Popup
                    wide='very'
                    hoverable
                    content={<div>
                      <div style={{ display: 'flex', flexDirection: 'row', alignItems: 'stretch', width: '100%' }}>
                        <div style={{ flex: '1' }}><b>{getQuantityLabel()} Qty:</b> <span style={{ color: 'red' }}>+{formatNumber(p.scannedQuantity)}</span></div>
                        <div style={{ flex: '1' }}><b>In Stock Qty:</b> {formatNumber(p.originalQuantity)}</div>
                        <div style={{ flex: '1' }}><b>New Qty:</b> {formatNumber(p.quantity)}</div>
                      </div>
                      {p.manufacturerPartNumber?.length > 0 && <div><b>Manufacturer Part Number:</b> {p.manufacturerPartNumber} <Clipboard text={p.manufacturerPartNumber} /></div>}
                      {p.manufacturer?.length > 0 && <div><b>Manufacturer:</b> {p.manufacturer} <Clipboard text={p.manufacturer} /></div>}
                      {p.packageType?.length > 0 && <div><b>Package:</b> {p.packageType} <Clipboard text={p.packageType} /></div>}
                      {p.supplierPartNumber?.length > 0 && <div><b>Supplier Part Number:</b> {p.supplierPartNumber} <Clipboard text={p.supplierPartNumber} /></div>}
                      {p.salesOrder?.length > 0 && <div><b>Sales Order:</b> {p.salesOrder} <Clipboard text={p.salesOrder} /></div>}
                      {p.cost && <div><b>Cost:</b> {formatCurrency(p.cost, p.currency || 'USD')}</div>}
                      {p.description?.length > 0 && <div><b>Description:</b> <Clipboard text={p.description} /><br />
                        <pre>{p.description}</pre>
                      </div>}
                      {p.keywords?.length > 0 && <div><b>Keywords:</b> <Clipboard text={p.keywords.join(' ')} /><br />
                        <pre>{p.keywords.join(' ')}</pre>
                      </div>}
                      {p.imageUrl?.length > 0 && <div><Image src={p.imageUrl} size='tiny' /></div>}
                      <div>
                        {p.existsInInventory && <div><Link onClick={(e) => handleViewPart(e, p)} size='tiny' target="_blank">{t('label.viewPart', "View Part")}</Link></div>}
                        {p.productUrl && <div><Link onClick={(e) => handleViewProduct(e, p)} size='tiny' target="_blank" disabled={!(p.productUrl?.length > 0)}>{t('label.viewProduct', "View Product")}</Link></div>}
                      </div>
                    </div>}
                    trigger={<div>
                      {p.imageUrl?.length > 0 && <Image src={p.imageUrl} size='tiny' style={{ maxHeight: '30px', width: 'auto' }} />}
                      <div><Link onClick={(e) => handleViewPart(e, p)} size='mini' target="_blank" disabled={!p.existsInInventory}>{t('label.view', "View Part")}</Link></div>
                      {p.productUrl && <div><Link onClick={(e) => handleViewProduct(e, p)} size='tiny' target="_blank">{t('label.viewProduct', "View Product")}</Link></div>}
                    </div>
                    }
                  />}
                </Table.Cell>
                <Table.Cell collapsing>
                  <ProtectedInput
                    hideIcon
                    name="partNumber"
                    value={p.partNumber || ''}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                    onFocus={cancelFocusTimer}
                  />
                </Table.Cell>
                <Table.Cell collapsing textAlign="center" style={{ lineHeight: '2.5em', fontWeight: '500', color: '#666' }}>
                  <Popup content={<p>Original quantity in inventory</p>} trigger={<div className="quantity stock">{formatNumber(p.originalQuantity) || '-'}</div>} />
                  <Popup content={<p>Scanned quantity</p>} trigger={<div className="quantity added">{getQuantitySymbol()}{formatNumber(p.scannedQuantity) || '-'}</div>} />
                </Table.Cell>
                <Table.Cell collapsing textAlign="center">
                  <ProtectedInput
                    hideIcon
                    value={p.quantity}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    name="quantity"
                    disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                    style={{ width: '75px' }}
                    onBlur={e => ensureNumeric(e, "quantity", p)}
                    onFocus={cancelFocusTimer}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <Popup
                    wide='very'
                    hoverable
                    content={p.description || ''}
                    trigger={<div><ProtectedInput
                      hideIcon
                      name="description"
                      value={p.description || ''}
                      onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                      disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                      onFocus={cancelFocusTimer}
                    /></div>
                    }
                  />
                </Table.Cell>
                <Table.Cell collapsing textAlign="center" verticalAlign="middle">
                  <Flag name={p.origin || ""} />
                </Table.Cell>
                <Table.Cell collapsing>
                  <ProtectedInput
                    hideIcon
                    placeholder={t('page.inventory.placeholder.location', "Home lab")}
                    value={p.location || ''}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    name="location"
                    disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                    style={{ width: '120px' }}
                    onFocus={cancelFocusTimer}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <ProtectedInput
                    hideIcon
                    placeholder=""
                    value={p.binNumber || ''}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    name="binNumber"
                    disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                    style={{ width: '120px' }}
                    onFocus={cancelFocusTimer}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <ProtectedInput
                    hideIcon
                    placeholder=""
                    value={p.binNumber2 || ''}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    name="binNumber2"
                    disabled={!p.isEditable || ((!p.existsInInventory && quantityMode === QuantityMode.Decrement))}
                    style={{ width: '120px' }}
                    onFocus={cancelFocusTimer}
                  />
                </Table.Cell>
                <Table.Cell collapsing textAlign="center" verticalAlign="middle">
                  <Button type="button" circular size="mini" icon="delete" title="Delete" onClick={(e, control) => deleteScannedPart(e, control, p)} />
                </Table.Cell>
              </Table.Row>
              {(!p.existsInInventory && quantityMode === QuantityMode.Decrement)
                  ? <Table.Row key={`${index}-warning`} className="row-warning">
                    <Table.Cell colSpan={11} textAlign="center">
                      Cannot edit non-existant part in Quantity Decrement mode.
                    </Table.Cell>
                  </Table.Row>
              : <></>
              }
            </React.Fragment>
          ))}
        </Table.Body>
      </Table>
    );
  };

  return (
    <>
      <Confirm
        className="confirm"
        header={t('confirm.header.clearScan', "Clear Scanned Parts")}
        open={confirmClearIsOpen}
        onCancel={confirmClearClose}
        onConfirm={handleBulkScanClear}
        content={confirmClearContent}
      />
      <Modal centered open={isOpen} onClose={handleBulkScanClose} className="bulkScanModal" id="bulkScanModal" tabIndex={1}>
        {focusCheckEnabled && <Modal.Dimmer blurring className="focusCheck" centered>
          <div>
            <span>{t('comp.bulkScanModal.notFocused', "Click on this dialog to continue scanning")}</span>
            <div style={{ marginTop: '20px' }}><Icon name="warning sign" color="red" size="huge" /></div>
            <span className="small">{t('comp.bulkScanModal.mustBeInFocus', "This dialog must be in focus to receive barcode input.")}</span>
          </div>
        </Modal.Dimmer>}

        <Dimmer.Dimmable as={Modal.Content} scrolling>
          <Dimmer active={bulkScanSaving} inverted><Loader /></Dimmer>
          <div className={`animated-border ${rest.isBarcodeReceiving ? 'visible' : ''}`}>
            <div style={{ width: "200px", height: "75px", margin: "auto" }}>
              <div className="anim-box">
                <div className={`scanner ${rest.isBarcodeReceiving ? '' : 'animated'}`} />
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-md"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-md"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-md"></div>
                <div className="anim-item anim-item-md"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-md"></div>
                <div className="anim-item anim-item-lg"></div>
                <div className="anim-item anim-item-sm"></div>
                <div className="anim-item anim-item-md"></div>
              </div>
            </div>
            <p>{rest.isBarcodeReceiving ? t('comp.bulkScanModal.processing', "Processing...") : scannedParts.length > 0 ? t('comp.bulkScanModal.ready', "Ready.") : t('comp.bulkScanModal.startScanning', "Start scanning parts...")}</p>
          </div>
          <div style={{ textAlign: "center" }}>
            <Form>
              <div style={{ textAlign: 'right', height: '50px', width: '100%', marginBottom: '2px', verticalAlign: 'bottom' }}>
                <Form.Group style={{ justifyContent: 'end' }}>
                  <Form.Field style={{ margin: 'auto 0', flex: '1', textAlign: 'left', fontSize: '0.9em' }}>
                    {t('label.partsScanned', "({{count}}) parts", { count: scannedParts?.length || 0 })}.
                  </Form.Field>
                  <Form.Field style={{ margin: 'auto 0' }}>
                    <span style={{ marginRight: '15px' }}>Quantity mode</span>
                    <Popup
                      hoverable
                      content={<p>{t('comp.bulkScanModal.popup.quantityMode', "Choose if you want to increment or decrement the quantity of the scanned inventory.")}</p>}
                      trigger={<ButtonGroup>
                        <Button type="button" size="mini" positive={quantityMode === QuantityMode.Increment ? true : false} onClick={(e) => handleSetQuantityMode(e, QuantityMode.Increment)}>Increment</Button>
                        <ButtonOr />
                        <Button type="button" size="mini" positive={quantityMode === QuantityMode.Decrement ? true : false} onClick={(e) => handleSetQuantityMode(e, QuantityMode.Decrement)}>Decrement</Button>
                      </ButtonGroup>}
                    />
                  </Form.Field>
                  <Form.Field style={{ margin: 'auto 0' }}>
                    <div style={{ textAlign: 'left', margin: '0', padding: '0', scale: '0.8' }}>
                      <Popup
                        content={<p>{t('comp.bulkScanModal.popup.rememberLocation', "When selected, repeat the location of the last added part.")}</p>}
                        trigger={<Checkbox toggle label={t('comp.bulkScanModal.rememberLocation', "Remember Location")} checked={rememberLocation} onChange={handleRememberLocationChange} onFocus={(e, control) => handleFocusTimer(e, control, 100)} />}
                      />
                    </div>
                    <div style={{ textAlign: 'left', margin: '0', padding: '0', scale: '0.8' }}>
                      <Popup
                        hoverable
                        content={<p>{t('comp.bulkScanModal.popup.autoIncrement', "When selected, Bin Number 2 will auto increment if it's a numeric data type.")}</p>}
                        trigger={<Checkbox toggle label={t('comp.bulkScanModal.autoIncrementBinNumber2', "Auto Increment Bin Number 2")} checked={autoIncrementBinNumber2} onChange={handleAutoIncrementChange} onFocus={(e, control) => handleFocusTimer(e, control, 100)} />}
                      />
                    </div>
                  </Form.Field>
                  <Form.Field>
                    <Popup
                      wide
                      content={<p>Add a row for manual data entry.</p>}
                      trigger={<Button size='mini' onClick={handleAddBulkScanRow}><Icon name="plus" color="green" /> {t('button.manualAdd', "Manual Add")}</Button>}
                    />
                    
                  </Form.Field>
                </Form.Group>
              </div>
              {isOpen && renderScannedParts(highlightScannedPart)}
            </Form>
          </div>
        </Dimmer.Dimmable>
        <Modal.Actions>
          <Button onClick={confirmClearOpen} disabled={scannedParts?.length === 0}>{t('button.clear', "Clear")}</Button>
          <Button onClick={handleBulkScanClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleSave} disabled={bulkScanSaving}>
            {t('button.save', "Save")}
          </Button>
        </Modal.Actions>
      </Modal>
    </>);
};

BulkScanModal.propTypes = {
  /** Event handler to call when saving scanned parts */
  onSave: PropTypes.func.isRequired,
  /** Event handler to call when needing part metadata */
  onBarcodeLookup: PropTypes.func.isRequired,
  /** Event handler to call when needing part info */
  onGetPartMetadata: PropTypes.func.isRequired,
  /** Event handler to call when needing local inventory lookup */
  onInventoryPartSearch: PropTypes.func.isRequired,
  /** Event handler when a part is scanned */
  onAdd: PropTypes.func,
  /** Event handler when a part is removed */
  onRemove: PropTypes.func,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set the barcode input when received */
  barcodeInput: PropTypes.object,
  /** Set the quantity input (for quantity only updates) */
  quantityInput: PropTypes.number,
  /** Set this to true to open model */
  isOpen: PropTypes.bool,
  isBulkScanSaving: PropTypes.bool,
  /** Set this to notify the modal a barcode is being received */
  isBarcodeReceiving: PropTypes.bool,
};
