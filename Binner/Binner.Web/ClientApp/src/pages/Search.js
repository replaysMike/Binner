import React, { Component } from 'react';
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Table, Visibility, Input, Label, Segment } from 'semantic-ui-react';
import { getQueryVariable } from '../common/query';

export class Search extends Component {
  static displayName = Search.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.search.bind(this), 1200);
    this.state = {
      parts: [],
      keyword: getQueryVariable(props.location.search, 'keyword') || '',
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      lastSavedPartId: 0,
      loading: true,
      saveMessage: ''
    };
    this.loadParts = this.loadParts.bind(this);
    this.search = this.search.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handleSearch = this.handleSearch.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLoadPartClick = this.handleLoadPartClick.bind(this);
  }

  async componentDidMount() {
    if (this.state.keyword)
      await this.search(this.state.keyword);
    else
      await this.loadParts(this.state.page);
  }

  componentWillUnmount() {
    Search.abortController.abort();
  }

  UNSAFE_componentWillReceiveProps(nextProps) {
    // if the path changes due to a new search via query, reset the keyword and perform the search
    // the component will not be recreated when this happens, only during rerender
    if (nextProps.location.search !== this.props.location.search) {
      const keyword = getQueryVariable(nextProps.location.search, 'keyword') || '';
      this.setState({ keyword });
      this.search(keyword);
    }
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
    part.partTypeId = part.partTypeId + '';
    part.mountingTypeId = part.mountingTypeId + '';
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
    let saveMessage = '';
    if (response.status == 200) {
      const data = await response.json();
      lastSavedPartId = part.partId;
      saveMessage = `Saved part ${part.partNumber}!`;
    }
    else {
      console.log('failed to save part', response);
      saveMessage = `Error saving part ${part.partNumber} - ${response.statusText}!`;
    }
    this.setState({ loading: false, lastSavedPartId, saveMessage });
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

  handleLoadPartClick(e, part) {
    this.props.history.push(`/inventory/${part.partNumber}`);
  }

  renderParts(parts, column, direction) {
    const { keyword, lastSavedPartId, loading } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <Input placeholder='Search' icon='search' focus value={keyword} onChange={this.handleSearch} name='keyword' />
        <Segment loading={loading}>
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
                <Table.Row key={p.partId} onClick={e => this.handleLoadPartClick(e, p)}>
                  <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>
                  <Table.Cell><Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed100' onChange={this.handleChange} onClick={e => e.stopPropagation()} onBlur={this.saveColumn} /></Table.Cell>
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
        </Segment>
      </Visibility>
    );
  }

  render() {
    const { parts, column, direction } = this.state;
    let contents = this.renderParts(parts, column, direction);

    return (
      <div>
        <h1>Inventory</h1>
        {contents}
      </div>
    );
  }
}
