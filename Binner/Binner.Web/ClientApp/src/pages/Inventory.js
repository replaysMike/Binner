import React, { useState, useEffect, useMemo } from "react";
import ReactDOM from "react-dom";
import PropTypes from "prop-types";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import _ from "underscore";
import debounce from "lodash.debounce";
import {
  Icon,
  Input,
  Label,
  Button,
  TextArea,
  Image,
  Form,
  Table,
  Segment,
  Popup,
  Modal,
  Dimmer,
  Loader,
  Header,
  Confirm,
  Grid,
  Card,
  Menu,
  Placeholder
} from "semantic-ui-react";
import Carousel from "react-bootstrap/Carousel";
import NumberPicker from "../components/NumberPicker";
import { ChooseAlternatePartModal } from "../components/ChooseAlternatePartModal";
import Dropzone from "../components/Dropzone";
import { ProjectColors } from "../common/Types";
import { fetchApi } from "../common/fetchApi";
import { formatCurrency, formatNumber } from "../common/Utils";
import { toast } from "react-toastify";
import { getPartTypeId } from "../common/partTypes";
import axios from "axios";
import { StoredFileType } from "../common/StoredFileType";
import { GetTypeName, GetTypeValue } from "../common/Types";
import "./Inventory.css";

const ProductImageIntervalMs = 10 * 1000;

export function Inventory(props) {
  const maxRecentAddedParts = 10;
  const [searchParams] = useSearchParams();
  let barcodeBuffer = "";
  const defaultViewPreferences = JSON.parse(localStorage.getItem("viewPreferences")) || {
    helpDisabled: false,
    lastPartTypeId: 14, // IC
    lastMountingTypeId: 1, // Through Hole
    lastQuantity: 1,
    lastProjectId: null,
    lastLocation: "",
    lastBinNumber: "",
    lastBinNumber2: "",
    lowStockThreshold: 10
  };

  const [viewPreferences, setViewPreferences] = useState(defaultViewPreferences);
  const [infoResponse, setInfoResponse] = useState([]);
  const [datasheetTitle, setDatasheetTitle] = useState("");
  const [datasheetPartName, setDatasheetPartName] = useState("");
  const [datasheetDescription, setDatasheetDescription] = useState("");
  const [datasheetManufacturer, setDatasheetManufacturer] = useState("");
  const scannedPartsSerialized = JSON.parse(localStorage.getItem("scannedPartsSerialized")) || [];
  const defaultPart = {
    partId: 0,
    partNumber: props.params.partNumber || "",
    allowPotentialDuplicate: false,
    quantity: viewPreferences.lastQuantity + "",
    lowStockThreshold: viewPreferences.lowStockThreshold + "",
    partTypeId: viewPreferences.lastPartTypeId || 14,
    mountingTypeId: viewPreferences.lastMountingTypeId || 1,
    packageType: "",
    keywords: "",
    description: "",
    datasheetUrl: "",
    digiKeyPartNumber: "",
    mouserPartNumber: "",
    location: viewPreferences.lastLocation || "",
    binNumber: viewPreferences.lastBinNumber || "",
    binNumber2: viewPreferences.lastBinNumber2 || "",
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
      key: 999,
      value: null,
      text: ""
    },
    {
      key: 1,
      value: 1,
      text: "Through Hole"
    },
    {
      key: 2,
      value: 2,
      text: "Surface Mount"
    }
  ];
  const [parts, setParts] = useState([]);
  const [isDirty, setIsDirty] = useState(false);
  const [selectedPart, setSelectedPart] = useState(null);
  const [selectedLocalFile, setSelectedLocalFile] = useState(null);
  const [recentParts, setRecentParts] = useState([]);
  const [metadataParts, setMetadataParts] = useState([]);
  const [duplicateParts, setDuplicateParts] = useState([]);
  const [scannedParts, setScannedParts] = useState(scannedPartsSerialized);
  const [highlightScannedPart, setHighlightScannedPart] = useState(null);
  const [partModalOpen, setPartModalOpen] = useState(false);
  const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState("Are you sure you want to delete this part?");
  const [confirmLocalFileDeleteContent, setConfirmLocalFileDeleteContent] = useState("Are you sure you want to delete this local file?");
  const [confirmDeleteLocalFileIsOpen, setConfirmDeleteLocalFileIsOpen] = useState(false);
  const [part, setPart] = useState(defaultPart);
  const [partTypes, setPartTypes] = useState([]);
  const [projects, setProjects] = useState([]);
  const [mountingTypes, setMountingTypes] = useState(defaultMountingTypes);
  const [loadingPartMetadata, setLoadingPartMetadata] = useState(true);
  const [loadingPartTypes, setLoadingPartTypes] = useState(true);
  const [loadingProjects, setLoadingProjects] = useState(true);
  const [loadingRecent, setLoadingRecent] = useState(true);
  const [partMetadataIsSubscribed, setPartMetadataIsSubscribed] = useState(false);
  const [saveMessage, setSaveMessage] = useState("");
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [showBarcodeBeingScanned, setShowBarcodeBeingScanned] = useState(false);
  const [bulkScanIsOpen, setBulkScanIsOpen] = useState(false);
  const [error, setError] = useState(null);
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [dragOverClass, setDragOverClass] = useState("");

  useEffect(() => {
    const partNumberStr = props.params.partNumber;
    const fetchData = async () => {
      setPartMetadataIsSubscribed(false);
      await fetchPartTypes();
      await fetchProjects();
      await fetchRecentRows();
      if (partNumberStr) {
        var part = await fetchPart(partNumberStr);
        await fetchPartMetadata(partNumberStr, part);
      } else {
        resetForm();
      }
      addKeyboardHandler();
    };
    fetchData().catch(console.error);
    return () => {
      removeKeyboardHandler();
      scannerDebounced.cancel();
      searchDebounced.cancel();
    };
  }, [props.params.partNumber]); //test

  const fetchPartMetadata = async (input, part) => {
    Inventory.infoAbortController.abort();
    Inventory.infoAbortController = new AbortController();
    setLoadingPartMetadata(true);
    setPartMetadataIsSubscribed(false);
    try {
      const response = await fetchApi(`part/info?partNumber=${input}&partTypeId=${part.partTypeId}&mountingTypeId=${part.mountingTypeId}`, {
        signal: Inventory.infoAbortController.signal
      });
      const data = response.data;
      if (data.requiresAuthentication) {
        // redirect for authentication
        window.open(data.redirectUrl, "_blank");
        return;
      }

      let metadataParts = [];
      const infoResponse = mergeInfoResponse(data.response, part.storedFiles);
      if (infoResponse && infoResponse.parts && infoResponse.parts.length > 0) {
        metadataParts = infoResponse.parts;

        const suggestedPart = infoResponse.parts[0];
        // populate the form with data from the part metadata
        if (!part.partNumber) setPartFromMetadata(metadataParts, suggestedPart, partTypes);
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

  // debounced handler for processing barcode scanner input
  const barcodeInput = (e, value) => {
    barcodeBuffer = "";
    if (value.indexOf(String.fromCharCode(13), value.length - 2) >= 0) {
      const cleanPartNumber = value.replace(String.fromCharCode(13), "").trim();
      // if we have an ok string lets search for the part
      if (cleanPartNumber.length > 2) {
        if (bulkScanIsOpen) {
          // add to bulk scanned parts
          const lastPart = _.last(scannedParts);
          const scannedPart = {
            partNumber: cleanPartNumber,
            quantity: 1,
            location: (lastPart && lastPart.location) || "",
            binNumber: (lastPart && lastPart.binNumber) || "",
            binNumber2: (lastPart && lastPart.binNumber2) || ""
          };
          const existingPartNumber = _.find(scannedParts, { partNumber: cleanPartNumber });
          if (existingPartNumber) {
            existingPartNumber.quantity++;
            localStorage.setItem("scannedPartsSerialized", JSON.stringify(scannedParts));
            setShowBarcodeBeingScanned(false);
            setHighlightScannedPart(existingPartNumber);
            setScannedParts(scannedParts);
          } else {
            const newScannedParts = [...scannedParts, scannedPart];
            localStorage.setItem("scannedPartsSerialized", JSON.stringify(newScannedParts));
            setShowBarcodeBeingScanned(false);
            setHighlightScannedPart(scannedPart);
            setScannedParts(newScannedParts);
          }
        } else {
          // scan single part
          handleChange(e, { name: "partNumber", value: cleanPartNumber });
          setShowBarcodeBeingScanned(false);
        }
      }
    }
  };

  const scannerDebounced = useMemo(() => debounce(barcodeInput, 100), []);
  const searchDebounced = useMemo(() => debounce(fetchPartMetadata, 1000), [partTypes]);

  const onUploadSubmit = async (uploadFiles, type) => {
    setUploading(true);
    if (!part.partId) {
      toast.warn("Files can't be uploaded until the part is saved.");
      return;
    }
    if (uploadFiles && uploadFiles.length > 0) {
      const requestData = new FormData();
      requestData.append("partId", part.partId);
      requestData.append("storedFileType", GetTypeValue(StoredFileType, type));
      for (let i = 0; i < uploadFiles.length; i++) 
        requestData.append("files", uploadFiles[i], uploadFiles[i].name);

      axios
        .request({
          method: "post",
          url: `storedFile`,
          data: requestData
        })
        .then((response) => {
          const { data } = response;
          let errorMessage;
          toast.dismiss();
          if (data.errors && data.errors.length > 0) {
            errorMessage = data.errors.map((err, key) => <li key={key}>{err.message}</li>);
            toast.error(`Failed to upload file!`, { autoClose: 10000 });
            setError(<ul className="errors">{errorMessage}</ul>);
          } else {
            // success uploading
            if (uploadFiles.length === 1)
              toast.success(`File uploaded.`);
            else 
              toast.success(`${uploadFiles.length} files uploaded.`);

            // add it to the local data
            var typeValue = GetTypeValue(StoredFileType, type);
            var i = 0;
            switch(typeValue){
              case StoredFileType.ProductImage:
                const productImages = [...infoResponse.productImages];
                for(i = 0; i < data.length; i++){
                  productImages.unshift({ 
                    name: data[i].originalFileName,
                    value: `/storedFile/preview?fileName=${data[i].fileName}`,
                    id: data[i].storedFileId 
                  });
                }
                setInfoResponse({...infoResponse, productImages });
                break;
              case StoredFileType.Datasheet:
                const datasheets = [...infoResponse.datasheets];
                for(i = 0; i < data.length; i++){
                  const datasheet = { 
                    name: data[i].originalFileName,
                    value: {
                      datasheetUrl: `/storedFile/local?fileName=${data[i].fileName}`,
                      description: data[i].originalFileName,
                      imageUrl: `/storedFile/preview?fileName=${data[i].fileName}`,
                      manufacturer: '',
                      title: data[i].originalFileName,
                    },
                    id: data[i].storedFileId 
                  };
                  datasheets.unshift(datasheet);
                  setDatasheetMeta(datasheet);
                }
                setInfoResponse({...infoResponse, datasheets });
                break;
              case StoredFileType.Pinout:
                const pinoutImages = [...infoResponse.pinoutImages];
                for(i = 0; i < data.length; i++){
                  pinoutImages.unshift({ 
                    name: data[i].originalFileName,
                    value: `/storedFile/preview?fileName=${data[i].fileName}`,
                    id: data[i].storedFileId 
                  });
                }
                setInfoResponse({...infoResponse, pinoutImages });
                break;
              case StoredFileType.ReferenceDesign:
                const circuitImages = [...infoResponse.circuitImages ];
                for(i = 0; i < data.length; i++){
                  circuitImages.unshift({ 
                    name: data[i].originalFileName,
                    value: `/storedFile/preview?fileName=${data[i].fileName}`,
                    id: data[i].storedFileId 
                  });
                }
                setInfoResponse({...infoResponse, circuitImages });
                break;
              default:
            }

            setError(null);
            setIsDirty(false);
          }
          setUploading(false);
          setFiles([]);
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          if (error.code === "ERR_NETWORK") {
            const msg = "Unable to upload. Check that the file is not locked or deleted.";
						toast.error(msg, { autoClose: 10000 });
						setError(msg);
          } else {
            toast.error(`Import upload failed!`);
            setError(error.message);
          }
          setIsDirty(false);
          setUploading(false);
          setFiles([]);
        });
    } else {
      toast.error("No files selected for upload!");
    }
  };

  const onUploadError = (errors) => {
    for(let i = 0; i < errors.length; i++)
      toast.error(errors[i], { autoClose: 10000 });
  };

  const addKeyboardHandler = () => {
    if (document) document.addEventListener("keydown", onKeydown);
  };

  const removeKeyboardHandler = () => {
    if (document) document.removeEventListener("keydown");
  };

  const enableKeyboardListening = () => {
    setIsKeyboardListening(true);
  };

  const disableKeyboardListening = () => {
    setIsKeyboardListening(false);
  };

  /*UNSAFE_componentWillReceiveProps(nextProps) {
    // if the path changes do some necessary housekeeping
    if (nextProps.location !== props.location) {
      // reset the form if the URL has changed
      resetForm();
    }
  }*/

  // listens for document keydown events, used for barcode scanner input
  const onKeydown = (e) => {
    if (isKeyboardListening) {
      let char = String.fromCharCode(96 <= e.keyCode && e.keyCode <= 105 ? e.keyCode - 48 : e.keyCode);
      // map proper value when shift is used
      if (e.shiftKey) char = e.key;
      // map numlock extra keys
      if ((e.keyCode >= 186 && e.keyCode <= 192) || (e.keyCode >= 219 && e.keyCode <= 222)) char = e.key;
      if (
        e.keyCode === 13 ||
        e.keyCode === 32 ||
        e.keyCode === 9 ||
        (e.keyCode >= 48 && e.keyCode <= 90) ||
        (e.keyCode >= 107 && e.keyCode <= 111) ||
        (e.keyCode >= 186 && e.keyCode <= 222)
      ) {
        barcodeBuffer += char;
        scannerDebounced(e, barcodeBuffer);
      }
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
    enableKeyboardListening(e);
    setPart(part);
  };

  const fetchPart = async (partNumber) => {
    Inventory.partAbortController.abort();
    Inventory.partAbortController = new AbortController();
    setLoadingPartMetadata(true);
    try {
      const response = await fetchApi(`part?partNumber=${partNumber}`, {
        signal: Inventory.partAbortController.signal
      });
      const { data } = response;
      setPart(data);
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
    const response = await fetchApi(`part/list?orderBy=DateCreatedUtc&direction=Descending&results=${maxRecentAddedParts}`);
    const { data } = response;
    setRecentParts(data.items);
    setLoadingRecent(false);
  };

  const fetchPartTypes = async () => {
    setLoadingPartTypes(true);
    const response = await fetchApi("partType/list");
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
    setLoadingPartTypes(false);
  };

  const fetchProjects = async () => {
    setLoadingProjects(true);
    const response = await fetchApi("project/list?orderBy=DateCreatedUtc&direction=Descending&results=99");
    const { data } = response;
    const projects = _.sortBy(
      data.map((item) => {
        return {
          key: item.projectId,
          value: item.projectId,
          text: item.name,
          label: {
            ...(_.find(ProjectColors, (c) => c.value === item.color).name !== "" && { color: _.find(ProjectColors, (c) => c.value === item.color).name }),
            circular: true,
            content: item.parts,
            size: "tiny"
          }
        };
      }),
      "text"
    );
    // ensure that the current part's projectId can't be set to an invalid project
    if (!_.find(projects, (p) => p.value === viewPreferences.lastProjectId)) {
      setPart({ ...part, projectId: "" });
    }
    setLoadingProjects(false);
    setProjects(projects);
  };

  const getMountingTypeById = (mountingTypeId) => {
    switch (mountingTypeId) {
      default:
      case 1:
        return "through hole";
      case 2:
        return "surface mount";
    }
  };

  const mergeInfoResponse = (infoResponse, storedFiles) => {
    var storedProductImages = _.filter(storedFiles, x => x.storedFileType === StoredFileType.ProductImage);
    var storedDatasheets = _.filter(storedFiles, x => x.storedFileType === StoredFileType.Datasheet);
    var storedPinouts = _.filter(storedFiles, x => x.storedFileType === StoredFileType.Pinout);
    var storedReferenceDesigns = _.filter(storedFiles, x => x.storedFileType === StoredFileType.ReferenceDesign);
    if (storedProductImages && storedProductImages.length > 0)
      infoResponse.productImages.unshift(...storedProductImages.map(pi => ({
        name: pi.originalFileName,
        value: `/storedFile/preview?fileName=${pi.fileName}`,
        id: pi.storedFileId
      })));
    if (storedDatasheets && storedDatasheets.length > 0)
      infoResponse.datasheets.unshift(...storedDatasheets.map(pi => ({
        name: pi.originalFileName,
        value: {
          datasheetUrl: `/storedFile/local?fileName=${pi.fileName}`,
          description: pi.originalFileName,
          imageUrl: `/storedFile/preview?fileName=${pi.fileName}`,
          manufacturer: '',
          title: pi.originalFileName,
        },
        id: pi.storedFileId
      })));
    if (storedPinouts && storedPinouts.length > 0)
      infoResponse.pinoutImages.unshift(...storedPinouts.map(pi => ({
        name: pi.originalFileName,
        value: `/storedFile/preview?fileName=${pi.fileName}`,
        id: pi.storedFileId
      })));
    if (storedReferenceDesigns && storedReferenceDesigns.length > 0)
      infoResponse.circuitImages.unshift(...storedReferenceDesigns.map(pi => ({
        name: pi.originalFileName,
        value: `/storedFile/preview?fileName=${pi.fileName}`,
        id: pi.storedFileId
      })));
    return infoResponse;
  };

  /**
   * Force a save of a possible duplicate part
   * @param {any} e
   */
  const handleForceSubmit = (e) => {
    setDuplicatePartModalOpen(false);
    setPart({ ...part, allowPotentialDuplicate: true });
    onSubmit(e);
  };

  /**
   * Save the part
   *
   * @param {any} e
   */
  const onSubmit = async (e) => {
    const isExisting = part.partId > 0;

    const request = { ...part };
    request.partTypeId = part.partTypeId.toString();
    request.mountingTypeId = part.mountingTypeId.toString();
    request.quantity = Number.parseInt(part.quantity) || 0;
    request.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    request.cost = Number.parseFloat(part.cost) || 0.0;
    request.projectId = Number.parseInt(part.projectId) || null;

    const saveMethod = isExisting ? "PUT" : "POST";
    const response = await fetch("part", {
      method: saveMethod,
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });

    let saveMessage = "";
    if (response.status === 409) {
      // possible duplicate
      const data = await response.json();
      setDuplicateParts(data.parts);
      setDuplicatePartModalOpen(true);
    } else if (response.status === 200) {
      // reset form if it was a new part
      if (isExisting) {
        saveMessage = `Saved part ${request.partNumber}!`;
        setSaveMessage(saveMessage);
        toast.info(saveMessage);
      } else {
        saveMessage = `Added part ${request.partNumber}!`;
        resetForm(saveMessage);
        toast.success(saveMessage);
      }
      // refresh recent parts list
      await fetchRecentRows();
    } else if (response.status === 400) {
      // other error (invalid part type, mounting type, etc.)
      saveMessage = `Failed to update, check Part Type and Mounting Type`;
      setSaveMessage(saveMessage);
      toast.error(saveMessage);
    }
  };

  const resetForm = (saveMessage = "", clearAll = false) => {
    setIsDirty(false);
    setSaveMessage(saveMessage);
    setMetadataParts([]);
    setDuplicateParts([]);
    setPart({
      partId: 0,
      partNumber: "",
      allowPotentialDuplicate: false,
      quantity: clearAll ? "1" : viewPreferences.lastQuantity + "",
      lowStockThreshold: clearAll ? "10" : viewPreferences.lowStockThreshold + "",
      partTypeId: clearAll ? 14 : viewPreferences.lastPartTypeId,
      mountingTypeId: clearAll ? 1 : viewPreferences.lastMountingTypeId,
      packageType: "",
      keywords: "",
      description: "",
      datasheetUrl: "",
      digiKeyPartNumber: "",
      mouserPartNumber: "",
      location: viewPreferences.lastLocation || "",
      binNumber: viewPreferences.lastBinNumber || "",
      binNumber2: viewPreferences.lastBinNumber2 || "",
      cost: "",
      lowestCostSupplier: "",
      lowestCostSupplierUrl: "",
      productUrl: "",
      manufacturer: "",
      manufacturerPartNumber: "",
      imageUrl: "",
      projectId: "",
      supplier: "",
      supplierPartNumber: ""
    });
    setLoadingPartMetadata(false);
    setLoadingPartTypes(false);
    setLoadingProjects(false);
  };

  const clearForm = (e) => {
    // e could be null as this special method can be called outside of the component without a synthetic event
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    resetForm("", true);
  };

  const updateNumberPicker = (e) => {
    localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastQuantity: e.value }));
    setPart({ ...part, quantity: e.value.toString() });
    setIsDirty(true);
  };

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setPartMetadataIsSubscribed(false);
    const updatedPart = { ...part };
    updatedPart[control.name] = control.value;
    switch (control.name) {
      case "partNumber":
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber);
        break;
      case "partTypeId":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastPartTypeId: control.value }));
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber);
        break;
      case "mountingTypeId":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastMountingTypeId: control.value }));
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0) searchDebounced(updatedPart.partNumber);
        break;
      case "lowStockThreshold":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lowStockThreshold: control.value }));
        break;
      case "projectId":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastProjectId: control.value }));
        break;
      case "location":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastLocation: control.value }));
        break;
      case "binNumber":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastBinNumber: control.value }));
        break;
      case "binNumber2":
        localStorage.setItem("viewPreferences", JSON.stringify({ ...viewPreferences, lastBinNumber2: control.value }));
        break;
      default:
        break;
    }
    setPart({ ...updatedPart });
    setIsDirty(true);
  };

  const printLabel = async (e) => {
    e.preventDefault();
    await fetchApi(`part/print?partNumber=${part.partNumber}&generateImageOnly=false`, { method: "POST" });
  };

  const setPartFromMetadata = (metadataParts, suggestedPart, partTypes) => {
    const entity = { ...part };
    const mappedPart = {
      partNumber: suggestedPart.basePartNumber,
      partTypeId: getPartTypeId(suggestedPart.partType, partTypes),
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
      status: suggestedPart.status
    };
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
      const searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "Mouser" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.mouserPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || "";
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }
    if (mappedPart.supplier === "Mouser") {
      entity.mouserPartNumber = mappedPart.supplierPartNumber || "";
      const searchResult = _.find(metadataParts, (e) => {
        return e !== undefined && e.supplier === "DigiKey" && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber;
      });
      if (searchResult) {
        entity.digiKeyPartNumber = searchResult.supplierPartNumber;
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

    entity.lowestCostSupplier = lowestCostPart.supplier;
    entity.lowestCostSupplierUrl = lowestCostPart.productUrl;
    setPart(entity);
  };

  const handleChooseAlternatePart = (e, part, partTypes) => {
    setPartFromMetadata(metadataParts, part, partTypes);
    setPartModalOpen(false);
  };

  const handleOpenModal = (e) => {
    e.preventDefault();
    setPartModalOpen(true);
  };

  const handleBulkBarcodeScan = (e) => {
    e.preventDefault();
    setBulkScanIsOpen(true);
  };

  const handleBulkScanClose = () => {
    setBulkScanIsOpen(false);
  };

  const renderAllMatchingParts = (part, metadataParts) => {
    return (
      <Table compact celled selectable size="small" className="partstable">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>Part</Table.HeaderCell>
            <Table.HeaderCell>Manufacturer</Table.HeaderCell>
            <Table.HeaderCell>Part Type</Table.HeaderCell>
            <Table.HeaderCell>Supplier</Table.HeaderCell>
            <Table.HeaderCell>Supplier Part</Table.HeaderCell>
            <Table.HeaderCell>Package Type</Table.HeaderCell>
            <Table.HeaderCell>Mounting Type</Table.HeaderCell>
            <Table.HeaderCell>Cost</Table.HeaderCell>
            <Table.HeaderCell>Image</Table.HeaderCell>
            <Table.HeaderCell>Datasheet</Table.HeaderCell>
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
                            View Datasheet
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
            <Table.HeaderCell>Part Number</Table.HeaderCell>
            <Table.HeaderCell>Manufacturer Part</Table.HeaderCell>
            <Table.HeaderCell>Manufacturer</Table.HeaderCell>
            <Table.HeaderCell>Description</Table.HeaderCell>
            <Table.HeaderCell>Part Type</Table.HeaderCell>
            <Table.HeaderCell>Location</Table.HeaderCell>
            <Table.HeaderCell>Bin Number</Table.HeaderCell>
            <Table.HeaderCell>Bin Number 2</Table.HeaderCell>
            <Table.HeaderCell>Mounting Type</Table.HeaderCell>
            <Table.HeaderCell>Image</Table.HeaderCell>
            <Table.HeaderCell>Datasheet</Table.HeaderCell>
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
                <Button onClick={(e) => handleHighlightAndVisit(e, p.datasheetUrl)}>View Datasheet</Button>
              </Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
    );
  };

  const handlePartModalClose = () => {
    setPartModalOpen(false);
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

  const onSubmitScannedParts = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    const request = {
      parts: scannedParts
    };
    await fetchApi("part/bulk", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    localStorage.setItem("scannedPartsSerialized", JSON.stringify([]));
    setBulkScanIsOpen(false);
    setScannedParts([]);
  };

  const handleScannedPartChange = (e, control, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    scannedPart[control.name] = control.value;
    setScannedParts(scannedParts);
    setIsDirty(true);
  };

  const deleteScannedPart = (e, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    const scannedPartsDeleted = _.without(scannedParts, _.findWhere(scannedParts, { partNumber: scannedPart.partNumber }));
    localStorage.setItem("scannedPartsSerialized", JSON.stringify(scannedPartsDeleted));
    setScannedParts(scannedPartsDeleted);
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`part`, {
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
    await fetchApi(`storedfile`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ storedFileId: selectedLocalFile.localFile.id })
    });
    var itemsExceptDeleted;
    switch(selectedLocalFile.type){
      case 'productImages':
        itemsExceptDeleted = _.without(infoResponse.productImages, _.findWhere(infoResponse.productImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({...infoResponse, productImages: itemsExceptDeleted});
        break;
      case 'datasheets':
        itemsExceptDeleted = _.without(infoResponse.datasheets, _.findWhere(infoResponse.datasheets, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({...infoResponse, datasheets: itemsExceptDeleted});
        if (itemsExceptDeleted.length > 0)
          setDatasheetMeta(itemsExceptDeleted[0]);
        break;
      case 'pinoutImages':
        itemsExceptDeleted = _.without(infoResponse.pinoutImages, _.findWhere(infoResponse.pinoutImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({...infoResponse, pinoutImages: itemsExceptDeleted});
          break;
      case 'circuitImages':
        itemsExceptDeleted = _.without(infoResponse.circuitImages, _.findWhere(infoResponse.circuitImages, { id: selectedLocalFile.localFile.id }));
        setInfoResponse({...infoResponse, circuitImages: itemsExceptDeleted});
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
    setConfirmPartDeleteContent(<p>Are you sure you want to delete part <b>{part.partNumber}</b>?<br/><br/>This action is <i>permanent and cannot be recovered</i>.</p>);
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
  };

  const confirmDeleteLocalFileOpen = (e, localFile, type) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteLocalFileIsOpen(true);
    setSelectedLocalFile({ localFile, type });
    setConfirmLocalFileDeleteContent(<p>Are you sure you want to delete this local file named <b>{localFile.name}</b>?<br/><br/>This action is <i>permanent and cannot be recovered</i>.</p>);
  };

  const confirmDeleteLocalFileClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteLocalFileIsOpen(false);
    setSelectedLocalFile(null);
  };

  const onScannedInputKeyDown = (e, scannedPart) => {
    if (e.keyCode === 13) {
      // copy downward
      let beginCopy = false;
      scannedParts.forEach((part) => {
        if (part.partName === scannedPart.partName) beginCopy = true;
        if (beginCopy && part[e.target.name] === "") {
          part[e.target.name] = scannedPart[e.target.name];
        }
      });
      setScannedParts(scannedParts);
    }
  };

  const visitAnchor = (e, anchor) => {
    e.preventDefault();
    var redirectToURL = document.URL.replace(/#.*$/, "");

    redirectToURL = redirectToURL + anchor;
    window.location.href = redirectToURL;
  };

  const setDatasheetMeta = (datasheet) => {
    const partName = datasheet.name;
    const title = datasheet.value.title;
    const description = datasheet.value.description;
    const manufacturer = datasheet.value.manufacturer;
    setDatasheetTitle(title);
    setDatasheetPartName(partName);
    setDatasheetManufacturer(manufacturer);
    setDatasheetDescription(description);
  };

  const onCurrentDatasheetChanged = (activeIndex, control) => {
    setDatasheetMeta(infoResponse.datasheets[activeIndex]);
  };

  const renderScannedParts = (scannedParts, highlightScannedPart) => {
    if (highlightScannedPart) {
      // reset the css highlight animation
      setTimeout(() => {
        const elements = document.getElementsByClassName("scannedPartAnimation");
        for (let i = 0; i < elements.length; i++) {
          elements[i].classList.add("lastScannedPart");
          if (elements[i].classList.contains("scannedPartAnimation")) elements[i].classList.remove("scannedPartAnimation");
        }
      }, 750);
    }
    return (
      <Form className="notdroptarget">
        <Table compact celled striped size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>Part</Table.HeaderCell>
              <Table.HeaderCell width={2}>Quantity</Table.HeaderCell>
              <Table.HeaderCell>Location</Table.HeaderCell>
              <Table.HeaderCell>Bin Number</Table.HeaderCell>
              <Table.HeaderCell>Bin Number 2</Table.HeaderCell>
              <Table.HeaderCell width={1}></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {scannedParts.map((p, index) => (
              <Table.Row
                key={index}
                className={highlightScannedPart && p.partNumber === highlightScannedPart.partNumber ? `scannedPartAnimation ${Math.random()}` : ""}
              >
                <Table.Cell collapsing>
                  <Label>{p.partNumber}</Label>
                </Table.Cell>
                <Table.Cell collapsing>
                  <Form.Input
                    width={10}
                    value={p.quantity}
                    onChange={(e, c) => handleScannedPartChange(e, c, p)}
                    name="quantity"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <Form.Input
                    width={16}
                    placeholder="Home lab"
                    value={p.location}
                    onChange={(e, c) => handleScannedPartChange(e, c, p)}
                    name="location"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                    onKeyDown={(e) => onScannedInputKeyDown(e, p)}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <Form.Input
                    width={14}
                    placeholder=""
                    value={p.binNumber}
                    onChange={(e, c) => handleScannedPartChange(e, c, p)}
                    name="binNumber"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                    onKeyDown={(e) => onScannedInputKeyDown(e, p)}
                  />
                </Table.Cell>
                <Table.Cell collapsing>
                  <Form.Input
                    width={14}
                    placeholder=""
                    value={p.binNumber2}
                    onChange={(e, c) => handleScannedPartChange(e, c, p)}
                    name="binNumber2"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                    onKeyDown={(e) => onScannedInputKeyDown(e, p)}
                  />
                </Table.Cell>
                <Table.Cell collapsing textAlign="center" verticalAlign="middle">
                  <Button type="button" circular size="mini" icon="delete" title="Delete" onClick={(e) => deleteScannedPart(e, p)} />
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table>
      </Form>
    );
  };

  const renderRecentParts = (recentParts) => {
    return (
      <Table compact celled selectable striped>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>Part</Table.HeaderCell>
            <Table.HeaderCell>Quantity</Table.HeaderCell>
            <Table.HeaderCell>Part Type</Table.HeaderCell>
            <Table.HeaderCell>Manufacturer Part</Table.HeaderCell>
            <Table.HeaderCell>Location</Table.HeaderCell>
            <Table.HeaderCell>Bin Number</Table.HeaderCell>
            <Table.HeaderCell>Bin Number 2</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {recentParts.map((p, index) => (
            <Table.Row key={index} onClick={(e) => handleRecentPartClick(e, p)}>
              <Table.Cell>{index === 0 ? <Label ribbon>{p.partNumber}</Label> : p.partNumber}</Table.Cell>
              <Table.Cell>{p.quantity}</Table.Cell>
              <Table.Cell>{p.partType}</Table.Cell>
              <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
              <Table.Cell>{p.location}</Table.Cell>
              <Table.Cell>{p.binNumber}</Table.Cell>
              <Table.Cell>{p.binNumber2}</Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
    );
  };

  const matchingPartsList = renderAllMatchingParts(part, metadataParts);
  const title = part.partId > 0 || props.params.partNumber ? "Edit Inventory" : "Add Inventory";

  /* RENDER */

  return (
    <div>
      <Modal centered open={duplicatePartModalOpen} onClose={handleDuplicatePartModalClose}>
        <Modal.Header>Duplicate Part</Modal.Header>
        <Modal.Content scrolling>
          <Modal.Description>
            <h3>There is a possible duplicate part already in your inventory.</h3>
            {renderDuplicateParts()}
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleDuplicatePartModalClose}>Cancel</Button>
          <Button primary onClick={handleForceSubmit}>
            Save Anyways
          </Button>
        </Modal.Actions>
      </Modal>
      <Confirm className="confirm" header='Delete Part' open={confirmDeleteIsOpen} onCancel={confirmDeleteClose} onConfirm={handleDeletePart} content={confirmPartDeleteContent} />
      <Confirm className="confirm" header='Delete File' open={confirmDeleteLocalFileIsOpen} onCancel={confirmDeleteLocalFileClose} onConfirm={handleDeleteLocalFile} content={confirmLocalFileDeleteContent} />

      {/* FORM START */}

      <Form onSubmit={onSubmit}>
        {part && part.partId > 0 && (
          <Button animated="vertical" circular floated="right" size="mini" onClick={printLabel} style={{ marginTop: "5px" }}>
            <Button.Content visible>
              <Icon name="print" />
            </Button.Content>
            <Button.Content hidden>Print</Button.Content>
          </Button>
        )}
        {part.partNumber && <Image src={"/part/preview?partNumber=" + part.partNumber} width={180} floated="right" style={{ marginTop: "0px" }} />}
        <h1 style={{ display: "inline-block", marginRight: "30px" }}>{title}</h1>
        <div title="Bulk Barcode Scan" style={{ width: "132px", height: "30px", display: "inline-block", cursor: "pointer" }} onClick={handleBulkBarcodeScan}>
          <div className="anim-box">
            <div className="scanner" />
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
          </div>
        </div>

        {partMetadataIsSubscribed && (
          <div className="page-notice" onClick={() => setPartMetadataIsSubscribed(false)}>
            <Icon name="close" /> No part information is available for '{part.partNumber}'. You are subscribed to updates and will be automatically updated when
            the part is indexed.
          </div>
        )}

        <Grid celled className="inventory-container">
          <Grid.Row>
            <Grid.Column width={12} className="left-column">
              {/** LEFT COLUMN */}

              <Form.Group>
                <Form.Input
                  label="Part"
                  required
                  placeholder="LM358"
                  icon="search"
                  focus
                  value={part.partNumber || ""}
                  onChange={handleChange}
                  name="partNumber"
                  onFocus={disableKeyboardListening}
                  onBlur={enableKeyboardListening}
                />
                <Form.Dropdown
                  label="Part Type"
                  placeholder="Part Type"
                  loading={loadingPartTypes}
                  search
                  selection
                  value={part.partTypeId || ""}
                  options={partTypes}
                  onChange={handleChange}
                  name="partTypeId"
                  onFocus={disableKeyboardListening}
                  onBlur={enableKeyboardListening}
                />
                <Form.Dropdown
                  label="Mounting Type"
                  placeholder="Mounting Type"
                  search
                  selection
                  value={part.mountingTypeId || ""}
                  options={mountingTypes}
                  onChange={handleChange}
                  name="mountingTypeId"
                  onFocus={disableKeyboardListening}
                  onBlur={enableKeyboardListening}
                />
              </Form.Group>
              <Form.Group>
                <Popup
                  hideOnScroll
                  disabled={viewPreferences.helpDisabled}
                  onOpen={disableHelp}
                  content="Use the mousewheel and CTRL/ALT to change step size"
                  trigger={
                    <Form.Field
                      control={NumberPicker}
                      label="Quantity"
                      placeholder="10"
                      min={0}
                      value={part.quantity || ""}
                      onChange={updateNumberPicker}
                      name="quantity"
                      autoComplete="off"
                      onFocus={disableKeyboardListening}
                      onBlur={enableKeyboardListening}
                    />
                  }
                />
                <Popup
                  hideOnScroll
                  disabled={viewPreferences.helpDisabled}
                  onOpen={disableHelp}
                  content="Alert when the quantity gets below this value"
                  trigger={
                    <Form.Input
                      label="Low Stock"
                      placeholder="10"
                      value={part.lowStockThreshold || ""}
                      onChange={handleChange}
                      name="lowStockThreshold"
                      width={3}
                      onFocus={disableKeyboardListening}
                      onBlur={enableKeyboardListening}
                    />
                  }
                />
                <Form.Dropdown
                  label="Project"
                  placeholder="My Project"
                  loading={loadingProjects}
                  search
                  selection
                  value={part.projectId || ""}
                  options={projects}
                  onChange={handleChange}
                  name="projectId"
                  onFocus={disableKeyboardListening}
                  onBlur={enableKeyboardListening}
                />
              </Form.Group>

              <Segment secondary>
                <Form.Group>
                  <Popup
                    hideOnScroll
                    disabled={viewPreferences.helpDisabled}
                    onOpen={disableHelp}
                    content="A custom value for identifying the parts location"
                    trigger={
                      <Form.Input
                        label="Location"
                        placeholder="Home lab"
                        value={part.location || ""}
                        onChange={handleChange}
                        name="location"
                        onFocus={disableKeyboardListening}
                        onBlur={enableKeyboardListening}
                        width={5}
                      />
                    }
                  />
                  <Popup
                    hideOnScroll
                    disabled={viewPreferences.helpDisabled}
                    onOpen={disableHelp}
                    content="A custom value for identifying the parts location"
                    trigger={
                      <Form.Input
                        label="Bin Number"
                        placeholder="IC Components 2"
                        value={part.binNumber || ""}
                        onChange={handleChange}
                        name="binNumber"
                        onFocus={disableKeyboardListening}
                        onBlur={enableKeyboardListening}
                        width={4}
                      />
                    }
                  />
                  <Popup
                    hideOnScroll
                    disabled={viewPreferences.helpDisabled}
                    onOpen={disableHelp}
                    content="A custom value for identifying the parts location"
                    trigger={
                      <Form.Input
                        label="Bin Number 2"
                        placeholder="14"
                        value={part.binNumber2 || ""}
                        onChange={handleChange}
                        name="binNumber2"
                        onFocus={disableKeyboardListening}
                        onBlur={enableKeyboardListening}
                        width={4}
                      />
                    }
                  />
                </Form.Group>
              </Segment>

              <Form.Field inline>
                <Button.Group>
                  <Button type="submit" primary style={{ width: "200px" }} disabled={!isDirty}>
                    <Icon name="save" />
                    Save
                  </Button>
                  <Button.Or />
                  <Button type="button" style={{ width: "100px" }} title="Clear form" disabled={!isDirty} onClick={clearForm}>
                    Clear
                  </Button>
                </Button.Group>
                {part && part.partId > 0 && (
                  <Button type="button" title="Delete" onClick={(e) => confirmDeleteOpen(e, part)} style={{ marginLeft: "10px" }}>
                    <Icon name="delete" />
                    Delete
                  </Button>
                )}
                {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
              </Form.Field>

              {/* PART METADATA */}

              <Segment loading={loadingPartMetadata} color="blue">
                <Header dividing as="h3">
                  Part Metadata
                </Header>

                {metadataParts && metadataParts.length > 0 && (
                  <ChooseAlternatePartModal
                    trigger={
                      <Popup
                        hideOnScroll
                        disabled={viewPreferences.helpDisabled}
                        onOpen={disableHelp}
                        content="Choose a different part to extract metadata information from. By default, Binner will give you the most relevant part and with the highest quantity available."
                        trigger={
                          <Button secondary>
                            <Icon name="external alternate" color="blue" />
                            Choose alternate part ({formatNumber(metadataParts.length)})
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
                    <label>Cost</label>
                    <Input
                      label="$"
                      placeholder="0.00"
                      value={part.cost}
                      type="text"
                      onChange={handleChange}
                      name="cost"
                      onFocus={disableKeyboardListening}
                      onBlur={formatField}
                    />
                  </Form.Field>
                  <Form.Input
                    label="Manufacturer"
                    placeholder="Texas Instruments"
                    value={part.manufacturer || ""}
                    onChange={handleChange}
                    name="manufacturer"
                    width={4}
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                  />
                  <Form.Input
                    label="Manufacturer Part"
                    placeholder="LM358"
                    value={part.manufacturerPartNumber || ""}
                    onChange={handleChange}
                    name="manufacturerPartNumber"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                  />
                  <Image src={part.imageUrl} size="tiny" />
                </Form.Group>
                <Form.Field width={10}>
                  <label>Keywords</label>
                  <Input
                    icon="tags"
                    iconPosition="left"
                    label={{ tag: true, content: "Add Keyword" }}
                    labelPosition="right"
                    placeholder="op amp"
                    onChange={handleChange}
                    value={part.keywords || ""}
                    name="keywords"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                  />
                </Form.Field>
                <Form.Field width={4}>
                  <label>Package Type</label>
                  <Input
                    placeholder="DIP8"
                    value={part.packageType || ""}
                    onChange={handleChange}
                    name="packageType"
                    onFocus={disableKeyboardListening}
                    onBlur={enableKeyboardListening}
                  />
                </Form.Field>
                <Form.Field
                  width={10}
                  control={TextArea}
                  label="Description"
                  value={part.description || ""}
                  onChange={handleChange}
                  name="description"
                  onFocus={disableKeyboardListening}
                  onBlur={enableKeyboardListening}
                />
              </Segment>

              {/* Suppliers */}

              <Segment loading={loadingPartMetadata} color="violet">
                <Header dividing as="h3">
                  Suppliers
                </Header>
                <Table compact celled sortable selectable striped unstackable size="small">
                  <Table.Header>
                    <Table.Row>
                      <Table.HeaderCell textAlign="center">Supplier</Table.HeaderCell>
                      <Table.HeaderCell textAlign="center">Supplier Part Number</Table.HeaderCell>
                      <Table.HeaderCell textAlign="center">Cost</Table.HeaderCell>
                      <Table.HeaderCell textAlign="center">Quantity Available</Table.HeaderCell>
                      <Table.HeaderCell textAlign="center">Minimum Order Quantity</Table.HeaderCell>
                      <Table.HeaderCell></Table.HeaderCell>
                      <Table.HeaderCell></Table.HeaderCell>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                    {part &&
                      metadataParts &&
                      _.filter(metadataParts, (p) => p.manufacturerPartNumber === part.manufacturerPartNumber).map((supplier, supplierKey) => (
                        <Table.Row key={supplierKey}>
                          <Table.Cell textAlign="center">{supplier.supplier}</Table.Cell>
                          <Table.Cell textAlign="center">{supplier.supplierPartNumber}</Table.Cell>
                          <Table.Cell textAlign="center">{formatCurrency(supplier.cost)}</Table.Cell>
                          <Table.Cell textAlign="center">{formatNumber(supplier.quantityAvailable)}</Table.Cell>
                          <Table.Cell textAlign="center">{formatNumber(supplier.minimumOrderQuantity)}</Table.Cell>
                          <Table.Cell textAlign="center">
                            {supplier.imageUrl && <img src={supplier.imageUrl} alt={supplier.supplierPartNumber} className="product productshot" />}
                          </Table.Cell>
                          <Table.Cell textAlign="center">
                            <a href={supplier.productUrl} target="_blank" rel="noreferrer">
                              Visit
                            </a>
                          </Table.Cell>
                        </Table.Row>
                      ))}
                  </Table.Body>
                </Table>
              </Segment>
            </Grid.Column>

            <Grid.Column width={4} className="right-column">
              {/** RIGHT COLUMN */}

              <Menu>
                <Menu.Item onClick={(e) => visitAnchor(e, "#datasheets")}>Datasheets</Menu.Item>
                <Menu.Item onClick={(e) => visitAnchor(e, "#pinout")}>Pinout</Menu.Item>
                <Menu.Item onClick={(e) => visitAnchor(e, "#circuits")}>Circuits</Menu.Item>
              </Menu>

              {/* Product Images Carousel */}
              <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ProductImage)}>
                <Card color="blue">
                  {infoResponse && infoResponse.productImages && infoResponse.productImages.length > 0 ? (
                    <Carousel variant="dark" interval={ProductImageIntervalMs} className="centered">
                      {infoResponse.productImages.map((productImage, imageKey) => (
                        <Carousel.Item key={imageKey}>
                          <Image src={productImage.value} size="large" />
                          {productImage.id && 
                            <Popup 
                              position='top left'
                              content="Delete this local file"
                              trigger={<Button onClick={e => confirmDeleteLocalFileOpen(e, productImage, 'productImages')} type="button" size='tiny' style={{position: 'absolute', top: '4px', right: '2px', padding: '2px', zIndex: '9999'}} color='red'><Icon name="delete" style={{margin: 0}} /></Button>}
                            />
                          }
                          <Carousel.Caption>
                            <h5>{productImage.name}</h5>
                          </Carousel.Caption>
                        </Carousel.Item>
                      ))}
                    </Carousel>
                  ) : (
                    <Placeholder>
                      <img src="/image/microchip.png" className="square" alt="" />
                    </Placeholder>
                  )}

                  <Card.Content>
                    <Loader active={loadingPartMetadata} inline size="small" as="i" style={{ float: "right" }} />
                    <Header as="h4">
                      <Icon name="images" />
                      Product Images
                    </Header>
                  </Card.Content>
                </Card>
              </Dropzone>

              {/* DATASHEETS */}
              <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Datasheet)}>
                <Card id="datasheets" color="green">
                  {infoResponse && infoResponse.datasheets && infoResponse.datasheets.length > 0 ? (
                    <div>
                      <Carousel variant="dark" interval={null} style={{ cursor: "pointer" }} onSelect={onCurrentDatasheetChanged}>
                        {infoResponse.datasheets.map((datasheet, datasheetKey) => (
                          <Carousel.Item key={datasheetKey} onClick={(e) => handleVisitLink(e, datasheet.value.datasheetUrl)} data={datasheet}>
                            <Image src={datasheet.value.imageUrl} size="large" />
                            {datasheet.id && 
                              <Popup 
                                position='top left'
                                content="Delete this local file"
                                trigger={<Button onClick={e => confirmDeleteLocalFileOpen(e, datasheet, 'datasheets')} type="button" size='tiny' style={{position: 'absolute', top: '4px', right: '2px', padding: '2px', zIndex: '9999'}} color='red'><Icon name="delete" style={{margin: 0}} /></Button>}
                              />
                            }
                          </Carousel.Item>
                        ))}
                      </Carousel>
                      <Card.Content style={{ textAlign: "left" }}>
                        <Card.Header>{datasheetTitle}</Card.Header>
                        <Card.Meta>
                          {datasheetPartName}, {datasheetManufacturer}
                        </Card.Meta>
                        <Card.Description>{datasheetDescription}</Card.Description>
                      </Card.Content>
                    </div>
                  ) : (
                    <Placeholder>
                      <img src="/image/datasheet.png" className="square" alt="" />
                      <Placeholder.Header>
                        <Placeholder.Line length="very long" />
                        <Placeholder.Line length="medium" />
                        <Placeholder.Line length="short" />
                      </Placeholder.Header>
                    </Placeholder>
                  )}
                  <Card.Content extra>
                      <Header as="h4">
                        <Icon name="file pdf" />
                        Datasheets
                      </Header>
                  </Card.Content>
                </Card>
              </Dropzone>

              {/* PINOUT */}

              <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Pinout)}>
                <Card id="pinout" color="purple">
                  {infoResponse && infoResponse.pinoutImages && infoResponse.pinoutImages.length > 0 ? (
                    <div>
                      <Carousel variant="dark" interval={null} style={{ cursor: "pointer" }}>
                        {infoResponse.pinoutImages.map((pinout, pinoutKey) => (
                          <Carousel.Item key={pinoutKey}>
                          <Image src={pinout.value} size="large" />
                          {pinout.id && 
                            <Popup 
                              position='top left'
                              content="Delete this local file"
                              trigger={<Button onClick={e => confirmDeleteLocalFileOpen(e, pinout, 'pinoutImages')} type="button" size='tiny' style={{position: 'absolute', top: '4px', right: '2px', padding: '2px', zIndex: '9999'}} color='red'><Icon name="delete" style={{margin: 0}} /></Button>}
                            />
                          }
                          <Carousel.Caption>
                            <h5>{pinout.name}</h5>
                          </Carousel.Caption>
                        </Carousel.Item>
                        ))}
                      </Carousel>
                    </div>
                  ) : (
                    <Placeholder>
                      <img src="/image/pinout.png" className="square" alt="" />
                    </Placeholder>
                  )}
                  <Card.Content extra>
                    <Header as="h4">
                      <Icon name="pin" />
                      Pinout
                    </Header>
                  </Card.Content>
                </Card>
              </Dropzone>

              {/* CIRCUITS */}

              <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ReferenceDesign)}>
                <Card id="circuits" color="violet">
                  {infoResponse && infoResponse.circuitImages && infoResponse.circuitImages.length > 0 ? (
                    <div>
                      <Carousel variant="dark" interval={null} style={{ cursor: "pointer" }}>
                        {infoResponse.circuitImages.map((circuit, circuitKey) => (
                          <Carousel.Item key={circuitKey}>
                          <Image src={circuit.value} size="large" />
                          {circuit.id && 
                            <Popup 
                              position='top left'
                              content="Delete this local file"
                              trigger={<Button onClick={e => confirmDeleteLocalFileOpen(e, circuit, 'circuitImages')} type="button" size='tiny' style={{position: 'absolute', top: '4px', right: '2px', padding: '2px', zIndex: '9999'}} color='red'><Icon name="delete" style={{margin: 0}} /></Button>}
                            />
                          }
                          <Carousel.Caption>
                            <h5>{circuit.name}</h5>
                          </Carousel.Caption>
                        </Carousel.Item>
                        ))}
                      </Carousel>
                    </div>
                  ) : (
                    <Placeholder>
                      <img src="/image/referencedesign.png" className="square" alt="" />
                    </Placeholder>
                  )}
                  <Card.Content extra>
                    <Header as="h4">
                      <Icon name="microchip" />
                      Reference Designs
                    </Header>
                  </Card.Content>
                </Card>
              </Dropzone>

              {/* END LEFT COLUMN */}
            </Grid.Column>
          </Grid.Row>
        </Grid>

        <Modal centered open={bulkScanIsOpen} onClose={handleBulkScanClose}>
          <Modal.Header>Bulk Scan</Modal.Header>
          <Modal.Content>
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
              Start scanning parts...
              {renderScannedParts(scannedParts, highlightScannedPart)}
            </div>
          </Modal.Content>
          <Modal.Actions>
            <Button onClick={() => setBulkScanIsOpen(false)}>Cancel</Button>
            <Button primary onClick={onSubmitScannedParts}>
              Save
            </Button>
          </Modal.Actions>
        </Modal>
      </Form>
      <br />
      <div style={{ marginTop: "20px" }}>
        <Segment style={{ minHeight: "50px" }} color="teal">
          <Header dividing as="h3">
            Recently Added
          </Header>
          <Dimmer active={loadingRecent} inverted>
            <Loader inverted />
          </Dimmer>
          {!loadingRecent && recentParts && renderRecentParts(recentParts)}
        </Segment>
      </div>
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
