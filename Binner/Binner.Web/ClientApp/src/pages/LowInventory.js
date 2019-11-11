import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label, Segment, Button } from 'semantic-ui-react';

export class LowInventory extends Component {
  static displayName = LowInventory.name;

  constructor(props) {
    super(props);
    this.state = {
      parts: [],
      keyword: '',
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      lastSavedPartId: 0,
      loading: true
    };
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLoadPartClick = this.handleLoadPartClick.bind(this);
    this.handleVisitLink = this.handleVisitLink.bind(this);
    this.handlePrintLabel = this.handlePrintLabel.bind(this);
  }

  async componentDidMount() {
    await this.loadParts(this.state.page);
  }

  async loadParts(page, reset = false) {
    const { records, parts } = this.state;
    let endOfData = false;
    const response = await fetch(`part/low?orderBy=DateCreatedUtc&direction=Descending&results=${records}&page=${page}`);
    const pageOfData = await response.json();
    if (pageOfData && pageOfData.length === 0)
      endOfData = true;
    let newData = [];
    if (reset)
      newData = [...pageOfData];
    else
      newData = [...parts, ...pageOfData];
    this.setState({ parts: newData, page, noRemainingData: endOfData, loading: false });
  }

  async save(part) {
    this.setState({ loading: true });
    let lastSavedPartId = 0;
    part.quantity = Number.parseInt(part.quantity) || 0;
    part.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    part.cost = Number.parseFloat(part.cost) || 0.00;
    part.projectId = Number.parseInt(part.projectId) || null;
    part.keywords = part.keywords && part.keywords.join(' ').toLowerCase();
    const response = await fetch('part', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(part)
    });
    if (response.status === 200) {
      const data = await response.json();
      lastSavedPartId = part.partId;
    }
    else
      console.log('failed to save part');
    this.setState({ loading: false, lastSavedPartId });
  }

  handleSort = (clickedColumn) => () => {
    const { column, parts, direction } = this.state

    if (column !== clickedColumn) {
      this.setState({
        column: clickedColumn,
        parts: _.sortBy(parts, [clickedColumn]),
        direction: 'ascending',
      })
    } else {
      this.setState({
        parts: parts.reverse(),
        direction: direction === 'ascending' ? 'descending' : 'ascending',
      })
    }
  }

  handleNextPage() {
    const { page, noRemainingData } = this.state;
    if (noRemainingData) return;

    const nextPage = page + 1;
    this.loadParts(nextPage);
  }

  async saveColumn(e) {
    const { parts, changeTracker } = this.state;
    changeTracker.forEach(async (val) => {
      const part = _.find(parts, { partId: val.partId });
      if (part)
        await this.save(part);
    });
    this.setState({ parts, changeTracker: [] });
  }

  handleChange(e, control) {
    const { parts, changeTracker } = this.state;
    const part = _.find(parts, { partId: control.data });
    let changes = [...changeTracker];
    if (part) {
      part[control.name] = control.value;
      if (_.where(changes, { partId: part.partId }).length === 0)
        changes.push({ partId: part.partId });
    }
    this.setState({ parts, changeTracker: changes });
  }

  handleVisitLink(e, url) {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  }

  handleLoadPartClick(e, part) {
    this.props.history.push(`/inventory/${part.partNumber}`);
  }

  handlePrintLabel(e, part) {
    e.preventDefault();
    e.stopPropagation();
    fetch(`part/print?partNumber=${part.partNumber}`, { method: 'POST' });
  }

  renderParts(parts, column, direction) {
    const { lastSavedPartId, loading } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <Segment loading={loading}>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell sorted={column === 'partNumber' ? direction : null} onClick={this.handleSort('partNumber')}>Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'quantity' ? direction : null} onClick={this.handleSort('quantity')}>Quantity</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'lowStockThreshold' ? direction : null} onClick={this.handleSort('lowStockThreshold')}>Low Threshold</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={this.handleSort('manufacturerPartNumber')}>Manufacturer Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'description' ? direction : null} onClick={this.handleSort('description')}>Description</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>Location</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'binNumber' ? direction : null} onClick={this.handleSort('binNumber')}>Bin Number</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'binNumber2' ? direction : null} onClick={this.handleSort('binNumber2')}>Bin Number 2</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'digiKeyPartNumber' ? direction : null} onClick={this.handleSort('digiKeyPartNumber')}>DigiKey Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'mouserPartNumber' ? direction : null} onClick={this.handleSort('mouserPartNumber')}>Mouser Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'datasheetUrl' ? direction : null} onClick={this.handleSort('datasheetUrl')}>Datasheet</Table.HeaderCell>
                <Table.HeaderCell>Print</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {parts.map(p =>
                <Table.Row key={p.partId} onClick={e => this.handleLoadPartClick(e, p)}>
                  <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>
                  <Table.Cell><Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed100' onClick={e => e.stopPropagation()} onChange={this.handleChange} onBlur={this.saveColumn} /></Table.Cell>
                  <Table.Cell><Input value={p.lowStockThreshold} data={p.partId} name='lowStockThreshold' className='borderless fixed100' onClick={e => e.stopPropagation()} onChange={this.handleChange} onBlur={this.saveColumn} /></Table.Cell>
                  <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                  <Table.Cell><span className='truncate small' title={p.description}>{p.description}</span></Table.Cell>
                  <Table.Cell><span className='truncate'>{p.location}</span></Table.Cell>
                  <Table.Cell>{p.binNumber}</Table.Cell>
                  <Table.Cell>{p.binNumber2}</Table.Cell>
                  <Table.Cell><span className='truncate'>{p.digiKeyPartNumber}</span></Table.Cell>
                  <Table.Cell><span className='truncate'>{p.mouserPartNumber}</span></Table.Cell>
                  <Table.Cell>{p.datasheetUrl && <a href='#' onClick={e => this.handleVisitLink(e, p.datasheetUrl)}>View Datasheet</a>}</Table.Cell>
                  <Table.Cell><Button circular size='mini' icon='print' onClick={e => this.handlePrintLabel(e, p)} /></Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </Segment>
      </Visibility>
    );
  }

  render() {
    const { parts, column, direction } = this.state;
    let contents = this.renderParts(parts, column, direction);

    return (
      <div>
        <h1>Low Inventory</h1>
        {contents}
      </div>
    );
  }
}
