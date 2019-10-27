import React, { Component } from 'react';
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Table, Visibility, Input, Label } from 'semantic-ui-react';

export class Search extends Component {
  static displayName = Search.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.search.bind(this), 1200);
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
    this.handleSearch = this.handleSearch.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
    this.handleChange = this.handleChange.bind(this);
  }

  async componentDidMount() {
    await this.loadParts(this.state.page);
  }

  async loadParts(page, reset = false) {
    const { records, parts } = this.state;
    let endOfData = false;
    const response = await fetch(`part/list?orderBy=DateCreatedUtc&direction=Descending&results=${records}&page=${page}`);
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

  async search(input) {
    Search.abortController.abort(); // Cancel the previous request
    Search.abortController = new AbortController();
    this.setState({ loading: true });
    try {
      const response = await fetch(`part/search?keywords=${input}`, {
        signal: Search.abortController.signal
      });

      if (response.status == 200) {
        const data = await response.json();
        this.setState({ parts: data || [], loading: false, noRemainingData: true });
      }
      else
        this.setState({ parts: [], loading: false, noRemainingData: true });
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
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
    if (response.status == 200) {
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

  handleSearch(e, control) {
    switch (control.name) {
      case 'keyword':
        if (control.value && control.value.length > 0) {
          this.searchDebounced(control.value);
        } else {
          this.loadParts(this.state.page, true);
        }
        break;
    }
    this.setState({ keyword: control.value });
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

  renderParts(parts, column, direction) {
    const { keyword, lastSavedPartId } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <Input placeholder='Search' icon='search' focus value={keyword} onChange={this.handleSearch} name='keyword' />
        <Table compact celled sortable selectable striped size='small'>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell sorted={column === 'partNumber' ? direction : null} onClick={this.handleSort('partNumber')}>Part</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'quantity' ? direction : null} onClick={this.handleSort('quantity')}>Quantity</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={this.handleSort('manufacturerPartNumber')}>Manufacturer Part</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>Location</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'binNumber' ? direction : null} onClick={this.handleSort('binNumber')}>Bin Number</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'binNumber2' ? direction : null} onClick={this.handleSort('binNumber2')}>Bin Number 2</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'digiKeyPartNumber' ? direction : null} onClick={this.handleSort('digiKeyPartNumber')}>DigiKey Part</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'mouserPartNumber' ? direction : null} onClick={this.handleSort('mouserPartNumber')}>Mouser Part</Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {parts.map(p =>
              <Table.Row key={p.partId} onClick={this.handleClick}>
                <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>
                <Table.Cell><Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed100' onChange={this.handleChange} onBlur={this.saveColumn} /></Table.Cell>
                <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                <Table.Cell>{p.location}</Table.Cell>
                <Table.Cell>{p.binNumber}</Table.Cell>
                <Table.Cell>{p.binNumber2}</Table.Cell>
                <Table.Cell>{p.digiKeyPartNumber}</Table.Cell>
                <Table.Cell>{p.mouserPartNumber}</Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table>
      </Visibility>
    );
  }

  render() {
    const { parts, column, direction, loading } = this.state;
    let contents = loading
      ? <p><em>Loading...</em></p>
      : this.renderParts(parts, column, direction);

    return (
      <div>
        <h1>Inventory</h1>
        {contents}
      </div>
    );
  }
}
