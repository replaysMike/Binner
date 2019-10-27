import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Input, Label, Button, TextArea, Image, Form, Table, Segment, Popup, Modal, Dropdown } from 'semantic-ui-react';
import NumberPicker from './NumberPicker';
import { ProjectColors } from './Types';

const inlineStyle = {
  modal: {
    display: 'fixed !important',
    marginTop: '0px !important',
    marginLeft: 'auto',
    marginRight: 'auto'
  }
};

export class AddInventory extends Component {
  static displayName = AddInventory.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.fetchPartMetadata.bind(this), 500);
    const viewPreferences = JSON.parse(localStorage.getItem('viewPreferences')) || {
      helpDisabled:
        false,
      lastPartType: '',
      lastMountingType: '',
      lastQuantity: 1,
      lastProjectId: null,
      lowStockThreshold: 10,
    };
    this.state = {
      recentParts: [],
      metadataParts: [],
      duplicateParts: [],
      viewPreferences,
      partModalOpen: false,
      duplicatePartModalOpen: false,
      part: {
        allowPotentialDuplicate: false,
        partNumber: '',
        quantity: viewPreferences.lastQuantity + '',
        lowStockThreshold: viewPreferences.lowStockThreshold + '',
        partType: viewPreferences.lastPartType || 'IC',
        mountingType: viewPreferences.lastMountingType || 'through hole',
        packageType: '',
        keywords: '',
        description: '',
        datasheetUrl: '',
        digikeyPartNumber: '',
        mouserPartNumber: '',
        location: '',
        binNumber: '',
        binNumber2: '',
        cost: '',
        lowestCostSupplier: '',
        lowestCostSupplierUrl: '',
        productUrl: '',
        manufacturer: '',
        manufacturerPartNumber: '',
        imageUrl: '',
        projectId: viewPreferences.lastProjectId,
        supplier: '',
        supplierPartNumber: ''
      },
      partTypes: [],
      projects: [],
      mountingTypes: [
        {
          key: 999,
          value: null,
          text: '',
        },
        {
          key: 1,
          value: 'through hole',
          text: 'Through Hole',
        },
        {
          key: 2,
          value: 'surface mount',
          text: 'Surface Mount',
        },
      ],
      loading: false
    };

    this.handleChange = this.handleChange.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.handleForceSubmit = this.handleForceSubmit.bind(this);
    this.updateNumberPicker = this.updateNumberPicker.bind(this);
    this.disableHelp = this.disableHelp.bind(this);
    this.handleChooseAlternatePart = this.handleChooseAlternatePart.bind(this);
    this.handleOpenModal = this.handleOpenModal.bind(this);
    this.handleVisitLink = this.handleVisitLink.bind(this);
    this.handlePartModalClose = this.handlePartModalClose.bind(this);
    this.handleDuplicatePartModalClose = this.handleDuplicatePartModalClose.bind(this);
    this.handleHighlightAndVisit = this.handleHighlightAndVisit.bind(this);
  }

  async componentDidMount() {
    await this.fetchPartTypes();
    await this.fetchProjects();
    await this.fetchRecentRows();
  }

  async fetchPartMetadata(input) {
    AddInventory.abortController.abort(); // Cancel the previous request
    AddInventory.abortController = new AbortController();
    const { part } = this.state;
    this.setState({ loading: true });
    try {
      const response = await fetch(`part/info?partNumber=${input}&partType=${part.partType}&mountingType=${part.mountingType}`, {
        signal: AddInventory.abortController.signal
      });
      const responseData = await response.json();
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }
      const { response: data } = responseData;
      let metadataParts = [];
      if (data && data.parts && data.parts.length > 0) {
        metadataParts = data.parts;
        const suggestedPart = data.parts[0];
        this.setPartFromMetadata(metadataParts, suggestedPart);
      }
      this.setState({ metadataParts, loading: false });
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  }

  async fetchRecentRows() {
    const response = await fetch('part/list?orderBy=DateCreatedUtc&direction=Descending&results=10');
    const data = await response.json();
    this.setState({ recentParts: data });
  }

  async fetchPartTypes() {
    const response = await fetch('partType/list');
    const data = await response.json();
    const partTypes = _.sortBy(data.map((item) => {
      return {
        key: item.partTypeId,
        value: item.name,
        text: item.name,
      };
    }), 'text');
    this.setState({ partTypes });
  }

  async fetchProjects() {
    const response = await fetch('project/list?orderBy=DateCreatedUtc&direction=Descending&results=999');
    const data = await response.json();
    const projects = _.sortBy(data.map((item) => {
      return {
        key: item.projectId,
        value: item.name,
        text: item.name,
        label: { ...(_.find(ProjectColors, c => c.value == item.color).name !== '' && { color: _.find(ProjectColors, c => c.value == item.color).name }), circular: true, content: item.parts, size: 'tiny' },
      };
    }), 'text');
    this.setState({ projects });
  }

  getMountingTypeById(mountingTypeId) {
    switch (mountingTypeId) {
      case 1:
        return 'through hole';
      case 2:
        return 'surface mount';
    }
  }

  /**
   * Force a save of a possible duplicate part
   * @param {any} e
   */
  handleForceSubmit(e) {
    const { part } = this.state;
    part.allowPotentialDuplicate = true;
    this.setState({
      duplicatePartModalOpen: false,
      part
    });
    this.onSubmit(e);
  }

  /**
   * Save the part
   * 
   * @param {any} e
   */
  async onSubmit(e) {
    const { part, viewPreferences } = this.state;
    part.quantity = Number.parseInt(part.quantity) || 0;
    part.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    part.cost = Number.parseFloat(part.cost) || 0.00;
    part.projectId = Number.parseInt(part.projectId) || null;

    const response = await fetch('part', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(part)
    });

    if (response.status === 409) {
      // possible duplicate
      const data = await response.json();
      this.setState({ duplicateParts: data.parts, duplicatePartModalOpen: true });
    } else {

      // reset form
      this.setState({
        part: {
          allowPotentialDuplicate: false,
          partNumber: '',
          quantity: viewPreferences.lastQuantity + '',
          lowStockThreshold: viewPreferences.lowStockThreshold + '',
          partType: viewPreferences.lastPartType,
          mountingType: viewPreferences.lastMountingType,
          packageType: '',
          keywords: '',
          description: '',
          datasheetUrl: '',
          digikeyPartNumber: '',
          mouserPartNumber: '',
          location: '',
          binNumber: '',
          binNumber2: '',
          cost: '',
          lowestCostSupplier: '',
          lowestCostSupplierUrl: '',
          productUrl: '',
          manufacturer: '',
          manufacturerPartNumber: '',
          imageUrl: '',
          projectId: viewPreferences.lastProjectId,
          supplier: '',
          supplierPartNumber: '',
        },
      });

      // refresh recent parts list
      await this.fetchRecentRows();
    }

  }

  updateNumberPicker(e) {
    const { part, viewPreferences } = this.state;
    part.quantity = e.value + '';
    localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastQuantity: e.value }));
    this.setState({ part });
  }

  handleChange(e, control) {
    const { part, viewPreferences } = this.state;
    part[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (control.value && control.value.length > 0)
          this.searchDebounced(control.value);
        break;
      case 'partType':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastPartType: control.value }));
        if (control.value && control.value.length > 0)
          this.searchDebounced(control.value);
        break;
      case 'mountingType':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastMountingType: control.value }));
        if (control.value && control.value.length > 0)
          this.searchDebounced(control.value);
        break;
      case 'lowStockThreshold':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lowStockThreshold: control.value }));
        break;
      case 'projectId':
        localStorage.setItem('viewPreferences', JSON.stringify({ ...viewPreferences, lastProjectId: control.value }));
        break;
    }
    this.setState({ part });
  }

  setPartFromMetadata(metadataParts, suggestedPart) {
    const { part } = this.state;
    const mappedPart = {
      partNumber: suggestedPart.partNumber,
      partType: suggestedPart.partType,
      mountingType: suggestedPart.mountingTypeId,
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
    part.supplier = mappedPart.supplier;
    part.supplierPartNumber = mappedPart.supplierPartNumber;
    if (mappedPart.partType && mappedPart.partType.length > 0)
      part.partType = mappedPart.partType || '';
    if (mappedPart.mountingType)
      part.mountingType = this.getMountingTypeById(mappedPart.mountingType) || '';
    part.packageType = mappedPart.packageType || '';
    part.keywords = mappedPart.keywords || '';
    part.description = mappedPart.description || '';
    part.manufacturer = mappedPart.manufacturer || '';
    part.manufacturerPartNumber = mappedPart.manufacturerPartNumber || '';
    part.productUrl = mappedPart.productUrl || '';
    part.imageUrl = mappedPart.imageUrl || '';
    if (mappedPart.datasheetUrls.length > 0) {
      part.datasheetUrl = _.first(mappedPart.datasheetUrls) || '';
    }
    if (mappedPart.supplier === 'DigiKey') {
      part.digikeyPartNumber = mappedPart.supplierPartNumber || '';
      const searchResult = _.find(metadataParts, e => { return e !== undefined && e.supplier === 'Mouser' && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber });
      if (searchResult) {
        part.mouserPartNumber = searchResult.supplierPartNumber;
        if (part.packageType.length === 0) part.packageType = searchResult.packageType;
        if (part.datasheetUrl.length === 0) part.datasheetUrl = _.first(searchResult.datasheetUrls) || '';
        if (part.packageType.length === 0) part.imageUrl = searchResult.imageUrl;
      }
    }
    if (mappedPart.supplier === 'Mouser') {
      part.mouserPartNumber = mappedPart.supplierPartNumber || '';
      const searchResult = _.find(metadataParts, e => { return e !== undefined && e.supplier === 'DigiKey' && e.manufacturerPartNumber === mappedPart.manufacturerPartNumber });
      if (searchResult) {
        part.digikeyPartNumber = searchResult.supplierPartNumber;
        if (part.packageType.length === 0) part.packageType = searchResult.packageType;
        if (part.datasheetUrl.length === 0) part.datasheetUrl = _.first(searchResult.datasheetUrls) || '';
        if (part.packageType.length === 0) part.imageUrl = searchResult.imageUrl;
      }
    }

    const lowestCostPart = _.first(_.sortBy(_.filter(metadataParts, (i) => i.cost > 0), 'cost'));

    part.cost = lowestCostPart.cost;
    part.lowestCostSupplier = lowestCostPart.supplier;
    part.lowestCostSupplierUrl = lowestCostPart.productUrl;
    console.log('updating part metadata', part);
    this.setState({ part });
  }

  handleChooseAlternatePart(e, part) {
    e.preventDefault();
    const { metadataParts } = this.state;
    this.setPartFromMetadata(metadataParts, part);
    this.setState({ partModalOpen: false });
  }

  handleOpenModal(e) {
    e.preventDefault();
    this.setState({ partModalOpen: true });
  }

  renderAllMatchingParts(part, metadataParts) {
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
            <Table.Row key={index} onClick={e => this.handleChooseAlternatePart(e, p)}>
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
              <Table.Cell>{this.getMountingTypeById(p.mountingTypeId)}</Table.Cell>
              <Table.Cell>{p.cost}</Table.Cell>
              <Table.Cell><Image src={p.imageUrl} size='mini'></Image></Table.Cell>
              <Table.Cell>{p.datasheetUrls.map((d, dindex) =>
                <a href='#' key={dindex} onClick={e => this.handleHighlightAndVisit(e, d)}>View Datasheet</a>
              )}</Table.Cell>
            </Table.Row>
          )}
        </Table.Body>
      </Table>
    );
  }

  renderDuplicateParts() {
    const { duplicateParts } = this.state;
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
              <Table.Cell>{this.getMountingTypeById(p.mountingTypeId)}</Table.Cell>
              <Table.Cell><Image src={p.imageUrl} size='mini'></Image></Table.Cell>
              <Table.Cell><a href='#' onClick={e => this.handleHighlightAndVisit(e, p.datasheetUrl)}>View Datasheet</a></Table.Cell>
            </Table.Row>
          )}
        </Table.Body>
      </Table>
    );
  }

  handlePartModalClose() {
    this.setState({ partModalOpen: false });
  }

  handleDuplicatePartModalClose() {
    this.setState({ duplicatePartModalOpen: false });
  }

  disableHelp() {
    const { viewPreferences } = this.state;
    const val = { ...viewPreferences, helpDisabled: true };
    // localStorage.setItem('viewPreferences', JSON.stringify(val));
  }

  handleHighlightAndVisit(e, url) {
    this.handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains('positive')) row.classList.remove('positive');
    }
    targetNode.classList.toggle('positive');
  }

  handleVisitLink(e, url) {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  }

  render() {
    const { part, recentParts, metadataParts, partTypes, mountingTypes, projects, viewPreferences, partModalOpen, duplicatePartModalOpen, loading } = this.state;
    const matchingPartsList = this.renderAllMatchingParts(part, metadataParts);
    return (
      <div>
        <Modal centered
          open={duplicatePartModalOpen}
          onClose={this.handleDuplicatePartModalClose}
        >
          <Modal.Header>Duplicate Part</Modal.Header>
          <Modal.Content scrolling>
            <Modal.Description>
              <h3>There is a possible duplicate part already in your inventory.</h3>
              {this.renderDuplicateParts()}
            </Modal.Description>
          </Modal.Content>
          <Modal.Actions>
            <Button onClick={this.handleDuplicatePartModalClose}>Cancel</Button>
            <Button primary onClick={this.handleForceSubmit}>Save Anyways</Button>
          </Modal.Actions>
        </Modal>
        <Form onSubmit={this.onSubmit}>
          <h1>Add Inventory</h1>
          <Form.Group>
            <Form.Input label='Part' required placeholder='LM358' icon='search' focus value={part.partNumber} onChange={this.handleChange} name='partNumber' />
            <Form.Dropdown label='Part Type' placeholder='Part Type' search selection value={part.partType} options={partTypes} onChange={this.handleChange} name='partType' />
            <Form.Dropdown label='Mounting Type' placeholder='Mounting Type' search selection value={part.mountingType} options={mountingTypes} onChange={this.handleChange} name='mountingType' />
            <Form.Dropdown label='Project' placeholder='My Project' search selection value={part.projectId} options={projects} onChange={this.handleChange} name='projectId' />
          </Form.Group>
          <Form.Group>
            <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={this.disableHelp} content='Use the mousewheel and CTRL/ALT to change step size' trigger={<Form.Field control={NumberPicker} label='Quantity' placeholder='10' min={0} value={part.quantity} onChange={this.updateNumberPicker} name='quantity' autoComplete='off' />} />
            <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={this.disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Location' placeholder='Home lab' value={part.location} onChange={this.handleChange} name='location' />} />
            <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={this.disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Bin Number' placeholder='IC Components 2' value={part.binNumber} onChange={this.handleChange} name='binNumber' />} />
            <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={this.disableHelp} content='A custom value for identifying the parts location' trigger={<Form.Input label='Bin Number 2' placeholder='14' value={part.binNumber2} onChange={this.handleChange} name='binNumber2' />} />
            <Popup hideOnScroll disabled={viewPreferences.helpDisabled} onOpen={this.disableHelp} content='Alert when the quantity gets below this value' trigger={<Form.Input label='Low Stock' placeholder='10' value={part.lowStockThreshold} onChange={this.handleChange} name='lowStockThreshold' width={3} />} />
          </Form.Group>
          <Button type='submit'>Save</Button>
          <Segment loading={loading}>
            {metadataParts.length > 0 &&
              <Modal centered
                trigger={<a href="#" onClick={this.handleOpenModal}>Choose alternate part</a>}
                open={partModalOpen}
                onClose={this.handlePartModalClose}
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
                <Input label='$' placeholder='0.000' value={part.cost} type='text' onChange={this.handleChange} name='cost' />
              </Form.Field>
              <Form.Input label='Manufacturer' placeholder='Texas Instruments' value={part.manufacturer} onChange={this.handleChange} name='manufacturer' width={4} />
              <Form.Input label='Manufacturer Part' placeholder='LM358' value={part.manufacturerPartNumber} onChange={this.handleChange} name='manufacturerPartNumber' />
              <Image src={part.imageUrl} size='tiny' />
            </Form.Group>
            <Form.Field width={10}>
              <label>Keywords</label>
              <Input icon='tags' iconPosition='left' label={{ tag: true, content: 'Add Keyword' }} labelPosition='right' placeholder='op amp' onChange={this.handleChange} value={part.keywords} name='keywords' />
            </Form.Field>
            <Form.Field width={4}>
              <label>Package Type</label>
              <Input placeholder='DIP8' value={part.packageType} onChange={this.handleChange} name='packageType' />
            </Form.Field>
            <Form.Field width={10} control={TextArea} label='Description' value={part.description} onChange={this.handleChange} name='description' />
            <Form.Field width={10}>
              <label>Datasheet Url</label>
              <Input action className='labeled' placeholder='www.ti.com/lit/ds/symlink/lm2904-n.pdf' value={part.datasheetUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='datasheetUrl'>
                <Label>http://</Label>
                <input />
                <Button onClick={e => this.handleVisitLink(e, part.datasheetUrl)} disabled={part.datasheetUrl.length === 0}>View</Button>
              </Input>
            </Form.Field>
            <Form.Field width={10}>
              <label>Product Url</label>
              <Input action className='labeled' placeholder='' value={part.productUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='productUrl'>
                <Label>http://</Label>
                <input />
                <Button onClick={e => this.handleVisitLink(e, part.productUrl)} disabled={part.productUrl.length === 0}>Visit</Button>
              </Input>
            </Form.Field>
            <Form.Field width={4}>
              <label>Lowest Cost Supplier</label>
              <Input placeholder='DigiKey' value={part.lowestCostSupplier} onChange={this.handleChange} name='lowestCostSupplier' />
            </Form.Field>
            <Form.Field width={10}>
              <label>Lowest Cost Supplier Url</label>
              <Input action className='labeled' placeholder='' value={part.lowestCostSupplierUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='lowestCostSupplierUrl'>
                <Label>http://</Label>
                <input />
                <Button onClick={e => this.handleVisitLink(e, part.lowestCostSupplierUrl)} disabled={part.lowestCostSupplierUrl.length === 0}>Visit</Button>
              </Input>
            </Form.Field>
            <Form.Field width={4}>
              <label>DigiKey Part Number</label>
              <Input placeholder='296-1395-5-ND' value={part.digikeyPartNumber} onChange={this.handleChange} name='digikeyPartNumber' />
            </Form.Field>
            <Form.Field width={4}>
              <label>Mouser Part Number</label>
              <Input placeholder='595-LM358AP' value={part.mouserPartNumber} onChange={this.handleChange} name='mouserPartNumber' />
            </Form.Field>
          </Segment>
        </Form>
        <br />
        <h4>Recently added parts</h4>
        <Table compact celled>
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
              <Table.Row key={index}>
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
      </div>
    );
  }
}
