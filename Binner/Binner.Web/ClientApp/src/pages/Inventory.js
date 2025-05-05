import React, { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { useParams, useNavigate, Link, useBlocker } from "react-router-dom";
import { useTranslation, Trans } from 'react-i18next';
import PropTypes from "prop-types";
import _ from "underscore";
import debounce from "lodash.debounce";
import { Icon, Input, Label, Button, TextArea, Image, Form, Segment, Popup, Header, Confirm, Grid, Checkbox, Dropdown, Breadcrumb, Dimmer, Loader } from "semantic-ui-react";
import { toast } from "react-toastify";
import TextSnippet from "@mui/icons-material/TextSnippet";
import ViewInArIcon from '@mui/icons-material/ViewInAr';
import BeenhereIcon from '@mui/icons-material/Beenhere';
import ProtectedInput from "../components/ProtectedInput";
import ClearableInput from "../components/ClearableInput";
import NumberPicker from "../components/NumberPicker";
import PartTypeSelectorMemoized from "../components/PartTypeSelectorMemoized";
import { FormHeader } from "../components/FormHeader";
import { ChooseAlternatePartModal } from "../components/modals/ChooseAlternatePartModal";
import { PartMediaMemoized } from "../components/PartMediaMemoized";
import { BulkScanModal } from "../components/BulkScanModal";
import { BulkScanIconMemoized } from "../components/BulkScanIconMemoized";
import { RecentParts } from "../components/RecentParts";
import { PartSuppliersMemoized } from "../components/PartSuppliersMemoized";
import { MatchingPartsMemoized } from "../components/MatchingPartsMemoized";
import { DuplicatePartModal } from "../components/DuplicatePartModal";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData, removeLocalData } from "../common/storage";
import { addMinutes } from "../common/datetime";
import { formatNumber } from "../common/Utils";
import { getPartTypeId } from "../common/partTypes";
import { getImagesToken } from "../common/authentication";
import { StoredFileType } from "../common/StoredFileType";
import { MountingTypes, GetAdvancedTypeDropdown } from "../common/Types";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import { Currencies } from "../common/currency";
import { getSystemSettings } from "../common/applicationSettings";
// overrides BarcodeScannerInput audio support
const enableSound = true;
const soundSuccess = new Audio('/audio/scan-success.mp3');
const soundFailure = new Audio('/audio/scan-failure.mp3');
const soundDiscard = new Audio('/audio/discard.mp3');
import "./Inventory.css";

export function Inventory({ partNumber = "", ...rest }) {
  const SearchDebounceTimeMs = 750;
  const IcPartType = 14;
  const DefaultLowStockThreshold = 10;
  const DefaultQuantity = 1;
  const DefaultMountingTypeId = 0;
  const maxRecentAddedParts = 10;
  const MinSearchKeywordLength = 3;
  const { t } = useTranslation();
  const navigate = useNavigate();

  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'inventory' })
  };

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'inventory', ...options });
  };

  const removeViewPreference = (preferenceName) => {
    removeLocalData(preferenceName, { settingsName: 'inventory' });
  };

  const defaultViewPreferences = JSON.parse(localStorage.getItem("viewPreferences")) || {
    helpDisabled: false,
    lastPartTypeId: IcPartType, // IC
    lastMountingTypeId: DefaultMountingTypeId, // None
    lastQuantity: DefaultQuantity,
    lastProjectId: null,
    lastLocation: "",
    lastBinNumber: "",
    lastBinNumber2: "",
    lowStockThreshold: DefaultLowStockThreshold,
    rememberLast: true
  };
  const [viewPreferences, setViewPreferences] = useState(defaultViewPreferences);
  const pageHasParameters = rest.params?.partNumber?.length > 0;
  const defaultPart = {
    partId: 0,
    partNumber: rest.params.partNumber || "",
    allowPotentialDuplicate: false,
    quantity: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastQuantity) || DefaultQuantity,
    lowStockThreshold: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lowStockThreshold) + "",
    partTypeId: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastPartTypeId) || IcPartType,
    mountingTypeId: (!pageHasParameters && viewPreferences.rememberLast && viewPreferences.lastMountingTypeId) || DefaultMountingTypeId,
    packageType: "",
    keywords: "",
    description: "",
    datasheetUrl: "",
    digiKeyPartNumber: "",
    mouserPartNumber: "",
    arrowPartNumber: "",
    tmePartNumber: "",
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
    symbolName: "",
    footprintName: "",
    extensionValue1: "",
    extensionValue2: "",
    storedFiles: []
  };

  const [inputPartNumber, setInputPartNumber] = useState(rest.params.partNumberToAdd || "");
  const [infoResponse, setInfoResponse] = useState({});
  const [parts, setParts] = useState([]);
  const [part, setPart] = useState(defaultPart);
  const [quantityAdded, setQuantityAdded] = useState(0);
  const [isEditing, setIsEditing] = useState((part && part.partId > 0) || pageHasParameters);
  const [isDirty, setIsDirty] = useState(false);
  const [selectedPart, setSelectedPart] = useState(null);
  const [recentParts, setRecentParts] = useState([]);
  const [metadataParts, setMetadataParts] = useState([]);
  const [duplicateParts, setDuplicateParts] = useState([]);
  const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(false);
  const [confirmDeletePartIsOpen, setConfirmDeletePartIsOpen] = useState(false);
  const [confirmRefreshPartIsOpen, setConfirmRefreshPartIsOpen] = useState(false);
  const [confirmDeletePartContent, setConfirmDeletePartContent] = useState(null);
  const [confirmRefreshPartContent, setConfirmRefreshPartContent] = useState(null);
  const [confirmRefreshPartDoNotAskAgain, setConfirmRefreshPartDoNotAskAgain] = useState(false);
  const [partTypes, setPartTypes] = useState([]);
  const [allPartTypes, setAllPartTypes] = useState([]);
  const [loadingPart, setLoadingPart] = useState(false);
  const [loadingPartMetadata, setLoadingPartMetadata] = useState(false);
  const [loadingPartTypes, setLoadingPartTypes] = useState(true);
  const [loadingRecent, setLoadingRecent] = useState(true);
  const [partMetadataIsSubscribed, setPartMetadataIsSubscribed] = useState(false);
  const [partMetadataErrors, setPartMetadataErrors] = useState([]);
  const [saveMessage, setSaveMessage] = useState("");
  const [bulkScanIsOpen, setBulkScanIsOpen] = useState(false);
  const [partExistsInInventory, setPartExistsInInventory] = useState(false);
  const [lastBarcodeScan, setLastBarcodeScan] = useState(null);
  const [isBulkScanSaving, setBulkScanSaving] = useState(false);
  const [scannedPartsBarcodeInput, setScannedPartsBarcodeInput] = useState(null);
  const [datasheetMeta, setDatasheetMeta] = useState(null);
  //const [disableRendering, setDisableRendering] = useState(false);
  const disableRendering = useRef(false);
  const currencyOptions = GetAdvancedTypeDropdown(Currencies, true);
  const mountingTypeOptions = GetAdvancedTypeDropdown(MountingTypes, true);
  const [systemSettings, setSystemSettings] = useState({ currency: "USD" });
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationApiName, setAuthorizationApiName] = useState('');
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const [confirmDiscardChanges, setConfirmDiscardChanges] = useState(false);
  const [confirmDiscardAction, setConfirmDiscardAction] = useState(null);
  const [confirmReImport, setConfirmReImport] = useState(false);
  const [confirmReImportAction, setConfirmReImportAction] = useState(null);

  let blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      isDirty && currentLocation.pathname !== nextLocation.pathname
  );

  // todo: find a better alternative, we shouldn't need to do this!
  const partRef = useRef();
  partRef.current = part;
  const bulkScanIsOpenRef = useRef();
  bulkScanIsOpenRef.current = bulkScanIsOpen;
  const partTypesRef = useRef();
  partTypesRef.current = partTypes;

  useEffect(() => {
    // when either of these change, play a sound
    if (blocker.state === "blocked" || confirmDiscardChanges) {
      if (enableSound) soundDiscard.play();
    }
  }, [blocker.state, confirmDiscardChanges]);

  useEffect(() => {
    const partNumberRaw = rest.params.partNumber;
    let partNumberStr = partNumberRaw?.trim();
    let partId = 0;
    if (partNumberRaw?.includes(':')) {
      const parts = partNumberRaw.split(':');
      if (parts.length >= 1)
        partNumberStr = parts[0].trim();
      if (parts.length >= 2)
        partId = parseInt(parts[1].trim());
    }
    const newIsEditing = partNumberStr?.length > 0;
    setIsEditing(newIsEditing);

    const fetchData = async (initialRequest, targetPart) => {
      let partToSearch = targetPart;
      setPartMetadataIsSubscribed(false);
      setPartMetadataErrors([]);
      await fetchPartTypes();
      await fetchRecentRows();
      
      if (partNumberStr) {
        // editing an existing part
        partToSearch = await fetchPart(partNumberStr, partId) || part;

        setInputPartNumber(partNumberStr);
        setLoadingPartMetadata(true);
        await fetchPartMetadataAndInventory(partNumberStr, partToSearch);
        setLoadingPartMetadata(false);
      } else if (rest.params.partNumberToAdd) {
        // a part number to add is specified in the URL path
        const { data } = await doFetchPartMetadata(rest.params.partNumberToAdd, partToSearch, false);
        processPartMetadataResponse(data, partToSearch.storedFiles, true, true);
        setLoadingPartMetadata(false);
        setIsDirty(true);
      } else {
        if (initialRequest) {
          // adding a new part, reset the form
          resetForm();
        } else {
          // fetch part metadata, don't allow overwriting of fields that have already been entered
          setLoadingPartMetadata(true);
          const { data } = await doFetchPartMetadata(targetPart.partNumber, partToSearch, false);
          processPartMetadataResponse(data, partToSearch.storedFiles, true, false); // false, don't overwrite entered fields
          setLoadingPartMetadata(false);
          setIsDirty(true);
        }
      }
    };

    // restore temporary input data on load, then remove it. Used for when a redirect to DigiKey is required.
    let initialRequest = true;
    const digikeyTempSettings = getViewPreference('digikey');
    if (digikeyTempSettings?.partNumber) {
      setPart(digikeyTempSettings);
      setInputPartNumber(digikeyTempSettings.partNumber);
      removeViewPreference('digikey');
      initialRequest = false;
    }

    fetchData(initialRequest, digikeyTempSettings || part).catch(console.error);
    
    getSystemSettings().then((systemSettings) => {
      setSystemSettings(systemSettings);
    });
    return () => {
      searchDebounced.cancel();
      Inventory.doFetchPartMetadataController?.abort();
      toast.dismiss();
    };
  }, [rest.params.partNumber]);

  const fetchPartMetadataAndInventory = async (input, localPart) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");
    if (input.trim().length < MinSearchKeywordLength)
      return { part: null, exists: false };
    setLoadingPartMetadata(true);
    setPartMetadataIsSubscribed(false);
    setPartMetadataErrors([]);
    try {
      const includeInventorySearch = !pageHasParameters;
      const { data, existsInInventory } = await doFetchPartMetadata(input, localPart, includeInventorySearch);
      if (existsInInventory) setPartExistsInInventory(true);

      processPartMetadataResponse(data, localPart.storedFiles, !pageHasParameters, false);
      setLoadingPartMetadata(false);
      return { part: localPart, exists: existsInInventory };
    } catch (ex) {
      setLoadingPartMetadata(false);
      console.error("Exception", ex);
      if (ex?.name === "AbortError") {
        return { part: null, exists: false }; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  /**
   * Map the supplier part numbers from the information in metadata info of all parts from all apis
   * @param {object} entity 
   * @param {array} metadataParts 
   * @returns 
   */
  const mapSupplierPartNumbers = (entity, metadataParts, allowOverwrite) => {
    // map digikey
    let searchResult = _.find(metadataParts, (e) => {
      return e !== undefined && e.supplier === "DigiKey" && e.manufacturerPartNumber === entity.manufacturerPartNumber;
    });
    if (searchResult) {
      entity.digiKeyPartNumber = (allowOverwrite || !entity.digiKeyPartNumber) ? searchResult.supplierPartNumber : entity.digiKeyPartNumber;
      if (entity.packageType?.length === 0) entity.packageType = searchResult.packageType;
      if (entity.datasheetUrl?.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
      if (entity.imageUrl?.length === 0) entity.imageUrl = searchResult.imageUrl;
    }
    
    // map mouser
    searchResult = _.find(metadataParts, (e) => {
      return e !== undefined && e.supplier === "Mouser" && e.manufacturerPartNumber === entity.manufacturerPartNumber;
    });
    if (searchResult) {
      entity.mouserPartNumber = (allowOverwrite || !entity.mouserPartNumber) ? searchResult.supplierPartNumber : entity.mouserPartNumber;
      if (entity.packageType?.length === 0) entity.packageType = searchResult.packageType;
      if (entity.datasheetUrl?.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
      if (entity.imageUrl?.length === 0) entity.imageUrl = searchResult.imageUrl;
    }
    // map arrow
    searchResult = _.find(metadataParts, (e) => {
      return e !== undefined && e.supplier === "Arrow" && e.manufacturerPartNumber === entity.manufacturerPartNumber;
    });
    if (searchResult) {
      entity.arrowPartNumber = (allowOverwrite || !entity.arrowPartNumber) ? searchResult.supplierPartNumber : entity.arrowPartNumber;
      if (entity.packageType?.length === 0) entity.packageType = searchResult.packageType;
      if (entity.datasheetUrl?.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
      if (entity.imageUrl?.length === 0) entity.imageUrl = searchResult.imageUrl;
    }
    // map tme
    searchResult = _.find(metadataParts, (e) => {
      return e !== undefined && e.supplier === "TME" && e.manufacturerPartNumber === entity.manufacturerPartNumber;
    });
    if (searchResult) {
      entity.tmePartNumber = (allowOverwrite || !entity.tmePartNumber) ? searchResult.supplierPartNumber : entity.tmePartNumber;
      if (entity.packageType?.length === 0) entity.packageType = searchResult.packageType;
      if (entity.datasheetUrl?.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
      if (entity.imageUrl?.length === 0) entity.imageUrl = searchResult.imageUrl;
    }
  }

  const mapIfValid = (property, existingValue, newValue, allowOverwrite, newProperty) => {
    if (!newProperty) newProperty = property;
    // map value if:
    // - we are allowing to overwrite an existing value
    // - the existing value is empty and the destination is not
    // - the new value is not empty and the existing value is empty

    if (allowOverwrite) {
      if (newValue[newProperty]) return newValue[newProperty] || ""; // new value if not empty
    }
    if (!existingValue[property]) return newValue[newProperty] || "";
    return existingValue[property] || "";
  };

  const setPartFromMetadata = useCallback((metadataParts, suggestedPart, allowOverwrite = true) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");

    const entity = { ...partRef.current };
    const mappedPart = {
      partNumber: suggestedPart.basePartNumber,
      partTypeId: getPartTypeId(suggestedPart.partType, partTypesRef.current),
      mountingTypeId: suggestedPart.mountingTypeId,
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
      quantity: suggestedPart.quantity,
    };

    if (mappedPart.quantity > 0)
      entity.quantity = mapIfValid("quantity", entity, mappedPart, allowOverwrite);

    entity.partNumber = mapIfValid("partNumber", entity, mappedPart, allowOverwrite);
    entity.supplier = mapIfValid("supplier", entity, mappedPart, allowOverwrite);
    entity.supplierPartNumber = mapIfValid("supplierPartNumber", entity, mappedPart, allowOverwrite);
    entity.partTypeId = mapIfValid("partTypeId", entity, mappedPart, allowOverwrite);
    entity.mountingTypeId = mapIfValid("mountingTypeId", entity, mappedPart, allowOverwrite);
    entity.packageType = mapIfValid("packageType", entity, mappedPart, allowOverwrite);
    entity.cost = mapIfValid("cost", entity, mappedPart, allowOverwrite);
    entity.keywords = mapIfValid("keywords", entity, mappedPart, allowOverwrite);
    entity.description = mapIfValid("description", entity, mappedPart, allowOverwrite);
    entity.manufacturer = mapIfValid("manufacturer", entity, mappedPart, allowOverwrite);
    entity.manufacturerPartNumber = mapIfValid("manufacturerPartNumber", entity, mappedPart, allowOverwrite);
    entity.productUrl = mapIfValid("productUrl", entity, mappedPart, allowOverwrite);
    entity.imageUrl = mapIfValid("imageUrl", entity, mappedPart, allowOverwrite);
    if ((allowOverwrite || !entity.datasheetUrl) && mappedPart.datasheetUrls.length > 0) {
      entity.datasheetUrl = _.first(mappedPart.datasheetUrls) || "";
    }
    mapSupplierPartNumbers(entity, metadataParts, allowOverwrite);
    
    switch(mappedPart.supplier) {
      case "DigiKey":
        entity.digiKeyPartNumber = mapIfValid("digiKeyPartNumber", entity, mappedPart, allowOverwrite, "supplierPartNumber");  
        break;
      case "Mouser":
        entity.mouserPartNumber = mapIfValid("mouserPartNumber", entity, mappedPart, allowOverwrite, "supplierPartNumber");
        break;
      case "Arrow":
        entity.arrowPartNumber = mapIfValid("arrowPartNumber", entity, mappedPart, allowOverwrite, "supplierPartNumber");
        break;
      case "TME":
        entity.tmePartNumber = mapIfValid("tmePartNumber", entity, mappedPart, allowOverwrite, "supplierPartNumber");
        break;
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
    return entity;
  }, []);

  const processPartMetadataResponse = useCallback((data, storedFiles, allowSetFromMetadata, allowOverwrite) => {
    // cancelled or auth required
    if (!data) {
      setLoadingPartMetadata(false);
      return;
    }

    if (data.errors && data.errors.length > 0) {
      setPartMetadataErrors(data.errors);
    }

    let updatedPart = part;
    let metadataParts = [];
    const infoResponse = mergeInfoResponse(data.response, storedFiles);
    if (infoResponse && infoResponse.parts && infoResponse.parts.length > 0) {
      metadataParts = infoResponse.parts;

      const suggestedPart = infoResponse.parts[0];
      // populate the form with data from the part metadata
      if (allowSetFromMetadata) { 
        updatedPart = setPartFromMetadata(metadataParts, { ...suggestedPart, quantity: -1 }, allowOverwrite);
      }
    } else {
      // no part metadata available
      setPartMetadataIsSubscribed(true);
    }

    // set the first datasheet meta display, because the carousel component doesnt fire the first event
    if (infoResponse && infoResponse.datasheets && infoResponse.datasheets.length > 0) setDatasheetMeta(infoResponse.datasheets[0]);

    setInfoResponse(infoResponse);
    setMetadataParts(metadataParts);
    setLoadingPartMetadata(false);
    return { parts: metadataParts, part: updatedPart };
  }, [setPartFromMetadata, part]);

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
    Inventory.doFetchPartMetadataController?.abort();
    Inventory.doFetchPartMetadataController = new AbortController();
    try {
      const response = await fetchApi(`/api/part/info?partNumber=${encodeURIComponent(partNumber.trim())}&partTypeId=${part.partTypeId}&mountingTypeId=${part.mountingTypeId}&supplierPartNumbers=digikey:${part.digiKeyPartNumber || ""},mouser:${part.mouserPartNumber || ""},arrow:${part.arrowPartNumber},tme:${part.tmePartNumber}`, {
        signal: Inventory.doFetchPartMetadataController.signal
      });

      const data = response.data;
      if (data.requiresAuthentication) {
        // notify and redirect for authentication
        if (data.redirectUrl && data.redirectUrl.length > 0) {
          // temporarily store page details and repopulate on return
          setViewPreference('digikey', {
            ...part,
            partNumber: partNumber
          }, { expireOn: addMinutes(new Date(), 5)});

          setAuthorizationApiName(data.apiName);
          setAuthorizationUrl(data.redirectUrl);
          setConfirmAuthIsOpen(true);
          return { data: null, existsInInventory: false };
        }
      }

      let existsInInventory = false;
      if (includeInventorySearch) {
        // also check inventory for this part
        const { exists } = await doInventoryPartSearch(partNumber);
        existsInInventory = exists;
      }

      // let caller handle errors
      return { data, existsInInventory };

    } catch (ex) {
      if (ex?.name === "AbortError") {
        // Continuation logic has already been skipped, so return normally
        return { data: null, existsInInventory: false };
      } else {
        console.error("Exception", ex);
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
    Inventory.doInventoryPartSearchController?.abort();
    Inventory.doInventoryPartSearchController = new AbortController();
    if (partNumber.length < MinSearchKeywordLength)
      return { exists: false, data: null, error: `Ignoring search as keywords are less than the minimum required (${MinSearchKeywordLength}).` };
    const existsResponse = await fetchApi(`/api/part/search?keywords=${encodeURIComponent(partNumber.trim())}&exactMatch=true`, {
      signal: Inventory.doInventoryPartSearchController.signal,
      catchErrors: true
    });
    if (existsResponse.responseObject && existsResponse.responseObject.ok && existsResponse.data !== null) {
      return { exists: true, data: existsResponse.data };
    }
    return { exists: false, data: null };
  };

  /**
   * Lookup a scanned part by it's barcode and return any metadata found
   * @param {object} scannedPart the scanned part object
   * @param {func} onSuccess function to invoke on barcode lookup success
   * @param {func} onFailure function to invoke on barcode lookup failure
   * @returns barcode metadata object
   */
  const doBarcodeLookup = async (scannedPart, onSuccess, onFailure) => {
    toast.dismiss();
    const response = await fetchApi(`/api/part/barcode/info?barcode=${encodeURIComponent(scannedPart.barcode.trim())}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      if (data.requiresAuthentication) {
        // notify and redirect for authentication
        if (data.redirectUrl && data.redirectUrl.length > 0) {
          setAuthorizationApiName(data.apiName);
          setAuthorizationUrl(data.redirectUrl);
          setConfirmAuthIsOpen(true);
          return;
        }
      }
      if (data.errors && data.errors.length > 0) {
        const errorMessage = data.errors.join("\n");
        if (data.errors[0] === "Api is not enabled.") {
          // supress warning for barcode scans
        }
        else
          toast.error(errorMessage);
      } else if (data.response.parts.length > 0) {
        // show the metadata in the UI
        var partInfo = data.response.parts[0];
        onSuccess(partInfo);
      } else {
        // no barcode found
        onFailure(scannedPart);
      }
    }
  };

  const searchDebounced = useMemo(() => debounce(fetchPartMetadataAndInventory, SearchDebounceTimeMs), [pageHasParameters]);

  const validateExistingBarcodeScan = async (input) => {
    Inventory.validateExistingBarcodeScanController?.abort();
    Inventory.validateExistingBarcodeScanController = new AbortController();

    const request = {
      rawScan: input.correctedValue || input.rawValue,
      searchCrc: true
    }
    // check if we have imported this label before
    return await fetchApi(`/api/partScanHistory/search`, {
      signal: Inventory.validateExistingBarcodeScanController.signal,
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request),
    }).then((response) => {
      if (response.responseObject.status === 404) {
        // no history record, proceed
        return true;
      } else if (response.responseObject.ok) {
        // record exists, we have scanned this label before
        return response.data;
      }
      // error
      return false;
    }).catch((ex) => {
      if (ex?.name === "AbortError") {
        // Continuation logic has already been skipped, so return normally
      } else {
        // other error
        const { data } = ex;
        if (data.status === 404) {
          // no history record, proceed
          return true;
        } else {
          console.error('http error', ex);
          toast.error(`Server returned ${data.status} error.`);
        }
      }
      return false;
    });
  };

  // for processing barcode scanner input
  const handleBarcodeInput = useCallback(async (e, input, allowReImport = false) => {
    if (!input?.value) return;
    toast.dismiss();

    let allowQuantityUpdate = true;
    setLastBarcodeScan(input);
    console.debug('barcode input received', input);
    let cleanPartNumber = "";
    if (input.type === "datamatrix") {
      // datamatrix codes contain additional information we can use directly
      // use the manufacturer's part number
      if (input.value.mfgPartNumber && input.value.mfgPartNumber.length > 0) cleanPartNumber = input.value.mfgPartNumber;
      // use the supplier's part number
      else if (input.value.supplierPartNumber && input.value.supplierPartNumber.length > 0) cleanPartNumber = input.value.supplierPartNumber;
      // use the description fallback, which works on older labels
      else if (input.value.description && input.value.description.length > 0) cleanPartNumber = input.value.description;
    } else if (input.type === "code128") {
      // code128 are 1-dimensional codes that only contain a single alphanumeric string, usually a part number
      cleanPartNumber = input.value;
    }

    // if we didn't rerceive a code we can understand, ignore it
    if (!cleanPartNumber || cleanPartNumber.length === 0) {
      console.debug('no clean part number found', cleanPartNumber, input?.value?.quantity);
      if (enableSound) soundFailure.play();
      return;
    }

    // validate if we have already scanned this label before
    const validateResult = await validateExistingBarcodeScan(input);
    if (validateResult === true) {
      // ================================
      // no label scan history, proceed
      // ================================
    } else if (validateResult === false) {
      // error occurred
      toast.error(`An error occurred while validating the barcode.`);
      if (enableSound) soundFailure.play();
      return;
    }

    // barcode scan successful
    if (enableSound) soundSuccess.play();
    
    // add part
    console.debug('clean part number found through barcode', cleanPartNumber, input.value?.quantity);
    if (bulkScanIsOpenRef.current) {
      // bulk scan add part
      setScannedPartsBarcodeInput({ cleanPartNumber, input });
    } else {
      // scan single part
      resetForm("", false, false);
      const scannedPart = {
        partNumber: cleanPartNumber,
        barcode: input.correctedValue
      };
      setInputPartNumber(cleanPartNumber);
      if ((input.value.mfgPartNumber && input.value.mfgPartNumber.length > 0)
        || (input.value.supplierPartNumber && input.value.supplierPartNumber.length > 0)) {
        setPartMetadataIsSubscribed(false);
        setPartMetadataErrors([]);
        // we already have a usable part number, look up its data
        const existingPart = await fetchPart(cleanPartNumber);
        //console.debug('fetchPart', existingPart);

        const labelQuantity = parseInt(input.value?.quantity || "1");
        if (existingPart) {
          // =========================
          // EDIT EXISTING PART
          // =========================

          setIsEditing(true);
          // ================================
          // label has _already_ been scanned
          // ================================
          if (!allowReImport) {
            // confirm do you want to import it again?
            setConfirmReImportAction(() => async (confirmEvent) => await handleBarcodeInput(e, input, true));
            setConfirmReImport(true);
            if (enableSound) soundFailure.play();
            return;
          }

          setInputPartNumber(existingPart.partNumber);
          
          const originalQuantity = existingPart.quantity;
          // add quantity to part
          if (allowQuantityUpdate) {
            existingPart.quantity += labelQuantity;
            setQuantityAdded(labelQuantity);
            console.debug('adding quantity to part', originalQuantity, labelQuantity);
            toast.info(`Added +${labelQuantity} to part "${cleanPartNumber}"`, { autoClose: false });
          }

          // part exists in inventory, switch to edit mode
          setLoadingPartMetadata(true);
          await fetchPartMetadataAndInventory(existingPart.partNumber, existingPart);
          setLoadingPartMetadata(false);
          setIsDirty(true);

          if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: existingPart.quantity });
        } else {
          // =================================
          // ADD AS NEW PART
          // =================================
          console.debug('no existing part, add as new');
          setIsEditing(false);

          // part is not in inventory, add it as new
          setLoadingPartMetadata(true);
          const { data } = await doFetchPartMetadata(cleanPartNumber, part, false);
          const metaResult = processPartMetadataResponse(data, part.storedFiles, true, true);
          setLoadingPartMetadata(false);
          setIsDirty(true);

          // new part being added
          metaResult.part.quantity = labelQuantity;
          setPart(metaResult.part);
          toast.info(`Ready to add new part "${cleanPartNumber}", qty=${labelQuantity}`, { autoClose: false });
        }
      } else {
        // fetch metadata on the barcode if available
        await doBarcodeLookup(scannedPart, async (partInfo) => {
          console.debug("doBarcodeLookup success, getting metadata", cleanPartNumber);
          // barcode found
          if (cleanPartNumber) {
            setPartMetadataIsSubscribed(false);
            setPartMetadataErrors([]);
            if (!isEditing) setPartFromMetadata(metadataParts, { ...partInfo, quantity: partInfo.quantityAvailable }, true);
            if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: partInfo.quantityAvailable });

            // also run a search to get datasheets/images
            await fetchPartMetadataAndInventory(cleanPartNumber, part);
            setIsDirty(true);
          }
        }, async (scannedPart) => {
          console.debug("doBarcodeLookup failed, searching for part", cleanPartNumber);
          // no barcode info found, try searching the part number
          if (cleanPartNumber) {
            setPartMetadataIsSubscribed(false);
            setPartMetadataErrors([]);
            let newQuantity = parseInt(input.value?.quantity) || DefaultQuantity;
            if (isNaN(newQuantity)) newQuantity = 1;
            const newPart = {
              ...part,
              partNumber: cleanPartNumber,
              quantity: newQuantity,
              partTypeId: -1,
              mountingTypeId: -1,
            };
            if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: newPart.quantity });
            setPart(newPart);
            // search for it
            await fetchPartMetadataAndInventory(cleanPartNumber, newPart);
            setIsDirty(true);
          }
        });
      }
      console.debug('barcode processing complete');
    }
  }, [part, isEditing]);

  const formatField = (e) => {
    switch (e.target.name) {
      default:
        break;
      case "cost":
        part.cost = Number(part.cost);
        if (isNaN(part.cost)) part.cost = Number(0).toFixed(2);
        break;
    }
    setPart(part);
  };

  const fetchPart = async (partNumber, partId) => {
    Inventory.fetchPartController?.abort();
    Inventory.fetchPartController = new AbortController();
    setLoadingPart(true);

    let query = `partNumber=${encodeURIComponent(partNumber.trim())}`;
    const validPartId = typeof partId === "number" ? partId : partId && parseInt(partId.trim());
    if (validPartId > 0)
      query += `&partId=${partId}`;

    // this endpoint can return an expected 404
    return await fetchApi(`/api/part?${query}`, {
      signal: Inventory.fetchPartController.signal
    }).then((response) => {
      setLoadingPart(false);
      if (response.responseObject.ok) {
        const { data } = response;
        setPart(data);
        setLoadingPart(false);
        return data;
      }
      return null;
    }).catch((ex) => {
      setLoadingPart(false);
      if (ex?.name === "AbortError") {
        // Continuation logic has already been skipped, so return normally
      } else {
        // other error
        const { data } = ex;
        if (data.status === 404)
          console.info('part not found');
        else {
          console.error('http error', ex);
          toast.error(`Server returned ${data.status} error.`);
        }
      }

      return null;
    });
  };

  const fetchRecentRows = async () => {
    setLoadingRecent(true);
    const response = await fetchApi(`/api/part/list?orderBy=DateCreatedUtc&direction=Descending&results=${maxRecentAddedParts}`);
    const { data } = response;
    setRecentParts(data.items);
    setLoadingRecent(false);
  };

  const fetchPartTypes = async () => {
    setLoadingPartTypes(true);
    const response = await fetchApi("/api/partType/all");
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
          url: `/api/storedFile/local?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId,
          localfile: pi.fileName,
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
          id: pi.storedFileId,
          localfile: pi.fileName,
        }))
      );
    if (storedPinouts && storedPinouts.length > 0)
      infoResponse.pinoutImages.unshift(
        ...storedPinouts.map((pi) => ({
          name: pi.originalFileName,
          value: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
          url: `/api/storedFile/local?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId,
          localfile: pi.fileName,
        }))
      );
    if (storedReferenceDesigns && storedReferenceDesigns.length > 0)
      infoResponse.circuitImages.unshift(
        ...storedReferenceDesigns.map((pi) => ({
          name: pi.originalFileName,
          value: `/api/storedFile/preview?fileName=${pi.fileName}&token=${getImagesToken()}`,
          url: `/api/storedFile/local?fileName=${pi.fileName}&token=${getImagesToken()}`,
          id: pi.storedFileId,
          localfile: pi.fileName,
        }))
      );
    return infoResponse;
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

    const request = { 
      ...part,
      partNumber: inputPartNumber.trim(),
      partTypeId: (parseInt(part.partTypeId) || 0) + "",
      mountingTypeId: (parseInt(part.mountingTypeId) || 0) + "",
      quantity: parseInt(part.quantity) || 0,
      lowStockThreshold: parseInt(part.lowStockThreshold) || 0,
      cost: parseFloat(part.cost) || 0.0,
      projectId: parseInt(part.projectId) || null,
      currency: part.currency || systemSettings.currency || 'USD',
      barcodeObject: lastBarcodeScan
    };

    toast.dismiss();

    if (request.partNumber.length === 0) {
      toast.error("Part Number is empty!");
      return;
    }

    removeViewPreference('digikey');
    const saveMethod = isExisting ? "PUT" : "POST";
    const response = await fetchApi("/api/part", {
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
      const { data } = response;
      setDuplicateParts(data.parts);
      setDuplicatePartModalOpen(true);
    } else if (response.responseObject.status === 200) {
      // reset the last barcode scan
      setLastBarcodeScan(null);
      // save success
      if (isExisting) {
        saveMessage = t('message.savedPart', "Saved part {{partNumber}}!", { partNumber: request.partNumber });
        setSaveMessage(saveMessage);
        toast.info(saveMessage);
      } else {
        // reset form if it was a new part
        saveMessage = t('message.addedPart', "Added part {{partNumber}}!", { partNumber: request.partNumber });
        resetForm(saveMessage);
        toast.success(saveMessage);
      }
      setQuantityAdded(0);
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

  const resetForm = (saveMessage = "", clearAll = false, dismiss = true) => {
    if (dismiss) toast.dismiss();

    removeViewPreference('digikey');
    setIsDirty(false);
    setIsEditing(false);
    setPartExistsInInventory(false);
    setSaveMessage(saveMessage);
    setMetadataParts([]);
    setDuplicateParts([]);
    setPartMetadataIsSubscribed(false);
    setInputPartNumber("");
    setQuantityAdded(0);
    const clearedPart = {
      partId: 0,
      partNumber: "",
      allowPotentialDuplicate: false,
      quantity: (clearAll || !viewPreferences.rememberLast) ? DefaultQuantity : viewPreferences.lastQuantity || DefaultQuantity,
      lowStockThreshold: (clearAll || !viewPreferences.rememberLast) ? DefaultLowStockThreshold : viewPreferences.lowStockThreshold || DefaultLowStockThreshold,
      partTypeId: (clearAll || !viewPreferences.rememberLast) ? IcPartType : viewPreferences.lastPartTypeId || IcPartType,
      mountingTypeId: (clearAll || !viewPreferences.rememberLast) ? DefaultMountingTypeId : viewPreferences.lastMountingTypeId || DefaultMountingTypeId,
      packageType: "",
      keywords: "",
      description: "",
      datasheetUrl: "",
      digiKeyPartNumber: "",
      mouserPartNumber: "",
      arrowPartNumber: "",
      tmePartNumber: "",
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
      supplier: "",
      supplierPartNumber: "",
      symbolName: "",
      footprintName: "",
      extensionValue1: "",
      extensionValue2: "",
    };
    setPart(clearedPart);
    setLoadingPartMetadata(false);
    setLoadingPartTypes(false);
    setInfoResponse({});
    if (clearAll && viewPreferences.rememberLast) {
      updateViewPreferences({ lastQuantity: clearedPart.quantity, lowStockThreshold: clearedPart.lowStockThreshold, lastPartTypeId: clearedPart.partTypeId, lastMountingTypeId: clearedPart.mountingTypeId });
    }
    document.getElementById('inputPartNumber').focus();
  };

  const clearForm = (e) => {
    // e could be null as this special method can be called outside of the component without a synthetic event
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (isDirty) {
      setConfirmDiscardAction(() => () => { resetForm("", true); });
      setConfirmDiscardChanges(true);
      return;
    }

    resetForm("", true);

    if (rest.params.partNumber) {
      navigate("/inventory/add");
      return;
    }
  };

  const updateNumberPicker = (e) => {
    if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: parseInt(e.value) || DefaultQuantity });
    setPart({ ...part, quantity: e.value.toString() });
    setIsDirty(true);
  };

  const updateViewPreferences = (preference) => {
    const newViewPreferences = { ...viewPreferences, ...preference };
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
    setPartMetadataIsSubscribed(false);
    setPartMetadataErrors([]);
    let searchPartNumber = control.value || '';

    if (searchPartNumber && searchPartNumber.length >= MinSearchKeywordLength) {
      searchPartNumber = control.value.replace("\t", "");
      await searchDebounced(searchPartNumber, part, partTypes);
      setIsDirty(true);
    }

    setInputPartNumber(searchPartNumber);

    // wont work unless we update render
    //setRenderIsDirty(!renderIsDirty);
  };

  const handlePartTypeChange = (e, partType, referencedPart) => {
    if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastPartTypeId: partType.partTypeId });
    const updatedPart = { ...partRef.current, partTypeId: partType.partTypeId };
    setPart(updatedPart);
    setIsDirty(true);
  };

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setPartMetadataIsSubscribed(false);
    setPartMetadataErrors([]);
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
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastPartTypeId: control.value });
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber, updatedPart, partTypes);
        break;
      case "mountingTypeId":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastMountingTypeId: control.value });
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber, updatedPart, partTypes);
        break;
      case "quantity":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ quantity: parseInt(control.value) || DefaultQuantity });
        break;
      case "lowStockThreshold":
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lowStockThreshold: control.value });
        break;
      case "location":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastLocation: updatedPart[control.name] });
        break;
      case "binNumber":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastBinNumber: updatedPart[control.name] });
        break;
      case "binNumber2":
        updatedPart[control.name] = control.value.replace("\t", "");
        if (viewPreferences.rememberLast && !isEditing) updateViewPreferences({ lastBinNumber2: updatedPart[control.name] });
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
    if (systemSettings.printer.printMode === 0) {
      // direct print
      await fetchApi(`/api/part/print?partNumber=${encodeURIComponent(part.partNumber.trim())}&partId=${part.partId}&generateImageOnly=false`, { method: "POST" });
    } else {
      window.print();
    }

  };

  const handleChooseAlternatePart = (e, part) => {
    setPartFromMetadata(metadataParts, part, true);
  };

  const handleBulkBarcodeScan = (e) => {
    e.preventDefault();
    setBulkScanIsOpen(true);
  };

  const disableHelp = () => {
    // const { viewPreferences } = state;
    // const val = { ...viewPreferences, helpDisabled: true };
    // localStorage.setItem('viewPreferences', JSON.stringify(val));
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const handleRecentPartClick = async (e, part) => {
    setPart(part);
    if (part.partId)
      rest.history(`/inventory/${encodeURIComponent(part.partNumber)}:${part.partId}`);
    else
      rest.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
    await fetchPart(part.partNumber, part.partId);
  };

  const handleSaveScannedParts = async (e, scannedParts) => {
    e.preventDefault();
    e.stopPropagation();
    if (scannedParts.length === 0)
      return true;
    toast.dismiss();
    setBulkScanSaving(true);
    const request = {
      parts: scannedParts
    };
    const response = await fetchApi("/api/part/bulk", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      toast.success(t('message.addXParts', "Added {{added}} parts, updated {{updated}} parts.", { added: data.added.length, updated: data.updated.length }));
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
    await fetchApi(`/api/part`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ partId: selectedPart.partId })
    });
    const partsDeleted = _.without(parts, _.findWhere(parts, { partId: selectedPart.partId }));
    setConfirmDeletePartIsOpen(false);
    setParts(partsDeleted);
    setSelectedPart(null);
    rest.history(`/inventory`);
  };

  const handleRefreshPart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    setLoadingPartMetadata(true);
    setConfirmRefreshPartIsOpen(false);
    const { data } = await doFetchPartMetadata(part.partNumber, part, false);
    processPartMetadataResponse(data, part.storedFiles, true, true);
    setLoadingPartMetadata(false);
    setIsDirty(true);
    if (confirmRefreshPartDoNotAskAgain) {
      setViewPreference('confirmRefreshPartDoNotAskAgain', confirmRefreshPartDoNotAskAgain);
    }
  };

  const openDeletePart = (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    setSelectedPart(part);
    setConfirmDeletePartContent(
      <p>
        <Trans i18nKey="confirm.deletePart" name={inputPartNumber}>
          Are you sure you want to delete part <b>{{ name: inputPartNumber }}</b>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
    setConfirmDeletePartIsOpen(true);
  };

  const openRefreshPart = async (e) => {
    if (getViewPreference('confirmRefreshPartDoNotAskAgain')) {
      await handleRefreshPart(e);
      return;
    }
    setConfirmRefreshPartContent(
      <>
      <p>
        <Trans i18nKey="confirm.refreshPart" name={inputPartNumber}>
          Are you sure you want to refresh part information for <b>{{ name: inputPartNumber }}</b>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.overwriteExistingContent">
          Existing values for fields provided by external APIs will be overwritten.
        </Trans>
      </p>
      </>      
    );
    setConfirmRefreshPartIsOpen(true);
  };

  const closeDeletePart = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeletePartIsOpen(false);
    setSelectedPart(null);
  };

  const closeRefreshPart = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmRefreshPartIsOpen(false);
    if (confirmRefreshPartDoNotAskAgain) {
      setViewPreference('confirmRefreshPartDoNotAskAgain', confirmRefreshPartDoNotAskAgain);
    }
  };

  const handleSetSuggestedPartNumber = (e, value) => {
    e.preventDefault();
    e.stopPropagation();
    setInputPartNumber(value);
  };

  const handleAuthRedirect = (e) => {
    e.preventDefault();
    window.location.href = authorizationUrl;
  };

  const handleCancelAuthRedirect = (e) => {
    setConfirmAuthIsOpen(false);
    removeViewPreference('digikey');
  };

  const handleInputPartNumberClear = (e) => {
    removeViewPreference('digikey');
  };

  /* RENDER */
  const title = isEditing
    ? t('page.inventory.edittitle', "Edit Inventory")
    : t('page.inventory.addtitle', "Add Inventory");

  /*<MatchingPartsMemoized part={part} metadataParts={metadataParts} partTypes={partTypes} setPartFromMetadata={setPartFromMetadata} />*/
  const renderForm = useMemo(() => {
    return (
      <>
        <div className="page-banner">
          {partMetadataIsSubscribed && (
            <div className="page-notice" onClick={() => setPartMetadataIsSubscribed(false)}>
              <div>
                <Icon name="close" />
                <Trans i18nKey="message.noPartInfo" partNumber={inputPartNumber}>
                  No part information is available for '{{ partNumber: inputPartNumber }}'. You are subscribed to updates and will be automatically updated when the part is indexed.
                </Trans>
              </div>
            </div>
          )}
          {partMetadataErrors?.length > 0 && (
            partMetadataErrors.map((error, key) => (
              <div key={key} className="page-error" onClick={() => setPartMetadataErrors(_.filter(partMetadataErrors, i => i !== error))}>
                <Icon name="close" /> {error}
              </div>
            ))
          )}
          {systemSettings?.digikey?.enabled && systemSettings?.digikey?.apiUrl?.includes('sandbox') && (
            <div className="page-notice" onClick={() => navigate('/settings')}>
              <Icon name="warning" />
              <Trans i18nKey="message.sandbox">
                You are currently using the DigiKey Sandbox - part numbers returned by the DigiKey API may be fictitious or based on other parts.
              </Trans>
            </div>
          )}
        </div>

        <Dimmer.Dimmable dimmed={loadingPart}>
          <Dimmer active={loadingPart} inverted={true}><Loader style={{ top: '150px', height: '200px' }}>Loading part...</Loader></Dimmer>
          <Grid celled className={`inventory-container ${disableRendering.current ? 'render-disabled' : ''}`}>
            <Grid.Row>
              <Grid.Column width={12} className="left-column" style={{ minHeight: '1100px' }}>
                {/** LEFT COLUMN */}
                <div style={{ minHeight: '370px' }}>
                  <div style={{ minHeight: '325px' }}>
                    <Form.Group style={{ marginBottom: '0' }}>
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
                          hideIcon
                          clearOnScan={false}
                          onClear={handleInputPartNumberClear}
                          onBarcodeReadStarted={(e) => { window.requestAnimationFrame(() => { disableRendering.current = true; }); searchDebounced.cancel(); }}
                          onBarcodeReadCancelled={(e) => { window.requestAnimationFrame(() => { disableRendering.current = false; }); searchDebounced.cancel(); }}
                          onBarcodeReadReceived={(e) => { window.requestAnimationFrame(() => { disableRendering.current = false; }); searchDebounced.cancel(); }}
                        />
                        {!isEditing && !partExistsInInventory && part.partNumber && part.partNumber !== inputPartNumber &&
                          <div className="suggested-part">
                            {<span>{t('page.inventory.suggestedPart')}: <button className="link-button" onClick={e => handleSetSuggestedPartNumber(e, part.partNumber)}>{part.partNumber}</button></span>}
                          </div>}
                        <div className="suggested-part">
                          {!isEditing && partExistsInInventory &&
                            <span><Icon name="warning sign" color="yellow" />
                              <Trans i18nKey="page.inventory.partExists">
                                This <Link to={`/inventory/${encodeURIComponent(inputPartNumber)}`}>part</Link> <span>already exists</span> in your inventory.
                              </Trans>
                            </span>}
                        </div>
                      </Form.Field>
                      {!disableRendering.current && <>
                        <Form.Field width={6}>
                          {/* PartTypeSelectorMemoized is internally memoized for performance */}
                          <PartTypeSelectorMemoized
                            label={t('label.partType', "Part Type")}
                            name="partTypeId"
                            value={part.partTypeId || ""}
                            partTypes={allPartTypes}
                            onSelect={(e, partType) => handlePartTypeChange(e, partType, part)}
                            loadingPartTypes={loadingPartTypes}
                          />
                        </Form.Field>
                        <Form.Dropdown
                          label={t('label.mountingType', "Mounting Type")}
                          placeholder={t('label.mountingType', "Mounting Type")}
                          search
                          selection
                          value={(part.mountingTypeId || "")}
                          options={mountingTypeOptions}
                          onChange={handleChange}
                          name="mountingTypeId"
                        />
                      </>}
                    </Form.Group>

                    {!disableRendering.current && <>
                      <Form.Group>
                        <div>
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
                                value={part.quantity || 0}
                                onChange={updateNumberPicker}
                                name="quantity"
                                autoComplete="off"
                              />
                            }
                          />
                          <div className="quantityAdded">{quantityAdded > 0 ? <div className="suggested-part"><Icon name="plus" />{quantityAdded} <span>qty added</span></div> : (<></>)}</div>
                        </div>
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
                    </>}

                    {/* Part Location Information */}
                    {!disableRendering.current && <Segment secondary>
                      <Form.Group>
                        <Popup
                          hideOnScroll
                          disabled={viewPreferences.helpDisabled}
                          onOpen={disableHelp}
                          content={t('page.inventory.popup.location', "A custom value for identifying the parts location")}
                          trigger={
                            <ClearableInput
                              label={t('label.location', "Location")}
                              placeholder="Home lab"
                              value={part.location || ""}
                              onChange={handleChange}
                              name="location"
                              width={5}
                              icon="home"
                              iconPosition="left"
                            />
                          }
                        />
                        <Popup
                          hideOnScroll
                          disabled={viewPreferences.helpDisabled}
                          onOpen={disableHelp}
                          content={t('page.inventory.popup.binNumber', "A custom value for identifying the parts location")}
                          trigger={
                            <ClearableInput
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
                            <ClearableInput
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
                    </Segment>}
                  </div>

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
                        trigger={<Checkbox toggle label={t('label.rememberLastSelection', "Remember last selection")} className="left small" style={{ float: 'right' }} checked={viewPreferences.rememberLast || false} onChange={handleRememberLastSelection} />}
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
                      <Button type="button" title="Delete" onClick={(e) => openDeletePart(e, part)} style={{ marginLeft: "10px" }}>
                        <Icon name="delete" />
                        {t('button.delete', "Delete")}
                      </Button>
                    )}
                    {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
                  </Form.Field>
                  <div className="sticky-target" style={{ padding: '10px 10px 20px 10%' }} data-bounds={"0.20,0.55"}>
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
                </div>

                {/* PART METADATA */}

                {!disableRendering.current && <Segment loading={loadingPartMetadata} color="blue">
                  <div style={{ display: 'flex', alignItems: 'stretch', flexDirection: 'row', paddingBottom: '0.22rem', borderBottom: '1px solid rgba(34,36,38,0.15)', marginBottom: '5px' }}>
                    <div style={{ display: 'flex', flex: '1' }}>
                      <h3 className="ui header">
                        {t('page.inventory.partMetadata', "Part Metadata")}
                      </h3>
                    </div>
                    <div style={{ position: 'relative' }}>
                      <div style={{position: 'absolute', right: '-10px', top: '-10px', width: '500px', textAlign: 'right'}}>
                        <Popup
                          hideOnScroll
                          disabled={viewPreferences.helpDisabled}
                          onOpen={disableHelp}
                          content={t('page.inventory.popup.apiRefresh', "Refresh the part information from external APIs. All API provided fields will be overwritten.")}
                          trigger={
                            <Button type="button" style={{ fontSize: '0.9em', width: '115px', padding: '0.68em 1.5em', position: 'relative' }} disabled={!part.partNumber} onClick={openRefreshPart}>
                              <Icon name="refresh" color="blue" />
                              {t('page.inventory.refresh', "Refresh")}
                            </Button>
                          }
                        />                        
                        {metadataParts && metadataParts.length > 1 && (
                          <ChooseAlternatePartModal
                            trigger={
                              <Popup
                                hideOnScroll
                                disabled={viewPreferences.helpDisabled}
                                onOpen={disableHelp}
                                content={t('page.inventory.popup.alternateParts', "Choose a different part to extract metadata information from. By default, Binner will give you the most relevant part and with the highest quantity available.")}
                                trigger={
                                  <Button type="button" secondary style={{ fontSize: '0.9em', width: '265px', padding: '0.68em 1.5em', position: 'relative' }}>
                                    <Icon name="external alternate" color="blue" />
                                    {t('page.inventory.chooseAlternatePart', "Choose alternate part ({{count}})", { count: formatNumber(metadataParts.length) })}
                                  </Button>
                                }
                              />
                            }
                            part={part}
                            metadataParts={metadataParts}
                            onPartChosen={(e, p) => handleChooseAlternatePart(e, p, partTypes)}
                          />
                        )}
                      </div>
                    </div>
                  </div>

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
                        width={4}
                      >
                        <Dropdown
                          name="currency"
                          className="label currency"
                          placeholder="$"
                          value={part.currency || systemSettings.currency || 'USD'}
                          options={currencyOptions}
                          onChange={handleChange}
                        />
                        <input />
                      </Input>
                    </Form.Field>
                    <ClearableInput
                      label={t('label.manufacturer', "Manufacturer")}
                      placeholder="Texas Instruments"
                      value={part.manufacturer || ""}
                      onChange={handleChange}
                      name="manufacturer"
                      width={5}
                    />
                    <ClearableInput
                      label={t('label.manufacturerPart', "Manufacturer Part")}
                      placeholder="LM358"
                      value={part.manufacturerPartNumber || ""}
                      onChange={handleChange}
                      name="manufacturerPartNumber"
                      width={5}
                    />
                    <Form.Field style={{ display: 'flex', justifyContent: 'end', flex: '1' }}>
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
                  <Form.Group>
                    <Form.Field width={6}>
                      <label>{t('label.packageType', "Package Type")}</label>
                      <ClearableInput
                        placeholder="DIP8"
                        value={part.packageType || ""}
                        onChange={handleChange}
                        name="packageType"
                      />
                    </Form.Field>
                    <Form.Field width={10} className="part-metadata-buttons">
                      <label>&nbsp;</label>
                      <Popup
                        content={<p>{t('page.inventory.popup.technicalSpecs', "View technical specs")}</p>}
                        trigger={<Button type="button" secondary disabled><TextSnippet className="technical-specs" /> {t('button.specs', "Specs")}</Button>}
                      />
                      <Popup
                        content={<p>{t('page.inventory.popup.compliance', "View compliance information")}</p>}
                        trigger={<Button type="button" secondary disabled><BeenhereIcon className="compliance" /> {t('button.compliance', "Compliance")}</Button>}
                      />
                      <Popup
                        content={<p>{t('page.inventory.popup.cadModels', "View CAD Models available for this part")}</p>}
                        trigger={<Button type="button" secondary disabled><ViewInArIcon className="cadModels" /> {t('button.cadModels', "CAD Models")}</Button>}
                      />
                    </Form.Field>
                  </Form.Group>
                  <Form.Field>
                    <label>{t('label.primaryDatasheetUrl', "Primary Datasheet Url")}</label>
                    <ClearableInput type="Input" action className='labeled' placeholder='ti.com/lit/ds/symlink/lm2904-n.pdf' value={(part.datasheetUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='datasheetUrl'>
                      <Label>https://</Label>
                      <input />
                      <Button onClick={e => handleVisitLink(e, part.datasheetUrl)} disabled={!part.datasheetUrl || part.datasheetUrl.length === 0}>{t('button.view', "View")}</Button>
                    </ClearableInput>
                  </Form.Field>
                  <Form.Field>
                    <label>{t('label.productUrl', "Product Url")}</label>
                    <ClearableInput type="Input" action className='labeled' placeholder='digikey.ca/en/products/detail/texas-instruments/LM2904DR/555718' value={(part.productUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='productUrl'>
                      <Label>https://</Label>
                      <input />
                      <Button onClick={e => handleVisitLink(e, part.productUrl)} disabled={!part.productUrl || part.productUrl.length === 0}>{t('button.visit', "Visit")}</Button>
                    </ClearableInput>
                  </Form.Field>
                  <Form.Group>
                    <Form.Field width={4}>
                      <label>{t('label.digikeyPartNumber', "DigiKey Part Number")}</label>
                      <ClearableInput placeholder='296-1395-5-ND' value={part.digiKeyPartNumber || ''} onChange={handleChange} name='digiKeyPartNumber' />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.mouserPartNumber', "Mouser Part Number")}</label>
                      <ClearableInput placeholder='595-LM358AP' value={part.mouserPartNumber || ''} onChange={handleChange} name='mouserPartNumber' />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.arrowPartNumber', "Arrow Part Number")}</label>
                      <ClearableInput placeholder='595-LM358AP' value={part.arrowPartNumber || ''} onChange={handleChange} name='arrowPartNumber' />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.tmePartNumber', "TME Part Number")}</label>
                      <ClearableInput placeholder='LM358PWR' value={part.tmePartNumber || ''} onChange={handleChange} name='tmePartNumber' />
                    </Form.Field>
                  </Form.Group>
                </Segment>}

                {/* Part Preferences */}
                {!disableRendering.current && <Segment loading={loadingPartMetadata} color="green">
                  <Header dividing as="h3">
                    {t('page.inventory.privatePartInfo', "Private Part Information")}
                  </Header>
                  <p>{t('page.inventory.privatePartInfoMessage', "These values can be set manually and will not be synchronized automatically via apis.")}</p>

                  <Form.Group>
                    <Form.Field width={4}>
                      <label>{t('label.symbolName', "KiCad Symbol Name")}</label>
                      <Popup
                        content={<p>{t('page.inventory.popup.symbolName', "Associate a KiCad symbol name with this part")}</p>}
                        trigger={<ClearableInput placeholder='R_0601' value={part.symbolName || ''} onChange={handleChange} name='symbolName' />}
                      />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.footprintName', "KiCad Footprint Name")}</label>
                      <Popup
                        content={<p>{t('page.inventory.popup.footprintName', "Associate a KiCad footprint name with this part")}</p>}
                        trigger={<ClearableInput placeholder='Resistor SMD 0601_1608Matric' value={part.footprintName || ''} onChange={handleChange} name='footprintName' />}
                      />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.extensionValue1', "Extension Value 1")}</label>
                      <Popup
                        content={<p>{t('page.inventory.popup.extensionValue', "Associate a custom value with this part")}</p>}
                        trigger={<ClearableInput placeholder='' value={part.extensionValue1 || ''} onChange={handleChange} name='extensionValue1' />}
                      />
                    </Form.Field>
                    <Form.Field width={4}>
                      <label>{t('label.extensionValue2', "Extension Value 2")}</label>
                      <Popup
                        content={<p>{t('page.inventory.popup.extensionValue', "Associate a custom value with this part")}</p>}
                        trigger={<ClearableInput placeholder='' value={part.extensionValue2 || ''} onChange={handleChange} name='extensionValue2' />}
                      />
                    </Form.Field>
                  </Form.Group>
                </Segment>}

              </Grid.Column>

              <Grid.Column width={4} className="right-column">
                {/** RIGHT COLUMN */}

                {!disableRendering.current && <PartMediaMemoized part={part} infoResponse={infoResponse} datasheet={datasheetMeta} loadingPartMetadata={loadingPartMetadata} />}

                {/* END RIGHT COLUMN */}
              </Grid.Column>
            </Grid.Row>
          </Grid>

          {/* Suppliers */}

          {!disableRendering.current && <PartSuppliersMemoized
            loadingPartMetadata={loadingPartMetadata}
            part={part}
            metadataParts={metadataParts}
          />}
        </Dimmer.Dimmable>

      </>);
  }, [inputPartNumber, part, viewPreferences.rememberLast, loadingPart, loadingPartMetadata, partMetadataErrors, isEditing, allPartTypes, isDirty, handlePartTypeChange]);

  const handleCancelDiscard = (e) => {
    setConfirmDiscardAction(null);
    setConfirmDiscardChanges(false);  // close confirm
    if (blocker.reset) blocker.reset(); 
  }

  const handleConfirmDiscard = async (e) => {
    // re-run command by executing the action set
    if (confirmDiscardAction) await confirmDiscardAction(e);
    setConfirmDiscardAction(null);
    setConfirmDiscardChanges(false);  // close confirm
    if (blocker.proceed) blocker.proceed();
  };

  const handleCancelReImport = (e) => {
    setConfirmReImportAction(null);
    setConfirmReImport(false); // close confirm
  }

  const handleConfirmReImport = async (e) => {
    // re-run command by executing the action set
    setConfirmReImport(false);  // close confirm
    if (confirmReImportAction) await confirmReImportAction(e);
    setConfirmReImportAction(null);
  };

  return (
    <div className="inventory mask">
      <DuplicatePartModal
        isOpen={duplicatePartModalOpen}
        part={part}
        duplicateParts={duplicateParts}
        onSetPart={setPart}
        onSubmit={onSubmit}
      />
      <Confirm
        className="confirm"
        header={t('confirm.header.deletePart', "Delete Part")}
        open={confirmDeletePartIsOpen}
        onCancel={closeDeletePart}
        onConfirm={handleDeletePart}
        content={confirmDeletePartContent}
      />
      <Confirm
        className="confirm"
        header={t('confirm.header.refreshPart', "Refresh Part Information")}
        open={confirmRefreshPartIsOpen}
        onCancel={closeRefreshPart}
        onConfirm={handleRefreshPart}
        content={confirmRefreshPartContent}
        cancelButton={<><Checkbox label={t('confirm.doNotAskAgain', "Do not ask again")} style={{paddingRight: '20px'}} checked={confirmRefreshPartDoNotAskAgain} onChange={() => setConfirmRefreshPartDoNotAskAgain(!confirmRefreshPartDoNotAskAgain)}/><Button content={t('button.cancel', "Cancel")} onClick={(e) => closeRefreshPart(e)} /></>}
      />
      <Confirm
        className="confirm"
        header={t('page.settings.confirm.mustAuthenticateHeader', "Must Authenticate")}
        open={confirmAuthIsOpen}
        onCancel={handleCancelAuthRedirect}
        onConfirm={handleAuthRedirect}
        content={<p>
          <Trans i18nKey="page.settings.confirm.mustAuthenticate" name={authorizationApiName}>
            External Api (<b>{{ name: authorizationApiName }}</b>) is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.
          </Trans>
        </p>
        }
      />
      <Confirm
        header={<div className="header"><Icon name="undo" color="grey" /> {t('confirm.discardChanges', "You have unsaved changes.")}</div>}
        open={blocker.state === "blocked" || confirmDiscardChanges}
        confirmButton={t('button.discard', "Discard")}
        cancelButton={t('button.noTakeMeBack', "No, take me back!")}
        content={
          <p style={{ padding: "20px", fontSize: '1.2em', textAlign: "center" }}>
            <span style={{ color: '#666' }}>{t('confirm.unsaved', "You have unsaved changes.")}</span>
            <br />
            <br />
            {t('confirm.confirmDiscard', "Are you sure you want to discard these changes?")}
          </p>
        }
        onCancel={handleCancelDiscard}
        onConfirm={handleConfirmDiscard}
      />
      <Confirm
        header={<div className="header"><Icon name="undo" color="grey" /> {t('confirm.importLabel', "Import Label")}</div>}
        open={confirmReImport}
        confirmButton={t('button.import', "Import")}
        cancelButton={t('button.cancel', "Cancel")}
        content={
          <p style={{ padding: "20px", fontSize: '1.2em', textAlign: "center" }}>
            <span style={{ color: '#666' }}>{t('confirm.alreadyImportedLabel', "You have already imported this label.")}</span>
            <br />
            <br />
            {t('confirm.confirmReImport', "Do you want to import this label again?")}
          </p>
        }
        onCancel={handleCancelReImport}
        onConfirm={handleConfirmReImport}
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
        <BarcodeScannerInput onReceived={handleBarcodeInput} minInputLength={4} swallowKeyEvent={false} enableSound={false} />
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
        <Breadcrumb>
          <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
          <Breadcrumb.Divider />
          <Breadcrumb.Section link onClick={() => navigate("/inventory")}>{t('bc.inventory', "Inventory")}</Breadcrumb.Section>
          <Breadcrumb.Divider />
          <Breadcrumb.Section active>{isEditing ? part.partNumber : t('page.inventory.addtitle', "Add Inventory")}</Breadcrumb.Section>
        </Breadcrumb>
        {part.partNumber && <Image src={`/api/part/preview?partNumber=${encodeURIComponent(part.partNumber.trim())}&partId=${part.partId}&token=${getImagesToken()}`} id="printarea" width={180} floated="right" style={{ marginTop: "0px" }} />}
        <div style={{ display: 'flex' }}>
          <FormHeader name={title} to=".." />
          {!isEditing &&
            <Popup
              wide
              position="right center"
              content={<p>{t('page.inventory.popup.bulkAddParts', "Bulk add parts using a barcode scanner")}</p>}
              trigger={<div style={{ paddingTop: '10px' }}><BulkScanIconMemoized onClick={handleBulkBarcodeScan} /></div>}
            />
          }
        </div>
        {renderForm}
        {/** internally memoized */}

        {!disableRendering.current && <RecentParts
          recentParts={recentParts}
          loadingRecent={loadingRecent}
          handleRecentPartClick={handleRecentPartClick}
        />}
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
