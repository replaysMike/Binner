import React, { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
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
import { ChooseAlternatePartModal } from "../components/ChooseAlternatePartModal";
import { PartMediaMemoized } from "../components/PartMediaMemoized";
import { BulkScanModal } from "../components/BulkScanModal";
import { BulkScanIconMemoized } from "../components/BulkScanIconMemoized";
import { RecentParts } from "../components/RecentParts";
import { PartSuppliersMemoized } from "../components/PartSuppliersMemoized";
import { MatchingPartsMemoized } from "../components/MatchingPartsMemoized";
import { DuplicatePartModal } from "../components/DuplicatePartModal";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData, removeLocalData } from "../common/storage";
import { formatNumber } from "../common/Utils";
import { getPartTypeId } from "../common/partTypes";
import { getImagesToken } from "../common/authentication";
import { StoredFileType } from "../common/StoredFileType";
import { MountingTypes, GetAdvancedTypeDropdown } from "../common/Types";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import { Currencies } from "../common/currency";
import { getSystemSettings } from "../common/applicationSettings";
import "./Inventory.css";

export function Inventory(props) {
  const SearchDebounceTimeMs = 500;
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

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'inventory' });
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
  const pageHasParameters = props.params?.partNumber?.length > 0;
  const defaultPart = {
    partId: 0,
    partNumber: props.params.partNumber || "",
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

  const [inputPartNumber, setInputPartNumber] = useState(props.params.partNumberToAdd || "");
  const [infoResponse, setInfoResponse] = useState({});
  const [parts, setParts] = useState([]);
  const [part, setPart] = useState(defaultPart);
  const [isEditing, setIsEditing] = useState((part && part.partId > 0) || pageHasParameters);
  const [isDirty, setIsDirty] = useState(false);
  const [selectedPart, setSelectedPart] = useState(null);
  const [recentParts, setRecentParts] = useState([]);
  const [metadataParts, setMetadataParts] = useState([]);
  const [duplicateParts, setDuplicateParts] = useState([]);
  const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
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

  // todo: find a better alternative, we shouldn't need to do this!
  const partRef = useRef();
  partRef.current = part;
  const bulkScanIsOpenRef = useRef();
  bulkScanIsOpenRef.current = bulkScanIsOpen;
  const partTypesRef = useRef();
  partTypesRef.current = partTypes;

  useEffect(() => {
    const partNumberRaw = props.params.partNumber;
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
    // restore input data on load, then remove it
    const digikeyTempSettings = getViewPreference('digikey');
    if (digikeyTempSettings?.partNumber) {
      partNumberStr = digikeyTempSettings.partNumber;
      removeViewPreference('digikey');
    }

    const fetchData = async () => {
      setPartMetadataIsSubscribed(false);
      setPartMetadataErrors([]);
      await fetchPartTypes();
      await fetchRecentRows();
      if (partNumberStr) {
        var loadedPart = await fetchPart(partNumberStr, partId);
        if (newIsEditing) setInputPartNumber(partNumberStr);
        setInputPartNumber(partNumberStr);
        await fetchPartMetadataAndInventory(partNumberStr, loadedPart || part);
      } else if (props.params.partNumberToAdd) {
        const { data } = await doFetchPartMetadata(props.params.partNumberToAdd, loadedPart || part, false);
        processPartMetadataResponse(data, loadedPart || part);
        setIsDirty(true);
      } else {
        resetForm();
      }
    };
    fetchData().catch(console.error);
    getSystemSettings().then((systemSettings) => {
      setSystemSettings(systemSettings);
    });
    return () => {
      searchDebounced.cancel();
    };
  }, [props.params.partNumber]);

  /*useEffect(() => {
    if (!props.params.partNumberToAdd) {
      resetForm();
    }
  }, [props.params.partNumberToAdd]);*/

  const fetchPartMetadataAndInventory = async (input, localPart) => {
    if (partTypesRef.current.length === 0)
      console.error("There are no partTypes! This shouldn't happen and is a bug.");
    if (input.trim().length < MinSearchKeywordLength)
      return;
    Inventory.infoAbortController.abort();
    Inventory.infoAbortController = new AbortController();
    setLoadingPartMetadata(true);
    setPartMetadataIsSubscribed(false);
    setPartMetadataErrors([]);
    try {
      const includeInventorySearch = !pageHasParameters;
      const { data, existsInInventory } = await doFetchPartMetadata(input, localPart, includeInventorySearch);
      if (existsInInventory) setPartExistsInInventory(true);

      processPartMetadataResponse(data, localPart);
    } catch (ex) {
      console.error("Exception", ex);
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const setPartFromMetadata = useCallback((metadataParts, suggestedPart) => {
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
      entity.quantity = mappedPart.quantity;

    entity.partNumber = mappedPart.partNumber;
    entity.supplier = mappedPart.supplier;
    entity.supplierPartNumber = mappedPart.supplierPartNumber;
    if (mappedPart.partTypeId) entity.partTypeId = mappedPart.partTypeId || "";
    if (mappedPart.mountingTypeId) entity.mountingTypeId = mappedPart.mountingTypeId || "";
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
      // also map tme
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "TME" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.tmePartNumber = searchResult.supplierPartNumber;
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
      // also map tme
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "TME" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.tmePartNumber = searchResult.supplierPartNumber;
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
      // also map tme
      searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "TME" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.tmePartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }
    if (mappedPart.supplier === "TME") {
      entity.tmePartNumber = mappedPart.supplierPartNumber || "";
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
  }, []);

  const processPartMetadataResponse = useCallback((data, localPart) => {
    // cancelled or auth required
    if (!data) return;

    if (data.errors && data.errors.length > 0) {
      setPartMetadataErrors(data.errors);
    }

    let metadataParts = [];
    const infoResponse = mergeInfoResponse(data.response, localPart.storedFiles);
    if (infoResponse && infoResponse.parts && infoResponse.parts.length > 0) {
      metadataParts = infoResponse.parts;

      const suggestedPart = infoResponse.parts[0];
      // populate the form with data from the part metadata
      if (!pageHasParameters) setPartFromMetadata(metadataParts, { ...suggestedPart, quantity: -1 });
    } else {
      // no part metadata available
      setPartMetadataIsSubscribed(true);
    }

    // set the first datasheet meta display, because the carousel component doesnt fire the first event
    if (infoResponse && infoResponse.datasheets && infoResponse.datasheets.length > 0) setDatasheetMeta(infoResponse.datasheets[0]);

    setInfoResponse(infoResponse);
    setMetadataParts(metadataParts);
    setLoadingPartMetadata(false);
  }, [pageHasParameters, setPartFromMetadata]);

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
      const response = await fetchApi(`api/part/info?partNumber=${encodeURIComponent(partNumber.trim())}&supplierPartNumbers=digikey:${part.digiKeyPartNumber || ""},mouser:${part.mouserPartNumber || ""},arrow:${part.arrowPartNumber},tme:${part.tmePartNumber}`, {
        signal: Inventory.infoAbortController.signal
      });

      const data = response.data;
      if (data.requiresAuthentication) {
        // notify and redirect for authentication
        if (data.redirectUrl && data.redirectUrl.length > 0) {
          setViewPreference('digikey', {
            partNumber: partNumber,
          });

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
      console.error("Exception", ex);
      if (ex.name === "AbortError") {
        // Continuation logic has already been skipped, so return normally
        return { data: null, existsInInventory: false };
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
    if (partNumber.length < MinSearchKeywordLength)
      return { exists: false, data: null, error: `Ignoring search as keywords are less than the minimum required (${MinSearchKeywordLength}).` };
    const existsResponse = await fetchApi(`api/part/search?keywords=${encodeURIComponent(partNumber.trim())}&exactMatch=true`, {
      signal: Inventory.infoAbortController.signal,
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
    const response = await fetchApi(`api/part/barcode/info?barcode=${encodeURIComponent(scannedPart.barcode.trim())}`, {
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
      // fetch metadata on the barcode, don't await, do a background update
      const scannedPart = {
        partNumber: cleanPartNumber,
        barcode: input.correctedValue
      };
      setInputPartNumber(cleanPartNumber);
      doBarcodeLookup(scannedPart, (partInfo) => {
        // barcode found
        if (cleanPartNumber) {
          setPartMetadataIsSubscribed(false);
          setPartMetadataErrors([]);
          if (!isEditing) setPartFromMetadata(metadataParts, { ...partInfo, quantity: partInfo.quantityAvailable });
          if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: partInfo.quantityAvailable });

          // also run a search to get datasheets/images
          fetchPartMetadataAndInventory(cleanPartNumber, part);
          setIsDirty(true);
        }
      }, (scannedPart) => {
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
          setPart(newPart);
          if (viewPreferences.rememberLast) updateViewPreferences({ lastQuantity: newPart.quantity });
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

  const fetchPart = async (partNumber, partId) => {
    Inventory.partAbortController.abort();
    Inventory.partAbortController = new AbortController();
    setLoadingPart(true);
    try {
      let query = `partNumber=${encodeURIComponent(partNumber.trim())}`;
      const validPartId = typeof partId === "number" ? partId : partId && parseInt(partId.trim());
      if (validPartId > 0)
        query += `&partId=${partId}`;
      const response = await fetchApi(`api/part?${query}`, {
        signal: Inventory.partAbortController.signal
      });
      const { data } = response;
      setPart(data);
      setLoadingPart(false);
      return data;
    } catch (ex) {
      console.error("Exception", ex);
      setLoadingPart(false);
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
    request.partNumber = inputPartNumber.trim();
    request.partTypeId = (parseInt(part.partTypeId) || 0) + "";
    request.mountingTypeId = (parseInt(part.mountingTypeId) || 0) + "";
    request.quantity = parseInt(part.quantity) || 0;
    request.lowStockThreshold = parseInt(part.lowStockThreshold) || 0;
    request.cost = parseFloat(part.cost) || 0.0;
    request.projectId = parseInt(part.projectId) || null;
    request.currency = part.currency || systemSettings.currency || 'USD';

    if (request.partNumber.length === 0) {
      toast.error("Part Number is empty!");
      return;
    }

    removeViewPreference('digikey');
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
      const { data } = response;
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
    removeViewPreference('digikey');
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

    if (props.params.partNumber) {
      navigate("/inventory/add");
      return;
    }

    resetForm("", true);
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
    let searchPartNumber = control.value;

    if (searchPartNumber && searchPartNumber.length >= MinSearchKeywordLength) {
      searchPartNumber = control.value.replace("\t", "");
      await searchDebounced(searchPartNumber, part, partTypes);
    }

    setInputPartNumber(searchPartNumber);
    setIsDirty(true);

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
      await fetchApi(`api/part/print?partNumber=${encodeURIComponent(part.partNumber.trim())}&partId=${part.partId}&generateImageOnly=false`, { method: "POST" });
    } else {
      window.print();
    }

  };

  const handleChooseAlternatePart = (e, part) => {
    setPartFromMetadata(metadataParts, part);
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
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}:${part.partId}`);
    else
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
    await fetchPart(part.partNumber, part.partId);
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
      toast.success(t('message.addXParts', "Added {{count}} new parts!", { count: data.length }));
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

  const confirmDeleteOpen = (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(true);
    setSelectedPart(part);
    setConfirmPartDeleteContent(
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
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
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
                      <Button type="button" title="Delete" onClick={(e) => confirmDeleteOpen(e, part)} style={{ marginLeft: "10px" }}>
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
                      {metadataParts && metadataParts.length > 1 && (
                        <ChooseAlternatePartModal
                          trigger={
                            <Popup
                              hideOnScroll
                              disabled={viewPreferences.helpDisabled}
                              onOpen={disableHelp}
                              content={t('page.inventory.popup.alternateParts', "Choose a different part to extract metadata information from. By default, Binner will give you the most relevant part and with the highest quantity available.")}
                              trigger={
                                <Button type="button" secondary style={{ fontSize: '0.9em', width: '265px', padding: '0.68em 1.5em', position: 'absolute', right: '-10px', top: '-10px' }}>
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
                </Segment>}

                {/* Part Preferences */}
                {!disableRendering.current && <Segment loading={loadingPartMetadata} color="green">
                  <Header dividing as="h3">
                    {t('page.inventory.privatePartInfo', "Private Part Information")}
                  </Header>
                  <p>{t('page.inventory.privatePartInfoMessage', "These values can be set manually and will not be synchronized automatically via apis.")}</p>

                  <Form.Field>
                    <label>{t('label.primaryDatasheetUrl', "Primary Datasheet Url")}</label>
                    <ClearableInput type="Input" action className='labeled' placeholder='www.ti.com/lit/ds/symlink/lm2904-n.pdf' value={(part.datasheetUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='datasheetUrl'>
                      <Label>https://</Label>
                      <input />
                      <Button onClick={e => handleVisitLink(e, part.datasheetUrl)} disabled={!part.datasheetUrl || part.datasheetUrl.length === 0}>{t('button.view', "View")}</Button>
                    </ClearableInput>
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

                {/* Suppliers */}

                {!disableRendering.current && <PartSuppliersMemoized
                  loadingPartMetadata={loadingPartMetadata}
                  part={part}
                  metadataParts={metadataParts}
                />}

              </Grid.Column>

              <Grid.Column width={4} className="right-column">
                {/** RIGHT COLUMN */}

                {!disableRendering.current && <PartMediaMemoized part={part} infoResponse={infoResponse} datasheet={datasheetMeta} loadingPartMetadata={loadingPartMetadata} />}

                {/* END RIGHT COLUMN */}
              </Grid.Column>
            </Grid.Row>
          </Grid>
        </Dimmer.Dimmable>

      </>);
  }, [inputPartNumber, part, viewPreferences.rememberLast, loadingPart, loadingPartMetadata, partMetadataErrors, isEditing, allPartTypes, isDirty, handlePartTypeChange]);

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
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDeletePart}
        content={confirmPartDeleteContent}
      />
      <Confirm
        className="confirm"
        header={t('page.settings.confirm.mustAuthenticateHeader', "Must Authenticate")}
        open={confirmAuthIsOpen}
        onCancel={() => setConfirmAuthIsOpen(false)}
        onConfirm={handleAuthRedirect}
        content={<p>
          <Trans i18nKey="page.settings.confirm.mustAuthenticate" name={authorizationApiName}>
            External Api (<b>{{ name: authorizationApiName }}</b>) is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.
          </Trans>
        </p>
        }
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
        {!isEditing && <BarcodeScannerInput onReceived={handleBarcodeInput} minInputLength={4} swallowKeyEvent={false} />}
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
        {part.partNumber && <Image src={`api/part/preview?partNumber=${encodeURIComponent(part.partNumber.trim())}&partId=${part.partId}&token=${getImagesToken()}`} id="printarea" width={180} floated="right" style={{ marginTop: "0px" }} />}
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

Inventory.defaultProps = {
  partNumber: ""
};
