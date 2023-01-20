import React, { useState, useEffect } from 'react';
import ReactDOM from 'react-dom';
import PropTypes from 'prop-types';
import { useParams, useNavigate  } from "react-router-dom";
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Icon, Input, Label, Button, TextArea, Image, Form, Table, Segment, Popup, Modal, Dimmer, Loader, Header, Confirm } from 'semantic-ui-react';
import NumberPicker from '../components/NumberPicker';
import { ProjectColors } from '../common/Types';
import { fetchApi } from '../common/fetchApi';

export function Inventory(props) {
  const maxRecentAddedParts = 10;
  let barcodeBuffer = '';
  const defaultViewPreferences = JSON.parse(localStorage.getItem('viewPreferences')) || {
    helpDisabled:
      false,
    lastPartTypeId: 14, // IC
    lastMountingTypeId: 1, // Through Hole
    lastQuantity: 1,
    lastProjectId: null,
    lastLocation: '',
    lastBinNumber: '',
    lastBinNumber2: '',
    lowStockThreshold: 10,
  };

  const [partNumber, setPartNumber] = useState(props.params.partNumber);
  const [viewPreferences, setViewPreferences] = useState(defaultViewPreferences);
  const scannedPartsSerialized = JSON.parse(localStorage.getItem('scannedPartsSerialized')) || [];
  const defaultPart = {
    partId: 0,
    partNumber,
    allowPotentialDuplicate: false,
    quantity: viewPreferences.lastQuantity + '',
    lowStockThreshold: viewPreferences.lowStockThreshold + '',
    partTypeId: viewPreferences.lastPartTypeId || 14,
    mountingTypeId: viewPreferences.lastMountingTypeId || 1,
    packageType: '',
    keywords: '',
    description: '',
    datasheetUrl: '',
    digiKeyPartNumber: '',
    mouserPartNumber: '',
    location: viewPreferences.lastLocation || '',
    binNumber: viewPreferences.lastBinNumber || '',
    binNumber2: viewPreferences.lastBinNumber2 || '',
    cost: '',
    lowestCostSupplier: '',
    lowestCostSupplierUrl: '',
    productUrl: '',
    manufacturer: '',
    manufacturerPartNumber: '',
    imageUrl: '',
    projectId: '',
    supplier: '',
    supplierPartNumber: ''
  };
  const defaultMountingTypes = [
    {
      key: 999,
      value: null,
      text: '',
    },
    {
      key: 1,
      value: 1,
      text: 'Through Hole',
    },
    {
      key: 2,
      value: 2,
      text: 'Surface Mount',
    },
  ];
  const [parts, setParts] = useState([]);
  const [selectedPart, setSelectedPart] = useState(null);
  const [recentParts, setRecentParts] = useState([]);
  const [metadataParts, setMetadataParts] = useState([]);
  const [duplicateParts, setDuplicateParts] = useState([]);
  const [scannedParts, setScannedParts] = useState(scannedPartsSerialized);
  const [highlightScannedPart, setHighlightScannedPart] = useState(null);
  const [partModalOpen, setPartModalOpen] = useState(false);
  const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState('Are you sure you want to delete this part?');
  const [part, setPart] = useState(defaultPart);
  const [partTypes, setPartTypes] = useState([]);
  const [projects, setProjects] = useState([]);
  const [mountingTypes, setMountingTypes] = useState(defaultMountingTypes);
  const [loadingPartMetadata, setLoadingPartMetadata] = useState(true);
  const [loadingPartTypes, setLoadingPartTypes] = useState(true);
  const [loadingProjects, setLoadingProjects] = useState(true);
  const [loadingRecent, setLoadingRecent] = useState(true);
  const [saveMessage, setSaveMessage] = useState('');
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [showBarcodeBeingScanned, setShowBarcodeBeingScanned] = useState(false);
  const [bulkScanIsOpen, setBulkScanIsOpen] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      await fetchPartTypes();
      await fetchProjects();
      await fetchRecentRows();
      if (partNumber) {
        await fetchPart(partNumber);
      } else {
        setLoadingPartMetadata(false);
      }
      addKeyboardHandler();
    };
    fetchData().catch(console.error);
    return () => removeKeyboardHandler();
  }, [partNumber]); //test

  const fetchPartMetadata = async (input) => {
    Inventory.abortController.abort(); // Cancel the previous request
    Inventory.abortController = new AbortController();
    setLoadingPartMetadata(true);
    try {
      const response = await fetchApi(`part/info?partNumber=${input}&partTypeId=${part.partTypeId}&mountingTypeId=${part.mountingTypeId}`, {
        signal: Inventory.abortController.signal
      });
      const responseData = response.data;
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }
      const data = responseData.response;
      let metadataParts = [];
      if (data && data.parts && data.parts.length > 0) {
        metadataParts = data.parts;
        const suggestedPart = data.parts[0];      
        setPartFromMetadata(metadataParts, suggestedPart);
      }
      setMetadataParts(metadataParts);
      setLoadingPartMetadata(false);
    } catch (ex) {
      console.error('Exception', ex);
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  // debounced handler for processing barcode scanner input
  const barcodeInput = (e, value) => {
    barcodeBuffer = '';
    if (value.indexOf(String.fromCharCode(13), value.length - 2) >= 0) {
      const cleanPartNumber = value.replace(String.fromCharCode(13), '').trim();
      // if we have an ok string lets search for the part
      if (cleanPartNumber.length > 2) {
        if (bulkScanIsOpen) {
          // add to bulk scanned parts
          const lastPart = _.last(scannedParts);
          const scannedPart = {
            partNumber: cleanPartNumber,
            quantity: 1,
            location: (lastPart && lastPart.location) || '',
            binNumber: (lastPart && lastPart.binNumber) || '',
            binNumber2: (lastPart && lastPart.binNumber2) || '',
          };
          const existingPartNumber = _.find(scannedParts, { partNumber: cleanPartNumber });
          if (existingPartNumber) {
            existingPartNumber.quantity++;
            localStorage.setItem('scannedPartsSerialized', JSON.stringify(scannedParts));
            setShowBarcodeBeingScanned(false);
            setHighlightScannedPart(existingPartNumber);
            setScannedParts(scannedParts);
          }
          else {
            const newScannedParts = [...scannedParts, scannedPart];
            localStorage.setItem('scannedPartsSerialized', JSON.stringify(newScannedParts));
            setShowBarcodeBeingScanned(false);
            setHighlightScannedPart(scannedPart);
            setScannedParts(newScannedParts);
          }
        } else {
          // scan single part
          handleChange(e, { name: 'partNumber', value: cleanPartNumber });
          setShowBarcodeBeingScanned(false);
        }
      }
    }
  };

  const scannerDebounced = AwesomeDebouncePromise(barcodeInput, 100);
  const searchDebounced = AwesomeDebouncePromise(fetchPartMetadata, 500);

  const addKeyboardHandler = () => {
    if (document) document.addEventListener('keydown', onKeydown);
  };

  const removeKeyboardHandler = () => {
    if (document) document.removeEventListener('keydown');
  }

  const enableKeyboardListening = () => {
    setIsKeyboardListening(true);
  };

  const disableKeyboardListening = () => {
    setIsKeyboardListening(false);
  }

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
      let char = String.fromCharCode((96 <= e.keyCode && e.keyCode <= 105) ? e.keyCode - 48 : e.keyCode);
      // map proper value when shift is used
      if (e.shiftKey)
        char = e.key;
      // map numlock extra keys
      if ((e.keyCode >= 186 && e.keyCode <= 192) || (e.keyCode >= 219 && e.keyCode <= 222))
        char = e.key;
      // console.log('key', e, char);
      if (e.keyCode === 13 || e.keyCode === 32 || e.keyCode === 9 || (e.keyCode >= 48 && e.keyCode <= 90) || (e.keyCode >= 107 && e.keyCode <= 111) || (e.keyCode >= 186 && e.keyCode <= 222)) {
        barcodeBuffer += char;
        scannerDebounced(e, barcodeBuffer);
      }
    }
  };

  const formatField = (e) => {
    switch(e.target.name){
      default:
        break;
      case 'cost':
        part.cost = Number(part.cost).toFixed(2);
        if (isNaN(part.cost))
          part.cost = Number(0).toFixed(2);
        break;
    }
    enableKeyboardListening(e);
    setPart(part);
  };

  const fetchPart = async (partNumber) => {
    Inventory.abortController.abort(); // Cancel the previous request
    Inventory.abortController = new AbortController();
    setLoadingPartMetadata(true);
    try {
      const response = await fetchApi(`part?partNumber=${partNumber}`, {
        signal: Inventory.abortController.signal
      });
      const { data } = response;
      setPart(data);
    } catch (ex) {
      console.error('Exception', ex);
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
    setLoadingPartMetadata(false);
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
    const response = await fetchApi('partType/list');
    const { data } = response;
    const partTypes = _.sortBy(data.map((item) => {
      return {
        key: item.partTypeId,
        value: item.partTypeId,
        text: item.name,
      };
    }), 'text');
    setPartTypes(partTypes);
    setLoadingPartTypes(false);
  };

  const fetchProjects = async () => {
    setLoadingProjects(true);
    const response = await fetchApi('project/list?orderBy=DateCreatedUtc&direction=Descending&results=99');
    const { data } = response;
    const projects = _.sortBy(data.map((item) => {
      return {
        key: item.projectId,
        value: item.projectId,
        text: item.name,
        label: { ...(_.find(ProjectColors, c => c.value === item.color).name !== '' && { color: _.find(ProjectColors, c => c.value === item.color).name }), circular: true, content: item.parts, size: 'tiny' },
      };
    }), 'text');
    // ensure that the current part's projectId can't be set to an invalid project
    if (!_.find(projects, p => p.value === viewPreferences.lastProjectId)) {
      setPart({...part, projectId: ''});
    }
    setLoadingProjects(false);
    setProjects(projects);
  };

  const getMountingTypeById = (mountingTypeId) => {
    switch (mountingTypeId) {
      default:
      case 1:
        return 'through hole';
      case 2:
        return 'surface mount';
    }
  };

  /**
   * Force a save of a possible duplicate part
   * @param {any} e
   */
  const handleForceSubmit = (e) => {
    setDuplicatePartModalOpen(false);
    setPart({...part, allowPotentialDuplicate: true });
    onSubmit(e);
  };

  /**
   * Save the part
   * 
   * @param {any} e
   */
  const onSubmit = async (e) => {   
    const isExisting = part.partId > 0;

    const request = {...part};
    request.partTypeId = part.partTypeId.toString();
    request.mountingTypeId = part.mountingTypeId.toString();
    request.quantity = Number.parseInt(part.quantity) || 0;
    request.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    request.cost = Number.parseFloat(part.cost) || 0.00;
    request.projectId = Number.parseInt(part.projectId) || null;

    const saveMethod = isExisting ? 'PUT' : 'POST';
    const response = await fetch('part', {
      method: saveMethod,
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });

    let saveMessage = '';
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
      } else {
        saveMessage = `Added part ${request.partNumber}!`;
        resetForm(saveMessage);
      }
      // refresh recent parts list
      await fetchRecentRows();
    } else if (response.status === 400){
      // other error (invalid part type, mounting type, etc.)
      saveMessage = `Failed to update, check Part Type and Mounting Type`;
      setSaveMessage(saveMessage);
    }

  };

  const resetForm = (saveMessage = '') => {
    setSaveMessage(saveMessage);
    setMetadataParts([]);
    setDuplicateParts([]);
    setPart(defaultPart);
    setLoadingPartMetadata(false);
    setLoadingPartTypes(false);
    setLoadingProjects(false);
  };

  const updateNumberPicker = (e) => {
    localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastQuantity: e.value }));
    setPart({...part, quantity: e.value.toString()});
  };

  const handleChange= (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    const updatedPart = {...part};
    updatedPart[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0)
          searchDebounced(updatedPart.partNumber);
        break;
      case 'partTypeId':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastPartTypeId: control.value }));
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0)
          searchDebounced(updatedPart.partNumber);
        break;
      case 'mountingTypeId':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastMountingTypeId: control.value }));
        if (updatedPart.partNumber && updatedPart.partNumber.length > 0)
          searchDebounced(updatedPart.partNumber);
        break;
      case 'lowStockThreshold':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lowStockThreshold: control.value }));
        break;
      case 'projectId':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastProjectId: control.value }));
        break;
      case 'location':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastLocation: control.value }));
        break;
      case 'binNumber':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastBinNumber: control.value }));
        break;
      case 'binNumber2':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastBinNumber2: control.value }));
        break;
      default:
          break;
    }
    setPart({...updatedPart});
  };

  const printLabel = async (e) => {
    e.preventDefault();
    await fetchApi(`part/print?partNumber=${part.partNumber}&generateImageOnly=false`, { method: 'POST' });
  };

  const setPartFromMetadata = (metadataParts, suggestedPart) => {
    const entity = {...part};
    const mappedPart = {
      partNumber: suggestedPart.partNumber,
      partTypeId: suggestedPart.partTypeId,
      mountingTypeId: suggestedPart.mountingTypeId,
      packageType: suggestedPart.packageType,
      keywords: suggestedPart.keywords && suggestedPart.keywords.join(' ').toLowerCase(),
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
    };
    entity.supplier = mappedPart.supplier;
    entity.supplierPartNumber = mappedPart.supplierPartNumber;
    if (mappedPart.partTypeId)
    entity.partTypeId = mappedPart.partTypeId || '';
    if (mappedPart.mountingTypeId)
    entity.mountingTypeId = mappedPart.mountingTypeId || '';
    entity.packageType = mappedPart.packageType || '';
    entity.keywords = mappedPart.keywords || '';
    entity.description = mappedPart.description || '';
    entity.manufacturer = mappedPart.manufacturer || '';
    entity.manufacturerPartNumber = mappedPart.manufacturerPartNumber || '';
    entity.productUrl = mappedPart.productUrl || '';
    entity.imageUrl = mappedPart.imageUrl || '';
    if (mappedPart.datasheetUrls.length > 0) {
      entity.datasheetUrl = _.first(mappedPart.datasheetUrls) || '';
    }
    if (mappedPart.supplier === 'DigiKey') {
      entity.digiKeyPartNumber = mappedPart.supplierPartNumber || '';
      const searchResult = _.find(metadataParts, e => { return e !== undefined && e.supplier === 'Mouser' && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber });
      if (searchResult) {
        entity.mouserPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || '';
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }
    if (mappedPart.supplier === 'Mouser') {
      entity.mouserPartNumber = mappedPart.supplierPartNumber || '';
      const searchResult = _.find(metadataParts, e => { return e !== undefined && e.supplier === 'DigiKey' && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber });
      if (searchResult) {
        entity.digiKeyPartNumber = searchResult.supplierPartNumber;
        if (entity.packageType.length === 0) entity.packageType = searchResult.packageType;
        if (entity.datasheetUrl.length === 0) entity.datasheetUrl = _.first(searchResult.datasheetUrls) || '';
        if (entity.imageUrl.length === 0) entity.imageUrl = searchResult.imageUrl;
      }
    }

    const lowestCostPart = _.first(_.sortBy(_.filter(metadataParts, (i) => i.cost > 0), 'cost'));

    entity.cost = lowestCostPart.cost;
    entity.lowestCostSupplier = lowestCostPart.supplier;
    entity.lowestCostSupplierUrl = lowestCostPart.productUrl;
    setPart(entity);
  };

  const handleChooseAlternatePart = (e, part) => {
    e.preventDefault();
    setPartFromMetadata(metadataParts, part);
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
      <Table compact celled selectable size='small' className='partstable'>
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
          {metadataParts.map((p, index) =>
            <Table.Row key={index} onClick={e => handleChooseAlternatePart(e, p)}>
              <Table.Cell>
                {part.supplier === p.supplier && part.supplierPartNumber === p.supplierPartNumber ?
                  <Label ribbon>{p.manufacturerPartNumber}</Label>
                  : p.manufacturerPartNumber}
              </Table.Cell>
              <Table.Cell>{p.manufacturer}</Table.Cell>
              <Table.Cell>{p.partType}</Table.Cell>
              <Table.Cell>{p.supplier}</Table.Cell>
              <Table.Cell>{p.supplierPartNumber}</Table.Cell>
              <Table.Cell>{p.packageType}</Table.Cell>
              <Table.Cell>{getMountingTypeById(p.mountingTypeId)}</Table.Cell>
              <Table.Cell>{p.cost}</Table.Cell>
              <Table.Cell><Image src={p.imageUrl} size='mini'></Image></Table.Cell>
              <Table.Cell>{p.datasheetUrls.map((d, dindex) =>
                d && d.length > 0 && <Button key={dindex} onClick={e => handleHighlightAndVisit(e, d)}>View Datasheet</Button>
              )}</Table.Cell>
            </Table.Row>
          )}
        </Table.Body>
      </Table>
    );
  };

  const renderDuplicateParts = () => {
    return (
      <Table compact celled selectable size='small' className='partstable'>
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
          {duplicateParts.map((p, index) =>
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
              <Table.Cell><Image src={p.imageUrl} size='mini'></Image></Table.Cell>
              <Table.Cell><Button onClick={e => handleHighlightAndVisit(e, p.datasheetUrl)}>View Datasheet</Button></Table.Cell>
            </Table.Row>
          )}
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
      if (row.classList.contains('positive')) row.classList.remove('positive');
    }
    targetNode.classList.toggle('positive');
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  };

  const handleRecentPartClick = async (e, part) => {
    setPartNumber(part.partNumber);
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
    await fetchApi('part/bulk', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    localStorage.setItem('scannedPartsSerialized', JSON.stringify([]));
    setBulkScanIsOpen(false);
    setScannedParts([]);
  };

  const handleScannedPartChange = (e, control, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    scannedPart[control.name] = control.value;
    setScannedParts(scannedParts);
  };

  const deleteScannedPart = (e, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    const scannedPartsDeleted = _.without(scannedParts, _.findWhere(scannedParts, { partNumber: scannedPart.partNumber }));
    localStorage.setItem('scannedPartsSerialized', JSON.stringify(scannedPartsDeleted));
    setScannedParts(scannedPartsDeleted);
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`part`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
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
    setConfirmPartDeleteContent(`Are you sure you want to delete part ${part.partNumber}?`);
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
  };

  const onScannedInputKeyDown = (e, scannedPart) => {
    if (e.keyCode === 13) {
      // copy downward
      let beginCopy = false;
      scannedParts.forEach(part => {
        if (part.partName === scannedPart.partName) beginCopy = true;
        if (beginCopy && part[e.target.name] === '') {
          part[e.target.name] = scannedPart[e.target.name];
        }
      });
      setScannedParts(scannedParts);
    }
  };

  const renderScannedParts = (scannedParts, highlightScannedPart) => {
    if (highlightScannedPart) {
      // reset the css highlight animation
      setTimeout(() => {
        const elements = document.getElementsByClassName('scannedPartAnimation');
        for (let i = 0; i < elements.length; i++) {
          elements[i].classList.add('lastScannedPart');
          if (elements[i].classList.contains('scannedPartAnimation'))
            elements[i].classList.remove('scannedPartAnimation');
        }
      }, 750);
    }
    return (
      <Form>
        <Table compact celled striped size='small'>
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
            {scannedParts.map((p, index) =>
              <Table.Row key={index} className={(highlightScannedPart && p.partNumber === highlightScannedPart.partNumber ? `scannedPartAnimation ${Math.random()}` : '')}>
                <Table.Cell collapsing><Label>{p.partNumber}</Label></Table.Cell>
                <Table.Cell collapsing><Form.Input width={10} value={p.quantity} onChange={(e, c) => handleScannedPartChange(e, c, p)} name='quantity' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} /></Table.Cell>
                <Table.Cell collapsing><Form.Input width={16} placeholder='Home lab' value={p.location} onChange={(e, c) => handleScannedPartChange(e, c, p)} name='location' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} onKeyDown={e => onScannedInputKeyDown(e, p)} /></Table.Cell>
                <Table.Cell collapsing><Form.Input width={14} placeholder='' value={p.binNumber} onChange={(e, c) => handleScannedPartChange(e, c, p)} name='binNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} onKeyDown={e => onScannedInputKeyDown(e, p)} /></Table.Cell>
                <Table.Cell collapsing><Form.Input width={14} placeholder='' value={p.binNumber2} onChange={(e, c) => handleScannedPartChange(e, c, p)} name='binNumber2' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} onKeyDown={e => onScannedInputKeyDown(e, p)} /></Table.Cell>
                <Table.Cell collapsing textAlign='center' verticalAlign='middle'><Button type='button' circular size='mini' icon='delete' title='Delete' onClick={e => deleteScannedPart(e, p)} /></Table.Cell>
              </Table.Row>
            )}
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
          {recentParts.map((p, index) =>
            <Table.Row key={index} onClick={e => handleRecentPartClick(e, p)}>
              <Table.Cell>
                {index === 0 ?
                  <Label ribbon>{p.partNumber}</Label>
                  : p.partNumber}
              </Table.Cell>
              <Table.Cell>{p.quantity}</Table.Cell>
              <Table.Cell>{p.partType}</Table.Cell>
              <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
              <Table.Cell>{p.location}</Table.Cell>
              <Table.Cell>{p.binNumber}</Table.Cell>
              <Table.Cell>{p.binNumber2}</Table.Cell>
            </Table.Row>
          )}
        </Table.Body>
      </Table>
    );
  };

  const matchingPartsList = renderAllMatchingParts(part, metadataParts);
  const title = partNumber ? "Edit Inventory" : "Add Inventory";
  return (
    <div>
      <Modal centered
        open={duplicatePartModalOpen}
        onClose={handleDuplicatePartModalClose}
      >
        <Modal.Header>Duplicate Part</Modal.Header>
        <Modal.Content scrolling>
          <Modal.Description>
            <h3>There is a possible duplicate part already in your inventory.</h3>
            {renderDuplicateParts()}
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleDuplicatePartModalClose}>Cancel</Button>
          <Button primary onClick={handleForceSubmit}>Save Anyways</Button>
        </Modal.Actions>
      </Modal>
      <Confirm open={confirmDeleteIsOpen} onCancel={confirmDeleteClose} onConfirm={handleDeletePart} content={confirmPartDeleteContent} />
      <Form onSubmit={onSubmit}>
        {part.partId > 0 &&
          <Button animated='vertical' circular floated='right' size='mini' onClick={printLabel} style={{ marginTop: '5px'}}>
            <Button.Content visible><Icon name='print' /></Button.Content>
            <Button.Content hidden>Print</Button.Content>
          </Button>
        }
        {part.partNumber && <Image src={'/part/preview?partNumber=' + part.partNumber} width={180} floated='right' style={{ marginTop: '0px' }} />}
        <h1 style={{ display: 'inline-block', marginRight: '30px' }}>{title}</h1>
        <div title='Bulk Barcode Scan' style={{ width: '132px', height: '30px', display: 'inline-block', cursor: 'pointer' }} onClick={handleBulkBarcodeScan}>
          <div className='anim-box'>
            <div className='scanner' />
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-md'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-md'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-lg'></div>
            <div className='anim-item anim-item-sm'></div>
            <div className='anim-item anim-item-md'></div>
          </div>
        </div>
        <Form.Group>
          <Form.Input label='Part' required placeholder='LM358' icon='search' focus value={part.partNumber || ''} onChange={handleChange} name='partNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          <Form.Dropdown label='Part Type' placeholder='Part Type' loading={loadingPartTypes} search selection value={part.partTypeId || ''} options={partTypes} onChange={handleChange} name='partTypeId' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          <Form.Dropdown label='Mounting Type' placeholder='Mounting Type' search selection value={part.mountingTypeId || ''} options={mountingTypes} onChange={handleChange} name='mountingTypeId' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          <Form.Dropdown label='Project' placeholder='My Project' loading={loadingProjects} search selection value={part.projectId || ''} options={projects} onChange={handleChange} name='projectId' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
        </Form.Group>
        <Form.Group>
          <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={disableHelp} content='Use the mousewheel and CTRL/ALT to change step size' trigger={<Form.Field control={NumberPicker} label='Quantity' placeholder='10' min={0} value={part.quantity || ''} onChange={updateNumberPicker} name='quantity' autoComplete='off' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />} />
          <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Location' placeholder='Home lab' value={part.location || ''} onChange={handleChange} name='location' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />} />
          <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Bin Number' placeholder='IC Components 2' value={part.binNumber || ''} onChange={handleChange} name='binNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />} />
          <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Bin Number 2' placeholder='14' value={part.binNumber2 || ''} onChange={handleChange} name='binNumber2' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />} />
          <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={disableHelp} content='Alert when the quantity gets below this value' trigger={<Form.Input label='Low Stock' placeholder='10' value={part.lowStockThreshold || ''} onChange={handleChange} name='lowStockThreshold' width={3} onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />} />
        </Form.Group>
        <Form.Field inline>
          <Button type='submit' primary style={{ marginTop: '10px' }}><Icon name='save' />Save</Button>
          {part.partId > 0 &&
            <Button type='button' title='Delete' onClick={e => confirmDeleteOpen(e, part)}><Icon name='delete' />Delete</Button>
          }
          {saveMessage.length > 0 && <Label pointing='left'>{saveMessage}</Label>}
        </Form.Field>
        <Segment loading={loadingPartMetadata} color='blue'>
          <Header dividing as='h3'>Part Metadata</Header>
          {metadataParts.length > 1 &&
            <Modal centered
              trigger={<Button onClick={handleOpenModal}>Choose alternate part ({metadataParts.length})</Button>}
              open={partModalOpen}
              onClose={handlePartModalClose}
            >
              <Modal.Header>Matching Parts</Modal.Header>
              <Modal.Content scrolling>
                <Modal.Description>
                  {matchingPartsList}
                </Modal.Description>
              </Modal.Content>
            </Modal>
          }
          <Form.Group>
            <Form.Field width={4}>
              <label>Cost</label>
              <Input label='$' placeholder='0.00' value={part.cost} type='text' onChange={handleChange} name='cost' onFocus={disableKeyboardListening} onBlur={formatField} />
            </Form.Field>
            <Form.Input label='Manufacturer' placeholder='Texas Instruments' value={part.manufacturer || ''} onChange={handleChange} name='manufacturer' width={4} onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
            <Form.Input label='Manufacturer Part' placeholder='LM358' value={part.manufacturerPartNumber || ''} onChange={handleChange} name='manufacturerPartNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
            <Image src={part.imageUrl} size='tiny' />
          </Form.Group>
          <Form.Field width={10}>
            <label>Keywords</label>
            <Input icon='tags' iconPosition='left' label={{ tag: true, content: 'Add Keyword' }} labelPosition='right' placeholder='op amp' onChange={handleChange} value={part.keywords || ''} name='keywords' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          </Form.Field>
          <Form.Field width={4}>
            <label>Package Type</label>
            <Input placeholder='DIP8' value={part.packageType || ''} onChange={handleChange} name='packageType' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          </Form.Field>
          <Form.Field width={10} control={TextArea} label='Description' value={part.description || ''} onChange={handleChange} name='description' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          <Form.Field width={10}>
            <label>Datasheet Url</label>
            <Input action className='labeled' placeholder='www.ti.com/lit/ds/symlink/lm2904-n.pdf' value={(part.datasheetUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='datasheetUrl'>
              <Label>http://</Label>
              <input onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
              <Button onClick={e => handleVisitLink(e, part.datasheetUrl)} disabled={!part.datasheetUrl || part.datasheetUrl.length === 0}>View</Button>
            </Input>
          </Form.Field>
          <Form.Field width={10}>
            <label>Product Url</label>
            <Input action className='labeled' placeholder='' value={(part.productUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='productUrl'>
              <Label>http://</Label>
              <input onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
              <Button onClick={e => handleVisitLink(e, part.productUrl)} disabled={!part.productUrl || part.productUrl.length === 0}>Visit</Button>
            </Input>
          </Form.Field>
          <Form.Field width={4}>
            <label>Lowest Cost Supplier</label>
            <Input placeholder='DigiKey' value={part.lowestCostSupplier || ''} onChange={handleChange} name='lowestCostSupplier' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          </Form.Field>
          <Form.Field width={10}>
            <label>Lowest Cost Supplier Url</label>
            <Input action className='labeled' placeholder='' value={(part.lowestCostSupplierUrl || '').replace('http://', '').replace('https://', '')} onChange={handleChange} name='lowestCostSupplierUrl'>
              <Label>http://</Label>
              <input onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
              <Button onClick={e => handleVisitLink(e, part.lowestCostSupplierUrl)} disabled={!part.lowestCostSupplierUrl || part.lowestCostSupplierUrl.length === 0}>Visit</Button>
            </Input>
          </Form.Field>
          <Form.Field width={4}>
            <label>DigiKey Part Number</label>
            <Input placeholder='296-1395-5-ND' value={part.digiKeyPartNumber || ''} onChange={handleChange} name='digiKeyPartNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          </Form.Field>
          <Form.Field width={4}>
            <label>Mouser Part Number</label>
            <Input placeholder='595-LM358AP' value={part.mouserPartNumber || ''} onChange={handleChange} name='mouserPartNumber' onFocus={disableKeyboardListening} onBlur={enableKeyboardListening} />
          </Form.Field>
        </Segment>
        <Modal centered
          open={bulkScanIsOpen}
          onClose={handleBulkScanClose}
        >
          <Modal.Header>Bulk Scan</Modal.Header>
          <Modal.Content>
            <div style={{ width: '200px', height: '100px', margin: 'auto' }}>
              <div className='anim-box'>
                <div className='scanner animated' />
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-md'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-md'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-md'></div>
                <div className='anim-item anim-item-md'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-md'></div>
                <div className='anim-item anim-item-lg'></div>
                <div className='anim-item anim-item-sm'></div>
                <div className='anim-item anim-item-md'></div>
              </div>
            </div>
            <div style={{ textAlign: 'center' }}>
              Start scanning parts...
              {renderScannedParts(scannedParts, highlightScannedPart)}
            </div>
          </Modal.Content>
          <Modal.Actions>
            <Button onClick={() => setBulkScanIsOpen(false)}>Cancel</Button>
            <Button primary onClick={onSubmitScannedParts}>Save</Button>
          </Modal.Actions>
        </Modal>
      </Form>
      <br />
      <div style={{ marginTop: '20px' }}>
        <Segment style={{ minHeight: '50px' }} color='teal'>
          <Header dividing as='h3'>Recently Added</Header>
          <Dimmer active={loadingRecent} inverted>
            <Loader inverted />
          </Dimmer>
          {!loadingRecent && recentParts && renderRecentParts(recentParts)}
        </Segment>
      </div>
    </div>
  );
}

Inventory.abortController = new AbortController();

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <Inventory {...props} params={useParams()} history={useNavigate()} />
);

Inventory.propTypes = {
  partNumber: PropTypes.string
};

Inventory.defaultProps = {
  partNumber: ''
};