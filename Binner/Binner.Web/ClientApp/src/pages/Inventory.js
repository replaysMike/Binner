import React, { useState, useEffect, useMemo, useRef } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { useTranslation, Trans } from 'react-i18next';
import ReactDOM from "react-dom";
import PropTypes from "prop-types";
import _ from "underscore";
import debounce from "lodash.debounce";
import { Icon, Input, Label, Button, TextArea, Image, Form, Table, Segment, Popup, Modal, Header, Confirm, Grid, Checkbox, Dropdown} from "semantic-ui-react";
import ProtectedInput from "../components/ProtectedInput";
import NumberPicker from "../components/NumberPicker";
import PartTypeSelector from "../components/PartTypeSelector";
import { FormHeader } from "../components/FormHeader";
import { ChooseAlternatePartModal } from "../components/ChooseAlternatePartModal";
import { PartMedia } from "../components/PartMedia";
import { BulkScanModal } from "../components/BulkScanModal";
import { BulkScanIcon } from "../components/BulkScanIcon";
import { RecentParts } from "../components/RecentParts";
import { PartSuppliers } from "../components/PartSuppliers";
import { fetchApi } from "../common/fetchApi";
import { formatNumber } from "../common/Utils";
import { toast } from "react-toastify";
import { getPartTypeId } from "../common/partTypes";
import { getAuthToken, getImagesToken } from "../common/authentication";
import { StoredFileType } from "../common/StoredFileType";
import { GetAdvancedTypeDropdown } from "../common/Types";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import customEvents from '../common/customEvents';
import { Currencies } from "../common/currency";
import "./Inventory.css";

const IcPartType = 14;

export function Inventory(props) {
  const maxRecentAddedParts = 10;
  const { t } = useTranslation();
  const navigate = useNavigate();
  const defaultViewPreferences = JSON.parse(localStorage.getItem("viewPreferences")) || {
    helpDisabled: false,
    lastPartTypeId: IcPartType, // IC
    lastMountingTypeId: "0", // None
    lastQuantity: 1,
    lastProjectId: null,
    lastLocation: "",
    lastBinNumber: "",
    lastBinNumber2: "",
    lowStockThreshold: 10,
    rememberLast: true
  };

  const [inputPartNumber, setInputPartNumber] = useState("");
  const [suggestedPartNumber, setSuggestedPartNumber] = useState(null);
  const [viewPreferences, setViewPreferences] = useState(defaultViewPreferences);
  const [infoResponse, setInfoResponse] = useState({});
  
  const pageHasParameters = (props.params && props.params.partNumber !== undefined && props.params.partNumber.length > 0);
  const defaultPart = {
    partId: 0,
    partNumber: props.params.partNumber || "",
    allowPotentialDuplicate: false,
    quantity: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastQuantity) + "",
    lowStockThreshold: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lowStockThreshold) + "",
    partTypeId: (!pageHasParameters && viewPreferences.rememberLast && (viewPreferences.lastPartTypeId || IcPartType)) || 0 ,
    mountingTypeId: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastMountingTypeId) || 0 + "",
    packageType: "",
    keywords: "",
    description: "",
    datasheetUrl: "",
    digiKeyPartNumber: "",
    mouserPartNumber: "",
    arrowPartNumber: "",
    location: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastLocation) || "",
    binNumber: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastBinNumber) || "",
    binNumber2: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastBinNumber2) || "",
    cost: "",
    lowestCostSupplier: "",
    lowestCostSupplierUrl: "",
    productUrl: "",
    manufacturer: "",
    manufacturerPartNumber: "",
    imageUrl: "",
    projectId: "",
    supplier: "",
    supplierPartNumber: "",
    storedFiles: []
  };

  const defaultMountingTypes = [
    {
      key: 0,
      value: "0", /** using strings here because semantic doesnt allow selection of value=0 */
      text: "None"
    },
    {
      key: 1,
      value: "1",
      text: "Through Hole"
    },
    {
      key: 2,
      value: "2",
      text: "Surface Mount"
    }
  ];

  const [parts, setParts] = useState([]);
  const [part, setPart] = useState(defaultPart);
  const [isEditing, setIsEditing] = useState((part && part.partId > 0) || pageHasParameters);
  const [isDirty, setIsDirty] = useState(false);
  const [selectedPart, setSelectedPart] = useState(null);
  const [selectedLocalFile, setSelectedLocalFile] = useState(null);
  const [recentParts, setRecentParts] = useState([]);
  const [metadataParts, setMetadataParts] = useState([]);
  const [duplicateParts, setDuplicateParts] = useState([]);
  const [partModalOpen, setPartModalOpen] = useState(false);
  const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [confirmLocalFileDeleteContent, setConfirmLocalFileDeleteContent] = useState(null);
  const [confirmDeleteLocalFileIsOpen, setConfirmDeleteLocalFileIsOpen] = useState(false);
  const [partTypes, setPartTypes] = useState([]);
  const [allPartTypes, setAllPartTypes] = useState([]);
  const [mountingTypes] = useState(defaultMountingTypes);
  const [loadingPartMetadata, setLoadingPartMetadata] = useState(false);
  const [loadingPartTypes, setLoadingPartTypes] = useState(true);
  const [loadingProjects, setLoadingProjects] = useState(true);
  const [loadingRecent, setLoadingRecent] = useState(true);
  const [partMetadataIsSubscribed, setPartMetadataIsSubscribed] = useState(false);
  const [partMetadataError, setPartMetadataError] = useState(null);
  const [saveMessage, setSaveMessage] = useState("");
  const [showBarcodeBeingScanned, setShowBarcodeBeingScanned] = useState(false);
  const [bulkScanIsOpen, setBulkScanIsOpen] = useState(false);
  const [dragOverClass, setDragOverClass] = useState("");
  const [partExistsInInventory, setPartExistsInInventory] = useState(false);
  const [isBulkScanSaving, setBulkScanSaving] = useState(false);
  const [scannedPartsBarcodeInput, setScannedPartsBarcodeInput] = useState(null);
  const [datasheetMeta, setDatasheetMeta] = useState(null);
  const [renderIsDirty, setRenderIsDirty] = useState(true);
  const currencyOptions = GetAdvancedTypeDropdown(Currencies, true);

  // todo: find a better alternative, we shouldn't need to do this!
  const bulkScanIsOpenRef = useRef();
  bulkScanIsOpenRef.current = bulkScanIsOpen;
  const partTypesRef = useRef();
  partTypesRef.current = partTypes;

  useEffect(() => {
    const partNumberStr = props.params.partNumber;
    setIsEditing((props.params && props.params.partNumber !== undefined && props.params.partNumber.length > 0));
    const fetchData = async () => {
      setPartMetadataIsSubscribed(false);
      setPartMetadataError(null);
      await fetchPartTypes();
      await fetchRecentRows();
      if (partNumberStr) {
        var loadedPart = await fetchPart(partNumberStr);
        if (isEditing) setInputPartNumber(partNumberStr);
        await fetchPartMetadataAndInventory(partNumberStr, loadedPart || part);
      } else {
        resetForm();
      }
    };
    fetchData().catch(console.error);
    return () => {
      searchDebounced.cancel();
    };
  }, [props.params.partNumber]);

  useEffect(() => {
    customEvents.notifySubscribers("avatar", true);
    return () => {
      customEvents.notifySubscribers("avatar", false);
    };
  }, []);

  const fetchPartMetadataAndInventory = async (input, part) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");
    if (input.length <= 3)
      return;
    Inventory.infoAbortController.abort();
    Inventory.infoAbortController = new AbortController();
    setLoadingPartMetadata(true);
    setPartMetadataIsSubscribed(false);
    setPartMetadataError(null);
    try {
      const includeInventorySearch = !pageHasParameters;
      const { data, existsInInventory } = await doFetchPartMetadata(input, part, includeInventorySearch);
      if (existsInInventory) setPartExistsInInventory(true);

      if (data.errors && data.errors.length > 0) {
        setPartMetadataError(`Error: [${data.apiName}] ${data.errors.join()}`);
        return;
      }

      let metadataParts = [];
      const infoResponse = mergeInfoResponse(data.response, part.storedFiles);
      if (infoResponse && infoResponse.parts && infoResponse.parts.length > 0) {
        metadataParts = infoResponse.parts;

        const suggestedPart = infoResponse.parts[0];
        // populate the form with data from the part metadata
        if(!pageHasParameters) setPartFromMetadata(metadataParts, suggestedPart);
      } else {
        // no part metadata available
        setPartMetadataIsSubscribed(true);
      }

      // set the first datasheet meta display, because the carousel component doesnt fire the first event
      if (infoResponse && infoResponse.datasheets && infoResponse.datasheets.length > 0) setDatasheetMeta(infoResponse.datasheets[0]);

      setInfoResponse(infoResponse);
      setMetadataParts(metadataParts);
      setLoadingPartMetadata(false);
    } catch (ex) {
      console.error("Exception", ex);
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  /**
   * Do a part information search
   * @param {string} partNumber part number to search for
   * @param {object} part part object of the form
   * @param {bool} includeInventorySearch true to also check local inventory for the part
   * @returns part information
   */
  const doFetchPartMetadata = async (partNumber, part, includeInventorySearch = true) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");
    Inventory.infoAbortController.abort();
    Inventory.infoAbortController = new AbortController();
    try {
      const response = await fetchApi(`api/part/info?partNumber=${partNumber}&supplierPartNumbers=digikey:${part.digiKeyPartNumber || ""},mouser:${part.mouserPartNumber || ""},arrow:${part.arrowPartNumber}`, {
        signal: Inventory.infoAbortController.signal
      });

      const data = response.data;
      if (data.requiresAuthentication) {
        // redirect for authentication
        window.open(data.redirectUrl, "_blank");
        return;
      }

      let existsInInventory = false;
      if (includeInventorySearch) {
        // also check inventory for this part
        const { exists, existsResponse } = await doInventoryPartSearch(partNumber);
        existsInInventory = exists;
      }

      // let caller handle errors
      return { data, existsInInventory };

    } catch (ex) {
      console.error("Exception", ex);
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  /**
   * Check local inventory for a part
   * @param {string} partNumber The part number to search for
   * @returns exists: true if part exists inventory, plus response
   */
  const doInventoryPartSearch = async (partNumber) => {
    if (partNumber.length <= 3)
      return;
    const existsResponse = await fetchApi(`api/part/search?keywords=${partNumber}&exactMatch=true`, {
      signal: Inventory.infoAbortController.signal,
      catchErrors: true
    });
    
    if(existsResponse.responseObject.ok && existsResponse.data !== null) {
      return { exists: true, data: existsResponse.data };
    }
    return { exists: false };
  };

  /**
   * Lookup a scanned part by it's barcode and return any metadata found
   * @param {object} scannedPart the scanned part object
   * @param {func} onSuccess function to invoke on barcode lookup success
   * @param {func} onFailure function to invoke on barcode lookup failure
   * @returns barcode metadata object
   */
  const doBarcodeLookup = async (scannedPart, onSuccess, onFailure) => {
    const response = await fetchApi(`api/part/barcode/info?barcode=${scannedPart.barcode}&token=${getImagesToken()}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      if (data.requiresAuthentication) {
        // redirect for authentication
        window.open(data.redirectUrl, "_blank");
        return;
      }
      if (data.errors && data.errors.length > 0) {
        const errorMessage = data.errors.join("\n");
        if(data.errors[0] === "Api is not enabled."){
          // supress warning for barcode scans
        }
        else
          toast.error(errorMessage);
      } else if (data.response.parts.length > 0) {
        // show the metadata in the UI
        var partInfo =  data.response.parts[0];
        onSuccess(partInfo);
      } else {
        // no barcode found
        onFailure(scannedPart);
      }
    }
  };

  const searchDebounced = useMemo(() => debounce(fetchPartMetadataAndInventory, 1000), [pageHasParameters]);

  // for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    if (!input.value) return;

    let cleanPartNumber = "";
    if (input.type === "datamatrix") {
      if (input.value.mfgPartNumber && input.value.mfgPartNumber.length > 0) cleanPartNumber = input.value.mfgPartNumber;
      else if (input.value.description && input.value.description.length > 0) cleanPartNumber = input.value.description;
    } else if (input.type === "code128") {
      cleanPartNumber = input.value;
    }

    if (!cleanPartNumber) return;

    // add part
    if (bulkScanIsOpenRef.current) {
      // bulk scan add part
      setScannedPartsBarcodeInput({ cleanPartNumber, input });
    } else {
      // scan single part
      // console.log('bulk scan is NOT open', cleanPartNumber);
      // fetch metadata on the barcode, don't await, do a background update
      const scannedPart = {
        partNumber: cleanPartNumber,
        barcode: input.correctedValue
      };
      setInputPartNumber(cleanPartNumber);
      doBarcodeLookup(scannedPart, (partInfo) => {
        // barcode found
        // console.log("Barcode info found!", partInfo);
        if (cleanPartNumber) {
          setPartMetadataIsSubscribed(false);
          setPartMetadataError(null);
          if(!isEditing) setPartFromMetadata(metadataParts, { ...partInfo, barcodeQuantity: partInfo.quantityAvailable });
          if (viewPreferences.rememberLast) updateViewPreferences({lastQuantity: partInfo.quantityAvailable});
          setShowBarcodeBeingScanned(false);

          // also run a search to get datasheets/images
          searchDebounced(cleanPartNumber, part);
          setIsDirty(true);
        }
      }, (scannedPart) => {
        // console.log("No barcode info found, searching part number");
        // no barcode info found, try searching the part number
        if (cleanPartNumber) {
          setPartMetadataIsSubscribed(false);
          setPartMetadataError(null);
          const newPart = {...part, 
            partNumber: cleanPartNumber, 
            quantity: parseInt(input.value?.quantity || "1"),
            partTypeId: -1,
            mountingTypeId: "-1",
          };
          setPart(newPart);
          if (viewPreferences.rememberLast) updateViewPreferences({lastQuantity: newPart.quantity});
          setShowBarcodeBeingScanned(false);
          searchDebounced(cleanPartNumber, newPart);
          setIsDirty(true);
        }
      }); 
    }
  };

  const formatField = (e) => {
    switch (e.target.name) {
      default:
        break;
      case "cost":
        part.cost = Number(part.cost).toFixed(2);
        if (isNaN(part.cost)) part.cost = Number(0).toFixed(2);
        break;
    }
    setPart(part);
  };

  const fetchPart = async (partNumber) => {
    Inventory.partAbortController.abort();
    Inventory.partAbortController = new AbortController();
    setLoadingPartMetadata(true);
    try {
      const response = await fetchApi(`api/part?partNumber=${partNumber}`, {
        signal: Inventory.partAbortController.signal
      });
      const { data } = response;
      const mappedPart = {...data, mountingTypeId: data.mountingTypeId + ""};
      setPart(mappedPart);
      setLoadingPartMetadata(false);
      return data;
    } catch (ex) {
      console.error("Exception", ex);
      setLoadingPartMetadata(false);
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const fetchRecentRows = async () => {
    setLoadingRecent(true);
    const response = await fetchApi(`api/part/list?orderBy=DateCreatedUtc&direction=Descending&results=${maxRecentAddedParts}`);
    const { data } = response;
    setRecentParts(data.items);
    setLoadingRecent(false);
  };

  const fetchPartTypes = async () => {
    setLoadingPartTypes(true);
    const response = await fetchApi("api/partType/all");
    const { data } = response;
    const partTypes = _.sortBy(
      data.map((item) => {
        return {
          key: item.partTypeId,
          value: item.partTypeId,
          text: item.name
        };
      }),
      "text"
    );
    setPartTypes(partTypes);
    setAllPartTypes(data);
    setLoadingPartTypes(false);
  };

  const getMountingTypeById = (mountingTypeId) => {
    switch (parseInt(mountingTypeId)) {
      default:
      case 0:
        return "none";
      case 1:
        return "through hole";
      case 2:
        return "surface mount";
    }
  };

  const mergeInfoResponse = (infoResponse, storedFiles) => {
    var storedProductImages = _.filter(storedFiles, (x) => x.storedFileType === StoredFileType.ProductImage);
    var storedDatasheets = _.filter(storedFiles, (x) => x.storedFileType === StoredFileType.Datasheet);
    var storedPinouts = _.filter(storedFiles, (x) => x.storedFileType === StoredFileType.Pinout);
    var storedReferenceDesigns = _.filter(storedFiles, (x) => x.storedFileType === StoredFileType.ReferenceDesign);
    if (storedProductImages && storedProductImages.length > 0)
      infoResponse.productImages.unshift(
        ...storedProductImages.map((pi) => ({
          name: pi.originalFileName,
          value: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId
        }))
      );
    if (storedDatasheets && storedDatasheets.length > 0)
      infoResponse.datasheets.unshift(
        ...storedDatasheets.map((pi) => ({
          name: pi.originalFileName,
          value: {
            datasheetUrl: `/api/storedFile/local?fileName=${pi.fileName}&token=${getImagesToken()}`,
            description: pi.originalFileName,
            imageUrl: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
            manufacturer: "",
            title: pi.originalFileName
          },
          id: pi.storedFileId
        }))
      );
    if (storedPinouts && storedPinouts.length > 0)
      infoResponse.pinoutImages.unshift(
        ...storedPinouts.map((pi) => ({
          name: pi.originalFileName,
          value: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId
        }))
      );
    if (storedReferenceDesigns && storedReferenceDesigns.length > 0)
      infoResponse.circuitImages.unshift(
        ...storedReferenceDesigns.map((pi) => ({
          name: pi.originalFileName,
          value: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId
        }))
      );
    return infoResponse;
  };

  /**
   * Force a save of a possible duplicate part
   * @param {any} e
   */
  const handleForceSubmit = (e) => {
    setDuplicatePartModalOpen(false);
    const updatedPart = { ...part, allowPotentialDuplicate: true };
    setPart(updatedPart);
    onSubmit(e, updatedPart);
  };

  /**
   * Save the part
   *
   * @param {any} e Event
   * @param {object} part The part to submit
   */
  const onSubmit = async (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    const isExisting = part.partId > 0;

    const request = { ...part };
    request.partNumber = inputPartNumber;
    request.partTypeId = part.partTypeId.toString();
    request.mountingTypeId = part.mountingTypeId.toString();
    request.quantity = Number.parseInt(part.quantity) || 0;
    request.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    request.cost = Number.parseFloat(part.cost) || 0.0;
    request.projectId = Number.parseInt(part.projectId) || null;

    const saveMethod = isExisting ? "PUT" : "POST";
    const response = await fetchApi("api/part", {
      method: saveMethod,
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request),
      // swallow any errors, we will handle them below
      catchErrors: true
    });

    let saveMessage = "";
    if (response.responseObject.status === 409) {
      // possible duplicate
      const data = await response.json();
      setDuplicateParts(data.parts);
      setDuplicatePartModalOpen(true);
    } else if (response.responseObject.status === 200) {
      // reset form if it was a new part
      if (isExisting) {
        saveMessage = t('message.savedPart', "Saved part {{partNumber}}!", { partNumber: request.partNumber });
        setSaveMessage(saveMessage);
        toast.info(saveMessage);
      } else {
        saveMessage = t('message.addedPart', "Added part {{partNumber}}!", { partNumber: request.partNumber });
        resetForm(saveMessage);
        toast.success(saveMessage);
      }
      setIsDirty(false);
      // refresh recent parts list
      await fetchRecentRows();
    } else if (response.responseObject.status === 400) {
      // other error (invalid part type, mounting type, etc.)
      saveMessage = t('message.failedSavePart', "Failed to update, check Part Type and Mounting Type");
      setSaveMessage(saveMessage);
      toast.error(saveMessage);
    }
  };

  const resetForm = (saveMessage = "", clearAll = false) => {
    setIsDirty(false);
    setIsEditing(false);
    setPartExistsInInventory(false);
    setSaveMessage(saveMessage);
    setMetadataParts([]);
    setDuplicateParts([]);
    setPartMetadataIsSubscribed(false);
    setInputPartNumber("");
    const clearedPart = {
      partId: 0,
      partNumber: "",
      allowPotentialDuplicate: false,
      quantity: (clearAll || !viewPreferences.rememberLast) ? "1" : viewPreferences.lastQuantity + "",
      lowStockThreshold: (clearAll || !viewPreferences.rememberLast) ? "10" : viewPreferences.lowStockThreshold + "",
      partTypeId: (clearAll || !viewPreferences.rememberLast) ? 0 : viewPreferences.lastPartTypeId,
      mountingTypeId: (clearAll || !viewPreferences.rememberLast) ? "0" : viewPreferences.lastMountingTypeId + "",
      packageType: "",
      keywords: "",
      description: "",
      datasheetUrl: "",
      digiKeyPartNumber: "",
      mouserPartNumber: "",
      arrowPartNumber: "",
      location: (clearAll || !viewPreferences.rememberLast) ? "" : viewPreferences.lastLocation + "",
      binNumber: (clearAll || !viewPreferences.rememberLast) ? "" : viewPreferences.lastBinNumber + "",
      binNumber2: (clearAll || !viewPreferences.rememberLast) ? "" : viewPreferences.lastBinNumber2 + "",
      cost: "",
      lowestCostSupplier: "",
      lowestCostSupplierUrl: "",
      productUrl: "",
      manufacturer: "",
      manufacturerPartNumber: "",
      imageUrl: "",
      projectId: (clearAll || !viewPreferences.rememberLast) ? "" : viewPreferences.lastProjectId,
      supplier: "",
      supplierPartNumber: ""
    };
    setPart(clearedPart);
    setLoadingPartMetadata(false);
    setLoadingPartTypes(false);
    setLoadingProjects(false);
    setInfoResponse({});
    if (clearAll && viewPreferences.rememberLast) {
      updateViewPreferences({ lastQuantity: clearedPart.quantity, lowStockThreshold: clearedPart.lowStockThreshold, lastPartTypeId: clearedPart.partTypeId, lastMountingTypeId: clearedPart.mountingTypeId, lastProjectId: clearedPart.projectId });
    }
    document.getElementById('inputPartNumber').focus();
  };

  const clearForm = (e) => {
    // e could be null as this special method can be called outside of the component without a synthetic event
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }

    if (props.params.partNumber) {
      navigate("/inventory/add");
      return;
    }

    resetForm("", true);
  };

  const updateNumberPicker = (e) => {
    if (viewPreferences.rememberLast) updateViewPreferences({lastQuantity: e.value});
    setPart({ ...part, quantity: e.value.toString() });
    setIsDirty(true);
  };

  const updateViewPreferences = (preference) => {
    const newViewPreferences = {...viewPreferences, ...preference};
    setViewPreferences(newViewPreferences);
    localStorage.setItem("viewPreferences", JSON.stringify(newViewPreferences));
  };

  const handleRememberLastSelection = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    updateViewPreferences({ rememberLast: control.checked });
  };

  const handleInputPartNumberChange = async (e, control) => {
    //e.preventDefault();
    //e.stopPropagation();
    console.log('change', control.value);
    setPartMetadataIsSubscribed(false);
    setPartMetadataError(null);
    let searchPartNumber = control.value;

    if (searchPartNumber && searchPartNumber.length >= 3) {
      searchPartNumber = control.value.replace("\t", "");
      await searchDebounced(searchPartNumber, part, partTypes);
    }

    setInputPartNumber(searchPartNumber);
    setIsDirty(true);

    // wont work unless we update render
    //setRenderIsDirty(!renderIsDirty);
  };

  const handlePartTypeChange = (e, partType) => { 
    if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastPartTypeId: partType.partTypeId});
    setPart({...part, partTypeId: partType.partTypeId});
    setIsDirty(true); 
  }

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setPartMetadataIsSubscribed(false);
    setPartMetadataError(null);
    const updatedPart = { ...part };

    updatedPart[control.name] = control.value;
    switch (control.name) {
      case "partNumber":
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) {
          updatedPart[control.name] = control.value.replace("\t", "");
          searchDebounced(updatedPart.partNumber, updatedPart, partTypes);
        }
        break;
      case "partTypeId":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastPartTypeId: control.value});
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber, updatedPart, partTypes);
        break;
      case "mountingTypeId":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastMountingTypeId: control.value});
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber, updatedPart, partTypes);
        break;
      case "lowStockThreshold":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lowStockThreshold: control.value});
        break;
      case "projectId":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastProjectId: control.value});
        break;
      case "location":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastLocation: updatedPart[control.name]});
        break;
      case "binNumber":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastBinNumber: updatedPart[control.name]});
        break;
      case "binNumber2":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({lastBinNumber2: updatedPart[control.name]});
        break;
      default:
        break;
    }
    setPart({ ...updatedPart });
    setIsDirty(true);
  };

  const printLabel = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`api/part/print?partNumber=${part.partNumber}&generateImageOnly=false`, { method: "POST" });
  };

  const setPartFromMetadata = (metadataParts, suggestedPart) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");

    const entity = { ...part };
    const mappedPart = {
      partNumber: suggestedPart.basePartNumber,
      partTypeId: getPartTypeId(suggestedPart.partType, partTypesRef.current),
      mountingTypeId: suggestedPart.mountingTypeId + "",
      packageType: suggestedPart.packageType,
      keywords: suggestedPart.keywords && suggestedPart.keywords.join(" ").toLowerCase(),
      description: suggestedPart.description,
      datasheetUrls: suggestedPart.datasheetUrls,
      supplier: suggestedPart.supplier,
      supplierPartNumber: suggestedPart.supplierPartNumber,
      cost: suggestedPart.cost,
      productUrl: suggestedPart.productUrl,
      manufacturer: suggestedPart.manufacturer,
      manufacturerPartNumber: suggestedPart.manufacturerPartNumber,
      imageUrl: suggestedPart.imageUrl,
      status: suggestedPart.status,
      quantity: suggestedPart.barcodeQuantity || 1,
    };
    // console.log('suggestedPart', mappedPart);
    entity.partNumber = mappedPart.partNumber;
    entity.supplier = mappedPart.supplier;
    entity.supplierPartNumber = mappedPart.supplierPartNumber;
    if (mappedPart.partTypeId) entity.partTypeId = mappedPart.partTypeId || "";
    if (mappedPart.mountingTypeId) entity.mountingTypeId = (mappedPart.mountingTypeId || "") + "";
    entity.packageType = mappedPart.packageType || "";
    entity.cost = mappedPart.cost || 0.0;
    entity.keywords = mappedPart.keywords || "";
    entity.description = mappedPart.description || "";
    entity.manufacturer = mappedPart.manufacturer || "";
    entity.manufacturerPartNumber = mappedPart.manufacturerPartNumber || "";
    entity.productUrl = mappedPart.productUrl || "";
    entity.imageUrl = mappedPart.imageUrl || "";
    if (mappedPart.datasheetUrls.length > 0) {
      entity.datasheetUrl = _.first(mappedPart.datasheetUrls) || "";
    }
    if (mappedPart.supplier === "DigiKey") {
      entity.digiKeyPartNumber = mappedPart.supplierPartNumber || "";
      // also map mouser
      let searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "Mouser" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.mouserPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
      // also map arrow
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "Arrow" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.arrowPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
      
    }
    if (mappedPart.supplier === "Mouser") {
      entity.mouserPartNumber = mappedPart.supplierPartNumber || "";
      // also map digikey
      let searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "DigiKey" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.digiKeyPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
      // also map arrow
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "Arrow" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.arrowPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }
    if (mappedPart.supplier === "Arrow") {
      entity.arrowPartNumber = mappedPart.supplierPartNumber || "";
      // also map digikey
      let searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "DigiKey" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.digiKeyPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
      // also map mouser
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "Mouser" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.mouserPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }

    const lowestCostPart = _.first(
      _.sortBy(
        _.filter(metadataParts, (i) => i.cost > 0),
        "cost"
      )
    );

    if (lowestCostPart) {
      entity.lowestCostSupplier = lowestCostPart.supplier;
      entity.lowestCostSupplierUrl = lowestCostPart.productUrl;
    }
    setPart(entity);
  };

  const handleChooseAlternatePart = (e, part, partTypes) => {
    setPartFromMetadata(metadataParts, part);
    setPartModalOpen(false);
  };

  const handleBulkBarcodeScan = (e) => {
    e.preventDefault();
    setBulkScanIsOpen(true);
  };

  const renderAllMatchingParts = (part, metadataParts) => {
    return (
      <Table compact celled selectable size="small" className="partstable">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.part', "Part")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.supplier', "Supplier")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.supplierType', "Supplier Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.packageType', "Package Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.mountingType', "Mounting Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.cost', "Cost")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.datasheet', "Datasheet")}</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {metadataParts.map((p, index) => (
            <Popup
              key={index}
              content="This is a test"
              trigger={
                <Table.Row onClick={(e) => handleChooseAlternatePart(e, p, partTypes)}>
                  <Table.Cell>
                    {part.supplier === p.supplier && part.supplierPartNumber === p.supplierPartNumber ? (
                      <Label ribbon>{p.manufacturerPartNumber}</Label>
                    ) : (
                      p.manufacturerPartNumber
                    )}
                  </Table.Cell>
                  <Table.Cell>{p.manufacturer}</Table.Cell>
                  <Table.Cell>{p.partType}</Table.Cell>
                  <Table.Cell>{p.supplier}</Table.Cell>
                  <Table.Cell>{p.supplierPartNumber}</Table.Cell>
                  <Table.Cell>{p.packageType}</Table.Cell>
                  <Table.Cell>{getMountingTypeById(p.mountingTypeId)}</Table.Cell>
                  <Table.Cell>{p.cost}</Table.Cell>
                  <Table.Cell>
                    <Image src={p.imageUrl} size="mini"></Image>
                  </Table.Cell>
                  <Table.Cell>
                    {p.datasheetUrls.map(
                      (d, dindex) =>
                        d &&
                        d.length > 0 && (
                          <Button key={dindex} onClick={(e) => handleHighlightAndVisit(e, d)}>
                            {t('button.viewDatasheet', "View Datasheet")}
                          </Button>
                        )
                    )}
                  </Table.Cell>
                </Table.Row>
              }
            />
          ))}
        </Table.Body>
      </Table>
    );
  };

  const renderDuplicateParts = () => {
    return (
      <Table compact celled selectable size="small" className="partstable">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.partNumber', "Part Number")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.manufacturerPart', "Manufacturer Part")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.description', "Description")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.location', "Location")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber', "Bin Number")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber2', "Bin Number 2")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.mountingType', "Mounting Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.datasheet', "Datasheet")}</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {duplicateParts.map((p, index) => (
            <Table.Row key={index}>
              <Table.Cell>{p.partNumber}</Table.Cell>
              <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
              <Table.Cell>{p.manufacturer}</Table.Cell>
              <Table.Cell>{p.description}</Table.Cell>
              <Table.Cell>{p.partType}</Table.Cell>
              <Table.Cell>{p.location}</Table.Cell>
              <Table.Cell>{p.binNumber}</Table.Cell>
              <Table.Cell>{p.binNumber2}</Table.Cell>
              <Table.Cell>{getMountingTypeById(p.mountingTypeId)}</Table.Cell>
              <Table.Cell>
                <Image src={p.imageUrl} size="mini"></Image>
              </Table.Cell>
              <Table.Cell>
                <Button onClick={(e) => handleHighlightAndVisit(e, p.datasheetUrl)}>{t('button.viewDatasheet', "View Datasheet")}</Button>
              </Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
    );
  };

  const handleDuplicatePartModalClose = () => {
    setDuplicatePartModalOpen(false);
  };

  const disableHelp = () => {
    // const { viewPreferences } = state;
    // const val = { ...viewPreferences, helpDisabled: true };
    // localStorage.setItem('viewPreferences', JSON.stringify(val));
  };

  const handleHighlightAndVisit = (e, url) => {
    handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains("positive")) row.classList.remove("positive");
    }
    targetNode.classList.toggle("positive");
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const handleRecentPartClick = async (e, part) => {
    setPart(part);
    props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
    await fetchPart(part.partNumber);
  };

  const handleSaveScannedParts = async (e, scannedParts) => {
    e.preventDefault();
    e.stopPropagation();
    if (scannedParts.length === 0)
      return true;
    setBulkScanSaving(true);
    const request = {
      parts: scannedParts
    };
    const response = await fetchApi("api/part/bulk", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      toast.success(t('message.addXParts', "Added {{count}} new parts!", {count: data.length}));
      setBulkScanIsOpen(false);
      setBulkScanSaving(false);
      return true;
    }
    setBulkScanSaving(false);
    return false;
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`api/part`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ partId: selectedPart.partId })
    });
    const partsDeleted = _.without(parts, _.findWhere(parts, { partId: selectedPart.partId }));
    setConfirmDeleteIsOpen(false);
    setParts(partsDeleted);
    setSelectedPart(null);
    props.history(`/inventory`);
  };

  const handleDeleteLocalFile = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`api/storedfile`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ storedFileId: selectedLocalFile.localFile.id })
    });
    var itemsExceptDeleted;
    switch (selectedLocalFile.type) {
      case "productImages":
        itemsExceptDeleted = _.without(infoResponse.productImages, _.findWhere(infoResponse.productImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({ ...infoResponse, productImages: itemsExceptDeleted });
        break;
      case "datasheets":
        itemsExceptDeleted = _.without(infoResponse.datasheets, _.findWhere(infoResponse.datasheets, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({ ...infoResponse, datasheets: itemsExceptDeleted });
        if (itemsExceptDeleted.length > 0) setDatasheetMeta(itemsExceptDeleted[0]);
        break;
      case "pinoutImages":
        itemsExceptDeleted = _.without(infoResponse.pinoutImages, _.findWhere(infoResponse.pinoutImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({ ...infoResponse, pinoutImages: itemsExceptDeleted });
        break;
      case "circuitImages":
        itemsExceptDeleted = _.without(infoResponse.circuitImages, _.findWhere(infoResponse.circuitImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({ ...infoResponse, circuitImages: itemsExceptDeleted });
        break;
      default:
    }

    setConfirmDeleteLocalFileIsOpen(false);
    setSelectedLocalFile(null);
  };

  const confirmDeleteOpen = (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(true);
    setSelectedPart(part);
    setConfirmPartDeleteContent(
      <p>
        <Trans i18nKey="confirm.deletePart" name={inputPartNumber}>
        Are you sure you want to delete part <b>{{name: inputPartNumber}}</b>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>        
      </p>
    );
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
  };


  const confirmDeleteLocalFileClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteLocalFileIsOpen(false);
    setSelectedLocalFile(null);
  };

  const handleSetSuggestedPartNumber = (e, value) => {
    e.preventDefault();
    e.stopPropagation();
    setInputPartNumber(value);
  };

  // todo: can this be removed?
  const matchingPartsList = renderAllMatchingParts(part, metadataParts);
  const title = isEditing 
    ? t('page.inventory.edittitle', "Edit Inventory") 
    : t('page.inventory.addtitle', "Add Inventory");

  /* RENDER */

  const renderForm = useMemo(() => {
    console.log('render!');
    return (
    <>
    {!isEditing &&
      <Popup
        wide
        position="right center"
        content={<p>{t('page.inventory.popup.bulkAddParts', "Bulk add parts using a barcode scanner")}</p>}
        trigger={<div style={{display: 'inline-block'}}><BulkScanIcon onClick={handleBulkBarcodeScan} /></div>}
      />
    }

    <div className="page-banner">
      {partMetadataIsSubscribed && (
        <div className="page-notice" onClick={() => setPartMetadataIsSubscribed(false)}>
          <div>
            <Icon name="close" /> 
            <Trans i18nKey="message.noPartInfo" partNumber={inputPartNumber}>
            No part information is available for '{{partNumber: inputPartNumber}}'. You are subscribed to updates and will be automatically updated when the part is indexed.
            </Trans>
          </div>
        </div>
      )}
      {partMetadataError && (
        <div className="page-error" onClick={() => setPartMetadataError(null)}>
          <Icon name="close" /> {partMetadataError}
        </div>
      )}
    </div>

    <Grid celled className="inventory-container">
      <Grid.Row>
        <Grid.Column width={12} className="left-column">
          {/** LEFT COLUMN */}
          <Form.Group>
            <Form.Field>
              <ProtectedInput
                label={t('label.part', "Part")}
                required
                placeholder="LM358"
                focus
                /* this should be the only field that is not updated on an info update */
                value={inputPartNumber || ""}
                onChange={handleInputPartNumberChange}
                name="inputPartNumber"
                icon="search"
                id="inputPartNumber"
              />
              {!isEditing && part.partNumber && part.partNumber !== inputPartNumber && <div className="suggested-part">{<span>{t('page.inventory.suggestedPart')}: <a href="#" onClick={e => handleSetSuggestedPartNumber(e, part.partNumber)}>{part.partNumber}</a></span>}</div>}
              {!isEditing && partExistsInInventory && <div className="suggested-part">This <Link to={`/inventory/${inputPartNumber}`}>part</Link> <span>already exists</span> in your inventory.</div>}
            </Form.Field>
            <Form.Field width={6}>
              {/* PartTypeSelector is internally memoized for performance */}
              <PartTypeSelector 
                label={t('label.partType', "Part Type")}
                name="partTypeId"
                value={part.partTypeId || ""}
                partTypes={allPartTypes} 
                onSelect={handlePartTypeChange}
              />
            </Form.Field>
            <Form.Dropdown
              label={t('label.mountingType', "Mounting Type")}
              placeholder={t('label.mountingType', "Mounting Type")}
              search
              selection
              value={(part.mountingTypeId || "") + ""}
              options={mountingTypes}
              onChange={handleChange}
              name="mountingTypeId"
            />
          </Form.Group>
          <Form.Group>
            <Popup
              hideOnScroll
              disabled={viewPreferences.helpDisabled}
              onOpen={disableHelp}
              content={t('page.inventory.popup.quantity', "Use the mousewheel and CTRL/ALT to change step size")}
              trigger={
                <Form.Field
                  control={NumberPicker}
                  label={t('label.quantity', "Quantity")}
                  placeholder="10"
                  min={0}
                  value={part.quantity || ""}
                  onChange={updateNumberPicker}
                  name="quantity"
                  autoComplete="off"
                />
              }
            />
            <Popup
              hideOnScroll
              disabled={viewPreferences.helpDisabled}
              onOpen={disableHelp}
              content={t('page.inventory.popup.lowStock', "Alert when the quantity gets below this value")}
              trigger={
                <Form.Input
                  label={t('label.lowStock', "Low Stock")}
                  placeholder="10"
                  value={part.lowStockThreshold || ""}
                  onChange={handleChange}
                  name="lowStockThreshold"
                  width={3}
                />
              }
            />
          </Form.Group>

          {/* Part Location Information */}
          <Segment secondary>
            <Form.Group>
              <Popup
                hideOnScroll
                disabled={viewPreferences.helpDisabled}
                onOpen={disableHelp}
                content={t('page.inventory.popup.location', "A custom value for identifying the parts location")}
                trigger={
                  <Form.Input
                    label={t('label.location', "Location")}
                    placeholder="Home lab"
                    value={part.location || ""}
                    onChange={handleChange}
                    name="location"
                    width={5}
                  />
                }
              />
              <Popup
                hideOnScroll
                disabled={viewPreferences.helpDisabled}
                onOpen={disableHelp}
                content={t('page.inventory.popup.binNumber', "A custom value for identifying the parts location")}
                trigger={
                  <Form.Input
                    label={t('label.binNumber', "Bin Number")}
                    placeholder={t('page.inventory.placeholder.binNumber', "IC Components 2")}
                    value={part.binNumber || ""}
                    onChange={handleChange}
                    name="binNumber"
                    width={4}
                  />
                }
              />
              <Popup
                hideOnScroll
                disabled={viewPreferences.helpDisabled}
                onOpen={disableHelp}
                content={t('page.inventory.popup.binNumber', "A custom value for identifying the parts location")}
                trigger={
                  <Form.Input
                    label={t('label.binNumber2', "Bin Number 2")}
                    placeholder={t('page.inventory.placeholder.binNumber2', "14")}
                    value={part.binNumber2 || ""}
                    onChange={handleChange}
                    name="binNumber2"
                    width={4}
                  />
                }
              />
            </Form.Group>
          </Segment>
          <Form.Field inline>
            {!isEditing &&
              <Popup
                wide="very"
                position="top right"
                content={
                <p>
                  <Trans i18nKey="page.inventory.popup.rememberLastSelection">
                  Enable this toggle to remember the last selected values of: <i>Part Type, Mounting Type, Quantity, Low Stock, Project, Location, Bin Number, Bin Number 2</i>
                  </Trans>
                </p>}
                trigger={<Checkbox toggle label={t('label.rememberLastSelection', "Remember last selection")} className="left small" style={{float: 'right'}} checked={viewPreferences.rememberLast || false} onChange={handleRememberLastSelection} />}
              />
            }
            <Button.Group>
              <Button type="submit" primary style={{ width: "200px" }} disabled={!isDirty}>
                <Icon name="save" />
                {t('button.save', "Save")}
              </Button>
              <Button.Or text={t('button.or', "Or")} />
              <Popup 
                position="right center"
                content={t('page.inventory.popup.clear', "Clear the form to default values")}
                trigger={<Button type="button" style={{ width: "100px" }} onClick={clearForm}>{t('button.clear', "Clear")}</Button>}
              />                  
            </Button.Group>
            {part && part.partId > 0 && (
              <Button type="button" title="Delete" onClick={(e) => confirmDeleteOpen(e, part)} style={{ marginLeft: "10px" }}>
                <Icon name="delete" />
                {t('button.delete', "Delete")}
              </Button>
            )}
            {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
          </Form.Field>
          <div className="sticky-target" style={{padding: '10px 10px 20px 10%'}} data-bounds={"0.20,0.65"}>
            <Button.Group>
              <Button type="submit" primary style={{ width: "200px" }} disabled={!isDirty}>
                <Icon name="save" />
                {t('button.save', "Save")}
              </Button>
              <Button.Or text={t('button.or', "Or")} />
              <Popup 
                position="right center"
                content={t('page.inventory.popup.clear', "Clear the form to default values")}
                trigger={<Button type="button" style={{ width: "100px" }} onClick={clearForm}>{t('button.clear', "Clear")}</Button>}
              />                  
            </Button.Group>
          </div>

          {/* PART METADATA */}

          <Segment loading={loadingPartMetadata} color="blue">
            <Header dividing as="h3">
              {t('page.inventory.partMetadata', "Part Metadata")}
            </Header>

            {metadataParts && metadataParts.length > 1 && (
              <ChooseAlternatePartModal
                trigger={
                  <Popup
                    hideOnScroll
                    disabled={viewPreferences.helpDisabled}
                    onOpen={disableHelp}
                    content={t('page.inventory.popup.alternateParts', "Choose a different part to extract metadata information from. By default, Binner will give you the most relevant part and with the highest quantity available.")}
                    trigger={
                      <Button secondary>
                        <Icon name="external alternate" color="blue" />
                        {t('page.inventory.chooseAlternatePart', "Choose alternate part ({{count}})", { count: formatNumber(metadataParts.length) } )}
                      </Button>
                    }
                  />
                }
                part={part}
                metadataParts={metadataParts}
                onPartChosen={(e, p) => handleChooseAlternatePart(e, p, partTypes)}
              />
            )}

            <Form.Group>
              <Form.Field width={4}>
                <label>{t('label.cost', "Cost")}</label>
                <Input
                  className="labeled"
                  placeholder="0.00"
                  value={part.cost}
                  type="text"
                  onChange={handleChange}
                  name="cost"
                  onBlur={formatField}
                >
                  <Dropdown 
                    name="currency"
                    className="label currency"
                    placeholder="$"
                    value={part.currency || 'USD'}
                    options={currencyOptions}
                    onChange={handleChange}
                  />
                  <input />
                </Input>
              </Form.Field>
              <Form.Input
                label={t('label.manufacturer', "Manufacturer")}
                placeholder="Texas Instruments"
                value={part.manufacturer || ""}
                onChange={handleChange}
                name="manufacturer"
                width={4}
              />
              <Form.Input
                label={t('label.manufacturerPart', "Manufacturer Part")}
                placeholder="LM358"
                value={part.manufacturerPartNumber || ""}
                onChange={handleChange}
                name="manufacturerPartNumber"
              />
              <Form.Field style={{textAlign: 'right'}}>
                <Image src={part.imageUrl} size="tiny" />
              </Form.Field>
            </Form.Group>
            <Form.Field
              width={16}
              control={TextArea}
              label={t('label.description', "Description")}
              value={part.description || ""}
              onChange={handleChange}
              name="description"
            />
            <Form.Field width={16}>
              <label>{t('label.keywords', "Keywords")}</label>
              <Input
                icon="tags"
                iconPosition="left"
                label={{ tag: true, content: t('page.inventory.addKeyword', "Add Keyword") }}
                labelPosition="right"
                placeholder={t('page.inventory.placeholder.keywords', "op amp")}
                onChange={handleChange}
                value={part.keywords || ""}
                name="keywords"
              />
            </Form.Field>
            <Form.Field width={8}>
              <label>{t('label.packageType', "Package Type")}</label>
              <Input
                placeholder="DIP8"
                value={part.packageType || ""}
                onChange={handleChange}
                name="packageType"
              />
            </Form.Field>
          </Segment>

          {/* Part Preferences */}
          <Segment loading={loadingPartMetadata} color="green">
            <Header dividing as="h3">
              {t('page.inventory.privatePartInfo', "Private Part Information")}
            </Header>
            <p>{t('page.inventory.privatePartInfoMessage', "These values can be set manually and will not be synchronized automatically via apis.")}</p>

            <Form.Field>
              <label>{t('label.primaryDatasheetUrl', "Primary Datasheet Url")}</label>
              <Input action className='labeled' placeholder='www.ti.com/lit/ds/symlink/lm2904-n.pdf' value={(part.datasheetUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='datasheetUrl'>
                <Label>https://</Label>
                <input />
                <Button onClick={e => handleVisitLink(e, part.datasheetUrl)} disabled={!part.datasheetUrl || part.datasheetUrl.length === 0}>{t('button.view', "View")}</Button>
              </Input>
            </Form.Field>
            <Form.Field>
              <label>{t('label.productUrl', "Product Url")}</label>
              <Input action className='labeled' placeholder='' value={(part.productUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='productUrl'>
                <Label>https://</Label>
                <input />
                <Button onClick={e => handleVisitLink(e, part.productUrl)} disabled={!part.productUrl || part.productUrl.length === 0}>{t('button.visit', "Visit")}</Button>
              </Input>
            </Form.Field>
            <Form.Group>
              <Form.Field width={4}>
                <label>{t('label.digikeyPartNumber', "DigiKey Part Number")}</label>
                <Input placeholder='296-1395-5-ND' value={part.digiKeyPartNumber || ''} onChange={handleChange} name='digiKeyPartNumber' />
              </Form.Field>
              <Form.Field width={4}>
                <label>{t('label.mouserPartNumber', "Mouser Part Number")}</label>
                <Input placeholder='595-LM358AP' value={part.mouserPartNumber || ''} onChange={handleChange} name='mouserPartNumber' />
              </Form.Field>
              <Form.Field width={4}>
                <label>{t('label.arrowPartNumber', "Arrow Part Number")}</label>
                <Input placeholder='595-LM358AP' value={part.arrowPartNumber || ''} onChange={handleChange} name='arrowPartNumber' />
              </Form.Field>
            </Form.Group>
          </Segment>

          {/* Suppliers */}

          <PartSuppliers 
            loadingPartMetadata={loadingPartMetadata} 
            part={part} 
            metadataParts={metadataParts}
          />
          
        </Grid.Column>

        <Grid.Column width={4} className="right-column">
          {/** RIGHT COLUMN */}

          {/* todo: performance memoize */}
          <PartMedia infoResponse={infoResponse} datasheet={datasheetMeta} />          

          {/* END RIGHT COLUMN */}
        </Grid.Column>
      </Grid.Row>
    </Grid>

 
    </>);
  }, [renderIsDirty, inputPartNumber, part]);

  const renderRecentParts = useMemo(() => {
    console.log('render recent parts!');
    return (
      <RecentParts 
        recentParts={recentParts} 
        loadingRecent={loadingRecent} 
        handleRecentPartClick={handleRecentPartClick} 
      />
    );
  }, [recentParts, loadingRecent]);

  return (
    <div className="mask">
      <Modal centered open={duplicatePartModalOpen} onClose={handleDuplicatePartModalClose}>
        <Modal.Header>Duplicate Part</Modal.Header>
        <Modal.Content scrolling>
          <Modal.Description>
            <h3>There is a possible duplicate part already in your inventory.</h3>
            {renderDuplicateParts()}
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleDuplicatePartModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleForceSubmit}>
          {t('button.saveAnyway', "Save Anyway")}
          </Button>
        </Modal.Actions>
      </Modal>
      <Confirm
        className="confirm"
        header={t('confirm.header.deletePart', "Delete Part")}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDeletePart}
        content={confirmPartDeleteContent}
      />
      <Confirm
        className="confirm"
        header={t('confirm.header.deleteFile', "Delete File")}
        open={confirmDeleteLocalFileIsOpen}
        onCancel={confirmDeleteLocalFileClose}
        onConfirm={handleDeleteLocalFile}
        content={confirmLocalFileDeleteContent}
      />
      <BulkScanModal 
        isOpen={bulkScanIsOpen} 
        onClose={() => setBulkScanIsOpen(false)}
        barcodeInput={scannedPartsBarcodeInput} 
        isBulkScanSaving={isBulkScanSaving} 
        onSave={handleSaveScannedParts} 
        onBarcodeLookup={doBarcodeLookup} 
        onGetPartMetadata={doFetchPartMetadata} 
        onInventoryPartSearch={doInventoryPartSearch} 
      />

      {/* FORM START */}

      <Form onSubmit={e => onSubmit(e, part)} className="inventory">
        {!isEditing && <BarcodeScannerInput onReceived={handleBarcodeInput} minInputLength={4} swallowKeyEvent={false} /> }
        {part && part.partId > 0 && (
          <Button
            type="button"
            animated="vertical"
            circular
            floated="right"
            size="mini"
            onClick={printLabel}
            style={{ marginTop: "5px", marginRight: "20px", width: "100px" }}
          >
            <Button.Content hidden>{t('button.print', "Print")}</Button.Content>
            <Button.Content visible>
              <Icon name="print" />
            </Button.Content>
          </Button>
        )}
        {part.partNumber && <Image src={`api/part/preview?partNumber=${part.partNumber}&token=${getImagesToken()}`} width={180} floated="right" style={{ marginTop: "0px" }} />}
        <FormHeader name={title} to="..">
        </FormHeader>
        {renderForm}
        {renderRecentParts}
      </Form>
      
    </div>
  );
}

Inventory.partAbortController = new AbortController();
Inventory.infoAbortController = new AbortController();

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Inventory {...props} params={useParams()} history={useNavigate()} />;

Inventory.propTypes = {
  partNumber: PropTypes.string
};

Inventory.defaultProps = {
  partNumber: ""
};
