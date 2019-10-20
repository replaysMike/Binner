import React, { Component } from 'react';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Input, Label, Button, TextArea, Grid, Image, Form, Table } from 'semantic-ui-react';

export class AddInventory extends Component {
  static displayName = AddInventory.name;

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.fetchPartMetadata.bind(this), 1200);
    this.state = {
      recentParts: [],
      part: {
        partNumber: '',
        quantity: '',
        lowStockThreshold: '',
        partType: '',
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
        projectId: '',
      },
      loading: false
    };
    this.handleChange = this.handleChange.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
  }

  async fetchPartMetadata(input) {
    const { part } = this.state;
    this.setState({ loading: true });
    const response = await fetch(`part/metadata?partNumber=${input}`);
    const data = await response.json();
    const mappedPart = {
      partNumber: data.partNumber,
      partType: data.partType,
      keywords: data.keywords && data.keywords.join(' ').toLowerCase(),
      description: data.description + '\r\n' + data.detailedDescription,
      datasheetUrl: data.datasheetUrl,
      digikeyPartNumber: data.digikeyPartNumber,
      mouserPartNumber: data.mouserPartNumber,
      cost: data.cost,
      lowestCostSupplier: data.lowestCostSupplier,
      lowestCostSupplierUrl: data.lowestCostSupplierUrl,
      productUrl: data.productUrl,
      manufacturer: data.manufacturer,
      manufacturerPartNumber: data.manufacturerPartNumber,
      imageUrl: data.imageUrl,
    };
    part.partType = mappedPart.partType || '';
    part.keywords = mappedPart.keywords || '';
    part.description = mappedPart.description || '';
    part.datasheetUrl = mappedPart.datasheetUrl || '';
    part.digikeyPartNumber = mappedPart.digikeyPartNumber || '';
    part.mouserPartNumber = mappedPart.mouserPartNumber || '';
    part.cost = mappedPart.cost || '';
    part.lowestCostSupplier = mappedPart.lowestCostSupplier || '';
    part.lowestCostSupplierUrl = mappedPart.lowestCostSupplierUrl || '';
    part.manufacturer = mappedPart.manufacturer || '';
    part.manufacturerPartNumber = mappedPart.manufacturerPartNumber || '';
    part.productUrl = mappedPart.productUrl || '';
    part.imageUrl = mappedPart.imageUrl || '';
    this.setState({ part, loading: false });
  }

  async fetchRecentRows() {
    const response = await fetch('part/list?orderBy=DateCreatedUtc&direction=Descending&results=10');
    const data = await response.json();
    this.setState({ recentParts: data });
  }

  componentDidMount() {
    this.fetchRecentRows();
  }

  handleChange(e, control) {
    const { part } = this.state;
    part[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (control.value && control.value.length > 0)
          this.searchDebounced(control.value);
        break;
    }
    this.setState({ part });
  }

  async onSubmit(e, form) {
    const { part } = this.state;
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

    // reset form
    this.setState({
      part: {
        partNumber: '',
        quantity: '',
        lowStockThreshold: '',
        partType: '',
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
        projectId: '',
      },
    });

    await this.fetchRecentRows();
  }

  render() {
    const { part, recentParts } = this.state;
    return (
      <div>
        <Form onSubmit={this.onSubmit}>
          <h1 id="tabelLabel" >Add Inventory</h1>
          <Grid>
            <Grid.Row>
              <Grid.Column width={4}>
                <Form.Field width={10}>
                  <label>Part</label>
                  <Input placeholder='LM358' icon='search' focus value={part.partNumber} onChange={this.handleChange} name='partNumber' />
                </Form.Field>
                <Form.Field width={8}>
                  <label>Quantity</label>
                  <Input required placeholder='100' value={part.quantity} onChange={this.handleChange} name='quantity' id='quantity' />
                </Form.Field>
              </Grid.Column>
              <Grid.Column width={2}>
                <Image src={part.imageUrl} size='small' circular />
              </Grid.Column>
            </Grid.Row>
            <Grid.Row>
            </Grid.Row>
          </Grid>
          <Form.Field width={4}>
            <label>Location</label>
            <Input placeholder='Home lab' value={part.location} onChange={this.handleChange} name='location' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Bin Number</label>
            <Input placeholder='IC Components 2' value={part.binNumber} onChange={this.handleChange} name='binNumber' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Bin Number 2</label>
            <Input placeholder='14' value={part.binNumber2} onChange={this.handleChange} name='binNumber2' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Part Type</label>
            <Input placeholder='IC' value={part.partType} onChange={this.handleChange} name='partType' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Cost</label>
            <Input label='$' placeholder='0.000' value={part.cost} type='text' onChange={this.handleChange} name='cost' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Manufacturer</label>
            <Input placeholder='Texas Instruments' value={part.manufacturer} onChange={this.handleChange} name='manufacturer' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Manufacturer Part</label>
            <Input placeholder='LM358' value={part.manufacturerPartNumber} onChange={this.handleChange} name='manufacturerPartNumber' />
          </Form.Field>
          <Form.Field width={10}>
            <label>Keywords</label>
            <Input icon='tags' iconPosition='left' label={{ tag: true, content: 'Add Keyword' }} labelPosition='right' placeholder='op amp' onChange={this.handleChange} value={part.keywords} name='keywords' />
          </Form.Field>
          <Form.Field width={10} control={TextArea} label='Description' value={part.description} onChange={this.handleChange} name='description' />
          <Form.Field width={10}>
            <label>Datasheet Url</label>
            <Input label='http://' placeholder='www.ti.com/lit/ds/symlink/lm2904-n.pdf' value={part.datasheetUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='datasheetUrl' />
          </Form.Field>
          <Form.Field width={10}>
            <label>Product Url</label>
            <Input label='http://' placeholder='' value={part.productUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='productUrl' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Lowest Cost Supplier</label>
            <Input placeholder='DigiKey' value={part.lowestCostSupplier} onChange={this.handleChange} name='lowestCostSupplier' />
          </Form.Field>
          <Form.Field width={10}>
            <label>Lowest Cost Supplier Url</label>
            <Input label='http://' placeholder='' value={part.lowestCostSupplierUrl.replace('http://', '').replace('https://', '')} onChange={this.handleChange} name='lowestCostSupplierUrl' />
          </Form.Field>
          <Form.Field width={4}>
            <label>DigiKey Part Number</label>
            <Input placeholder='296-1395-5-ND' value={part.digikeyPartNumber} onChange={this.handleChange} name='digikeyPartNumber' />
          </Form.Field>
          <Form.Field width={4}>
            <label>Mouser Part Number</label>
            <Input placeholder='595-LM358AP' value={part.mouserPartNumber} onChange={this.handleChange} name='mouserPartNumber' />
          </Form.Field>
          <Button type='submit'>Save</Button>
        </Form>
        <Table compact celled>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>Part</Table.HeaderCell>
              <Table.HeaderCell>Quantity</Table.HeaderCell>
              <Table.HeaderCell>Manufacturer Part</Table.HeaderCell>
              <Table.HeaderCell>Location</Table.HeaderCell>
              <Table.HeaderCell>Bin Number</Table.HeaderCell>
              <Table.HeaderCell>Bin Number 2</Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {recentParts.map(p => 
              <Table.Row>
                <Table.Cell>{p.partNumber}</Table.Cell>
                <Table.Cell>{p.quantity}</Table.Cell>
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
