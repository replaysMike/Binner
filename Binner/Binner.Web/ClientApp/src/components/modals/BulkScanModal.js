import React, { useState, useEffect, useRef, useCallback } from "react";
import { Link } from "react-router-dom";
import { useTranslation, Trans } from 'react-i18next';
import _ from "underscore";
import { Confirm, Icon, Button, Checkbox, Form, Modal, Popup, Table, Flag, Dimmer, Loader, Image } from "semantic-ui-react";
import { getLocalData, setLocalData, removeLocalData } from "../../common/storage";
import ProtectedInput from "../ProtectedInput";
import PropTypes from "prop-types";
import { toast } from "react-toastify";
import { Clipboard } from "../Clipboard";
import { formatCurrency, getCurrencySymbol } from "../../common/Utils";
import "./BulkScanModal.css";
// overrides BarcodeScannerInput audio support
const enableSound = true;
const soundSuccess = new Audio('/audio/scan-success.mp3');
const soundFailure = new Audio('/audio/scan-failure.mp3');
const soundError = new Audio('/audio/scan-error.mp3');

/**
 * Bulk scan parts using a barcode scanner or manual entry
 */
export function BulkScanModal({ onBarcodeLookup, onGetPartMetadata, onInventoryPartSearch, ...rest }) {
  const { t } = useTranslation();
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
  const [autoIncrement, setAutoIncrement] = useState(true);
  const [rememberLocation, setRememberLocation] = useState(true);
  const [confirmClearIsOpen, setConfirmClearIsOpen] = useState(false);
  const [confirmClearContent, setConfirmClearContent] = useState(null);
  const [focusTimerHandle, setFocusTimerHandle] = useState(null);

  useEffect(() => {
    setIsOpen(rest.isOpen);
    if (rest.isOpen) {
      const scannedPartsSerialized = getFromLocalStorage() || [];
      setScannedParts(scannedPartsSerialized);
      setTimeout(() => {
        document.getElementById('bulkScanModal')?.focus();
      }, 100);
    }
  }, [rest.isOpen]);

  useEffect(() => {
    setBulkScanSaving(rest.isBulkScanSaving);
  }, [rest.isBulkScanSaving]);

  useEffect(() => {
    const doAutoIncrement = (str) => {
      let parsedNum = parseInt(str);
      if (!isNaN(parsedNum)) {
        return parsedNum + 1;
      }
      return str;
    };
    const handleBarcodeInput = (barcodeparams) => {
      // process barcode input data
      toast.dismiss();
      const { cleanPartNumber, input } = barcodeparams;

      // bulk scan add part
      const lastPart = _.last(scannedParts);
      console.debug('input', input.value);
      const scannedPart = {
        id: scannedParts.length + 1,
        partNumber: cleanPartNumber,
        originalQuantity: 0,
        quantity: parseInt(input.value.quantity || "1"),
        scannedQuantity: parseInt(input.value.quantity || "1"),
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
        dateAdded: new Date().getTime(),
        barcodeObject: input
      };
      console.debug('scannedPart', scannedPart);
      // stupid hack
      const els = document.getElementsByClassName('noMoreAnimation');
      for (var i = 0; i < els.length; i++)
        els[i].classList.remove('noMoreAnimation');

      if (scannedPart.binNumber2 && autoIncrement)
        scannedPart.binNumber2 = doAutoIncrement(scannedPart.binNumber2);

      const existingPartNumber = _.find(scannedParts, { partNumber: cleanPartNumber });
      if (existingPartNumber) {
        //existingPartNumber.quantity += existingPartNumber.scannedQuantity || 1; // add to quantity
        existingPartNumber.quantity = existingPartNumber.scannedQuantity || 1; // set quantity
        existingPartNumber.isEditable = true;
        saveToLocalStorage(scannedParts);
        setShowBarcodeBeingScanned(false);
        setHighlightScannedPart(existingPartNumber);
        setIsDirty(!isDirty);
        toast.error(t('comp.bulkScanModal.alreadyScanned', "Part {{partNumber}} was already scanned. (qty: {{quantity}})", { partNumber: cleanPartNumber, quantity: existingPartNumber.scannedQuantity }), { autoClose: 60000 });
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
          // barcode found

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
            if (localInventoryResponse.exists) {
              // part exists in inventory
              scannedPart.existsInInventory = localInventoryResponse.exists;
              scannedPart.description = localInventoryResponse.data[0].description;
              scannedPart.location = localInventoryResponse.data[0].location;
              scannedPart.binNumber = localInventoryResponse.data[0].binNumber;
              scannedPart.binNumber2 = localInventoryResponse.data[0].binNumber2;
              if (localInventoryResponse.data[0].manufacturer)
                scannedPart.manufacturer = localInventoryResponse.data[0].manufacturer;
              if (localInventoryResponse.data[0].manufacturerPartNumber)
                scannedPart.manufacturerPartNumber = localInventoryResponse.data[0].manufacturerPartNumber;
              const anySupplierNumber = localInventoryResponse.data[0].supplierPartNumber || localInventoryResponse.data[0].digiKeyPartNumber || localInventoryResponse.data[0].mouserPartNumber || localInventoryResponse.data[0].tmePartNumber || localInventoryResponse.data[0].arrowPartNumber;
              if (anySupplierNumber)
                scannedPart.supplierPartNumber = localInventoryResponse.data[0].supplierPartNumber || localInventoryResponse.data[0].digiKeyPartNumber || localInventoryResponse.data[0].mouserPartNumber;
              scannedPart.originalQuantity = localInventoryResponse.data[0].quantity;
              scannedPart.quantity = localInventoryResponse.data[0].quantity + scannedPart.scannedQuantity;
            } else {
              // new part
              scannedPart.originalQuantity = 0;
              scannedPart.description = partInfo.description;
              if (partInfo.basePartNumber && partInfo.basePartNumber.length > 0)
                scannedPart.partNumber = partInfo.basePartNumber;
            }
            scannedPart.isMetadataFound = true;
            scannedPart.isEditable = true;
            newScannedParts[scannedPartIndex] = scannedPart;
            setScannedParts(newScannedParts);
            setIsDirty(!isDirty);
            saveToLocalStorage(newScannedParts);
            if (enableSound) soundSuccess.play();
          }
        }, async (scannedPart) => {
          // no barcode info found, try searching the part number
          const includeInventorySearch = true;
          await onGetPartMetadata(scannedPart.partNumber, scannedPart, includeInventorySearch).then((response) => {
            const { data } = response;
            if (data.response.parts?.length > 0) {
              const firstPart = data.response.parts[0];
              // console.debug('adding part', firstPart);
              const newScannedParts = [...scannedPartsRef.current];
              const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === firstPart.manufacturerPartNumber || i.barcode === scannedPart.barcode);
              if (scannedPartIndex >= 0) {
                const scannedPart = newScannedParts[scannedPartIndex];
                scannedPart.isMetadataFound = true;
                scannedPart.isEditable = true;
                scannedPart.description = firstPart.description;
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
    setScannedParts([...scannedParts, {
      id: scannedParts.length + 1,
      basePartNumber: '',
      partNumber: '',
      originalQuantity: 0,
      scannedQuantity: 0,
      quantity: 1,
      description: '',
      supplierPartNumber: '',
      manufacturerPartNumber: '',
      salesOrder: '',
      origin: '',
      location: '',
      binNumber: '',
      binNumber2: '',
      isMetadataFound: false,
      isEditable: true,
      existsInInventory: false,
      dateAdded: new Date().getTime(),
    }]);
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
  };

  const handleRememberLocationChange = (e, control) => {
    setRememberLocation(control.checked);
    if (!control.checked) setAutoIncrement(false);
  };

  const handleAutoIncrementChange = (e, control) => {
    setAutoIncrement(control.checked);
    if (control.checked) setRememberLocation(true);
  };

  const handleOnSave = async (e) => {
    if (rest.onSave) {
      const mappedParts = scannedParts.map((p) => ({
        ...p,
        // ensure these are numeric
        quantity: Number.isInteger(p.quantity) ? p.quantity : parseInt(p.quantity),
        // ensure these aren't numeric
        binNumber: p.binNumber?.toString() || '',
        binNumber2: p.binNumber2?.toString() || '',
      }))
      const isSuccess = await rest.onSave(e, mappedParts);
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
            <Table.HeaderCell></Table.HeaderCell>
            <Table.HeaderCell></Table.HeaderCell>
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
            <Table.Row
              key={index}
              data-partnumber={p.partNumber}
              className={`${p.existsInInventory ? 'exists' : ''} `}
            >
              <Table.Cell textAlign="center" style={{ verticalAlign: 'middle', width: '50px' }}>
                {!p.isEditable
                  ? <div className="ui loader inline static small" />
                  : p.existsInInventory
                    ? <Popup content={<p>{t('message.scanPartExists', "Part exists in inventory, quantity specified by the label will be added to your inventory.")}</p>} trigger={<Icon name="check circle" color="blue" size="big" />} />
                    : p.isMetadataFound
                      ? <Popup content={<p>{t('message.scanPartNew', "New part will be added to your inventory.")}</p>} trigger={<Icon name="check circle" color="green" size="big" />} />
                      : <Popup content={<p>{t('message.noPartMetadata', "No part metadata found!")}</p>} trigger={<Icon name="warning sign" color="yellow" size="big" />} />
                }
              </Table.Cell>
              <Table.Cell textAlign="center">
                <Popup
                  wide='very'
                  hoverable
                  content={<div>
                    <div style={{ display: 'flex', flexDirection: 'row', alignItems: 'stretch', width: '100%' }}>
                      <div style={{ flex: '1' }}><b>Added Qty:</b> <span style={{ color: 'red' }}>+{p.scannedQuantity}</span></div>
                      <div style={{ flex: '1' }}><b>In Stock Qty:</b> {p.originalQuantity}</div>
                      <div style={{ flex: '1' }}><b>New Qty:</b> {p.quantity}</div>
                    </div>
                    {p.manufacturerPartNumber?.length > 0 && <div><b>Manufacturer Part Number:</b> {p.manufacturerPartNumber} <Clipboard text={p.manufacturerPartNumber} /></div>}
                    {p.manufacturer?.length > 0 && <div><b>Manufacturer:</b> {p.manufacturer} <Clipboard text={p.manufacturer} /></div>}
                    {p.packageType?.length > 0 && <div><b>Package:</b> {p.packageType} <Clipboard text={p.packageType} /></div>}
                    {p.supplierPartNumber?.length > 0 && <div><b>Supplier Part Number:</b> {p.supplierPartNumber} <Clipboard text={p.supplierPartNumber} /></div>}
                    {p.salesOrder?.length > 0 && <div><b>Sales Order:</b> {p.salesOrder} <Clipboard text={p.salesOrder} /></div>}
                    {p.cost && <div><b>Cost:</b> {formatCurrency(p.cost, p.currency || 'USD')}</div>}
                    {p.description.length > 0 && <div><b>Description:</b> <Clipboard text={p.description} /><br />
                      <pre>{p.description}</pre>
                    </div>}
                    {p.keywords?.length > 0 && <div><b>Keywords:</b> <Clipboard text={p.keywords.join(' ')} /><br />
                      <pre>{p.keywords.join(' ')}</pre>
                    </div>}
                    {p.imageUrl?.length > 0
                      ? <div><Link to={p.productUrl} size='tiny' target="_blank"><Image src={p.imageUrl} size='tiny' /></Link></div>
                      : <div><Link to={p.productUrl} size='tiny' target="_blank">{t('label.view', "View")}</Link></div>}
                  </div>}
                  trigger={<div>
                    {p.imageUrl?.length > 0
                      ? <Link to={p.productUrl} size='mini' target="_blank"><Image src={p.imageUrl} size='tiny' style={{ maxHeight: '30px', width: 'auto' }} /></Link>
                      : <Link to={p.productUrl} size='mini' target="_blank">{t('label.view', "View")}</Link>}
                  </div>
                  }
                />

                
              </Table.Cell>
              <Table.Cell collapsing>
                <ProtectedInput
                    hideIcon
                    name="partNumber"
                    value={p.partNumber || ''}
                    onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                    disabled={!p.isEditable}
                    onFocus={cancelFocusTimer}
                  />
              </Table.Cell>
              <Table.Cell collapsing textAlign="center" style={{ lineHeight: '2.5em', fontWeight: '500', color: '#666' }}>
                {p.originalQuantity || '-'}
              </Table.Cell>
              <Table.Cell collapsing textAlign="center">
                <ProtectedInput
                  hideIcon
                  value={p.quantity || '1'}
                  onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
                  name="quantity"
                  disabled={!p.isEditable}
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
                    disabled={!p.isEditable}
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
                  disabled={!p.isEditable}
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
                  disabled={!p.isEditable}
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
                  disabled={!p.isEditable}
                  style={{ width: '120px' }}
                  onFocus={cancelFocusTimer}
                />
              </Table.Cell>
              <Table.Cell collapsing textAlign="center" verticalAlign="middle">
                <Button type="button" circular size="mini" icon="delete" title="Delete" onClick={(e, control) => deleteScannedPart(e, control, p)} />
              </Table.Cell>
            </Table.Row>
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
          <Modal.Dimmer blurring className="focusCheck" centered>
            <div>
              <span>{t('comp.bulkScanModal.notFocused', "Click on this window to give it focus")}</span>
              <div style={{marginTop: '20px'}}><Icon name="warning sign" color="red" size="huge" /></div>
            </div>
          </Modal.Dimmer>
          <Modal.Header>{t('page.inventory.bulkScan', "Bulk Scan")}</Modal.Header>
          <Dimmer.Dimmable as={Modal.Content} scrolling>
            <Dimmer active={bulkScanSaving} inverted><Loader /></Dimmer>
            <div style={{ width: "200px", height: "100px", margin: "auto" }}>
              <div className="anim-box">
                <div className="scanner animated" />
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
            <div style={{ textAlign: "center" }}>
              <Form>
                <p>{t('page.inventory.startScanning', "Start scanning parts...")}</p>
                <div style={{ textAlign: 'right', height: '35px', width: '100%', marginBottom: '2px' }}>
                  <Form.Group style={{ justifyContent: 'end' }}>
                    <Form.Field style={{ margin: 'auto 0' }}><Popup hoverable content={<p>{t('comp.bulkScanModal.popup.autoIncrement', "Auto increment of Bin Number 2")}</p>} trigger={<Checkbox toggle label={t('comp.bulkScanModal.autoIncrement', "Auto Increment")} checked={autoIncrement} onChange={handleAutoIncrementChange} onFocus={(e, control) => handleFocusTimer(e, control, 100)} style={{ scale: '0.8' }} />} /></Form.Field>
                    <Form.Field style={{ margin: 'auto 0' }}><Popup content={<p>{t('comp.bulkScanModal.popup.rememberLocation', "Repeat the location of each added part")}</p>} trigger={<Checkbox toggle label={t('comp.bulkScanModal.rememberLocation', "Remember Location")} checked={rememberLocation} onChange={handleRememberLocationChange} onFocus={(e, control) => handleFocusTimer(e, control, 100)} style={{ scale: '0.8' }} />} /></Form.Field>
                    <Form.Field>
                      <Button size='mini' onClick={handleAddBulkScanRow}><Icon name="plus" color="green" /> {t('button.manualAdd', "Manual Add")}</Button>
                    </Form.Field>
                  </Form.Group>
                </div>
                {isOpen && renderScannedParts(highlightScannedPart)}
              </Form>
            </div>
          </Dimmer.Dimmable>
          <Modal.Actions>
            <Button onClick={confirmClearOpen} disabled={scannedParts.length === 0}>{t('button.clear', "Clear")}</Button>
            <Button onClick={handleBulkScanClose}>{t('button.cancel', "Cancel")}</Button>
            <Button primary onClick={handleOnSave} disabled={bulkScanSaving}>
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
  /** Set this to true to open model */
  isOpen: PropTypes.bool,
  isBulkScanSaving: PropTypes.bool,
};
