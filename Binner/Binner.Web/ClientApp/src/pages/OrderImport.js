import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import _ from 'underscore';
import { Label, Button, Image, Form, Table, Segment, Dimmer, Checkbox, Loader } from 'semantic-ui-react';

export class OrderImport extends Component {
  static displayName = OrderImport.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.state = {
      loading: false,
      metadataParts: [],
      results: {},
      order: {
        orderId: '',
        supplier: '',
      },
      suppliers: [
        {
          key: 1,
          value: 'DigiKey',
          text: 'DigiKey',
        },
        {
          key: 2,
          value: 'Mouser',
          text: 'Mouser',
        },
      ]
    };
    this.onSubmit = this.onSubmit.bind(this);
    this.onImportParts = this.onImportParts.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleChecked = this.handleChecked.bind(this);
  }

  componentDidMount() {
  }

  async onImportParts(e) {
    const { order, results } = this.state;
    this.setState({ loading: true });
    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      parts: _.where(results.parts, { selected: true })
    };
    const response = await fetch('part/importparts', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request),
    });
    const responseData = await response.json();
    // reset form
    this.setState({
      order: { orderId: '', supplier: order.supplier },
      results: {},
      loading: false
    });
  }

  async onSubmit(e) {
    OrderImport.abortController.abort(); // Cancel the previous request
    OrderImport.abortController = new AbortController();
    const { order } = this.state;
    this.setState({ loading: true });
    const request = {
      orderId: order.orderId,
      supplier: order.supplier
    };
    try {
      const response = await fetch('part/import', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request),
        signal: OrderImport.abortController.signal
      });
      const responseData = await response.json();
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }

      if (response.status === 200) {
        const { response: data } = responseData;
        data.parts.forEach((i) => {
          i.selected = true;
        });
        this.setState({ results: data, loading: false });
      }
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  }

  handleChange(e, control) {
    const { order } = this.state;
    switch (control.name) {
      case 'orderId':
        order.orderId = control.value;
        break;
      case 'supplier':
        order.supplier = control.value;
        break;
    }
    this.setState({ order });
  }

  getMountingTypeById(mountingTypeId) {
    switch (mountingTypeId) {
      case 1:
        return 'through hole';
      case 2:
        return 'surface mount';
    }
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

  handleChecked(e, p) {
    const { results } = this.state;
    const foundPart = _.find(results.parts, { supplierPartNumber: p.supplierPartNumber });
    foundPart.selected = !foundPart.selected;
    this.setState({ results });
  }

  renderAllMatchingParts(order) {
    return (
      <div>
        <Form onSubmit={this.onImportParts}>
          <Table compact celled selectable size='small' className='partstable'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell colSpan='11'>
                  <Table>
                    <Table.Body>
                      <Table.Row>
                        <Table.Cell>
                          <Label>Customer Id</Label>
                          {order.customerId}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Order Amount</Label>
                          ${order.amount} {order.currency}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Order Date</Label>
                          {order.orderDate}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Tracking Number</Label>
                          {order.trackingNumber}
                        </Table.Cell>
                      </Table.Row>
                    </Table.Body>
                  </Table>
                </Table.HeaderCell>
              </Table.Row>
              <Table.Row>
                <Table.HeaderCell></Table.HeaderCell>
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
              {order.parts.map((p, index) =>
                <Table.Row key={index}>
                  <Table.Cell><Checkbox toggle checked={p.selected} onChange={(e) => this.handleChecked(e, p)} data={p} /></Table.Cell>
                  <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
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
          <Button primary>Import Parts</Button>
        </Form>
      </div>
    );
  }

  render() {
    const { order, suppliers, results, loading } = this.state;
    return (
      <div>
        <h1>Order Import</h1>
        <Form onSubmit={this.onSubmit}>
          <Form.Group>
            <Form.Input label='Order Id' required placeholder='1023840' icon='search' focus value={order.orderId} onChange={this.handleChange} name='orderId' />
            <Form.Dropdown label='Supplier' placeholder='Choose a Supplier' selection value={order.supplier} options={suppliers} onChange={this.handleChange} name='supplier' />
          </Form.Group>
          <Button primary>Search</Button>
        </Form>
        <div style={{ marginTop: '20px' }}>
          <Segment style={{ minHeight: '50px' }}>
            <Dimmer active={loading} inverted>
              <Loader inverted />
            </Dimmer>
            {!loading && results && results.parts && this.renderAllMatchingParts(results)}
          </Segment>
        </div>
      </div>
    );
  }
}
