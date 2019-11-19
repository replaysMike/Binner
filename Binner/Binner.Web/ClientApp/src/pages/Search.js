import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Table, Visibility, Input, Label, Segment, Button, Confirm, Icon, Responsive } from 'semantic-ui-react';
import { getQueryVariable } from '../common/query';

export class Search extends Component {
  static displayName = Search.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.search.bind(this), 1200);
    this.state = {
      parts: [],
      selectedPart: null,
      keyword: getQueryVariable(props.location.search, 'keyword') || '',
      by: getQueryVariable(props.location.search, 'by') || '',
      byValue: getQueryVariable(props.location.search, 'value') || '',
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      lastSavedPartId: 0,
      loading: true,
      saveMessage: '',
      confirmDeleteIsOpen: false
    };
    this.loadParts = this.loadParts.bind(this);
    this.search = this.search.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handleSearch = this.handleSearch.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLoadPartClick = this.handleLoadPartClick.bind(this);
    this.handleVisitLink = this.handleVisitLink.bind(this);
    this.handlePrintLabel = this.handlePrintLabel.bind(this);
    this.handleSelfLink = this.handleSelfLink.bind(this);
    this.handleDeletePart = this.handleDeletePart.bind(this);
    this.confirmDeleteOpen = this.confirmDeleteOpen.bind(this);
    this.confirmDeleteClose = this.confirmDeleteClose.bind(this);
    this.removeFilter = this.removeFilter.bind(this);
  }

  async componentDidMount() {
    if (this.state.keyword && this.state.keyword.length > 0)
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
      const by = getQueryVariable(nextProps.location.search, 'by') || '';
      const byValue = getQueryVariable(nextProps.location.search, 'value') || '';
      if (keyword && keyword.length > 0) {
        this.setState({ keyword });
        this.search(keyword);
      } else if (by && by.length > 0) {
        this.setState({ by, byValue });
        this.loadParts(this.state.page, true, by, byValue);
      } else {
        this.loadParts(this.state.page, true, '', '');
      }
    }
  }

  async loadParts(page, reset = false, _by = null, _byValue = null) {
    const { records, parts, by, byValue } = this.state;
    let byParameter = _by;
    let byValueParameter = _byValue;
    if (byParameter === null)
      byParameter = by;
    if (byValueParameter === null)
      byValueParameter = byValue;
    let endOfData = false;
    const response = await fetch(`part/list?orderBy=DateCreatedUtc&direction=Descending&results=${records}&page=${page}&by=${byParameter}&value=${byValueParameter}`);
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

  async search(keyword) {
    Search.abortController.abort(); // Cancel the previous request
    Search.abortController = new AbortController();
    this.setState({ loading: true });
    try {
      const response = await fetch(`part/search?keywords=${keyword}`, {
        signal: Search.abortController.signal
      });

      if (response.status === 200) {
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
    if (response.status === 200) {
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

  handleVisitLink(e, url) {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  }

  handleSelfLink(e) {
    e.stopPropagation();
  }

  handleLoadPartClick(e, part) {
    this.props.history.push(`/inventory/${part.partNumber}`);
  }

  async handlePrintLabel(e, part) {
    e.preventDefault();
    e.stopPropagation();
    await fetch(`part/print?partNumber=${part.partNumber}`, { method: 'POST' });
  }

  async handleDeletePart(e) {
    e.preventDefault();
    e.stopPropagation();
    const { selectedPart, parts } = this.state;
    await fetch(`part`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ partId: selectedPart.partId })
    });
    const partsDeleted = _.without(parts, _.findWhere(parts, { partId: selectedPart.partId }));
    this.setState({ confirmDeleteIsOpen: false, parts: partsDeleted, selectedPart: null });
  }

  confirmDeleteOpen(e, part) {
    e.preventDefault();
    e.stopPropagation();
    this.setState({ confirmDeleteIsOpen: true, selectedPart: part });
  }

  confirmDeleteClose(e) {
    e.preventDefault();
    e.stopPropagation();
    this.setState({ confirmDeleteIsOpen: false, selectedPart: null });
  }

  removeFilter(e) {
    e.preventDefault();
    this.setState({ by: '', byValue: '' });
    this.props.history.push(`/inventory`);
  }

  renderParts(parts, column, direction) {
    const { keyword, lastSavedPartId, confirmDeleteIsOpen, loading, by, byValue } = this.state;
    console.log('minWidth', Responsive.onlyMobile);
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <Input placeholder='Search' icon='search' focus value={keyword} onChange={this.handleSearch} name='keyword' />
        <div style={{ paddingTop: '5px'}}>
          {by && <Button primary size='mini' onClick={this.removeFilter}><Icon name='delete' />{by}: {byValue}</Button>}
        </div>
        <div>
          <Table compact celled sortable selectable striped unstackable size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell sorted={column === 'partNumber' ? direction : null} onClick={this.handleSort('partNumber')}>Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'quantity' ? direction : null} onClick={this.handleSort('quantity')}>Quantity</Table.HeaderCell>
                <Responsive as={Table.HeaderCell} minWidth={800} sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={this.handleSort('manufacturerPartNumber')}>Manufacturer Part</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={800} sorted={column === 'description' ? direction : null} onClick={this.handleSort('description')}>Description</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={500} sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>Location</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={600} sorted={column === 'binNumber' ? direction : null} onClick={this.handleSort('binNumber')}>Bin Number</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={700} sorted={column === 'binNumber2' ? direction : null} onClick={this.handleSort('binNumber2')}>Bin Number 2</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={1100} sorted={column === 'cost' ? direction : null} onClick={this.handleSort('cost')}>Cost</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={1200} sorted={column === 'digiKeyPartNumber' ? direction : null} onClick={this.handleSort('digiKeyPartNumber')}>DigiKey Part</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={1300} sorted={column === 'mouserPartNumber' ? direction : null} onClick={this.handleSort('mouserPartNumber')}>Mouser Part</Responsive>
                <Responsive as={Table.HeaderCell} sorted={column === 'datasheetUrl' ? direction : null} onClick={this.handleSort('datasheetUrl')}>Datasheet</Responsive>
                <Responsive as={Table.HeaderCell} minWidth={1400}></Responsive>
                <Table.HeaderCell></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {parts.map(p =>
                <Table.Row key={p.partId} onClick={e => this.handleLoadPartClick(e, p)}>
                  <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>
                  <Table.Cell>
                    <Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed50' onChange={this.handleChange} onClick={e => e.stopPropagation()} onBlur={this.saveColumn} />
                  </Table.Cell>
                  <Responsive as={Table.Cell} minWidth={800}>
                    {p.manufacturerPartNumber}
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={800}>
                    <span className='truncate small' title={p.description}>{p.description}</span>
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={500}>
                    <Link to={`inventory?by=location&value=${p.location}`} onClick={this.handleSelfLink}><span className='truncate'>{p.location}</span></Link>
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={600}>
                    <Link to={`inventory?by=binNumber&value=${p.binNumber}`} onClick={this.handleSelfLink}>{p.binNumber}</Link>
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={700}>
                    <Link to={`inventory?by=binNumber2&value=${p.binNumber2}`} onClick={this.handleSelfLink}>{p.binNumber2}</Link>
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={1100}>
                    ${p.cost}
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={1200}>
                    <span className='truncate'>{p.digiKeyPartNumber}</span>
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={1300}>
                    <span className='truncate'>{p.mouserPartNumber}</span>
                  </Responsive>
                  <Responsive as={Table.Cell} textAlign='center' verticalAlign='middle'>
                    {p.datasheetUrl && <a href='#' onClick={e => this.handleVisitLink(e, p.datasheetUrl)}>View</a>}
                  </Responsive>
                  <Responsive as={Table.Cell} minWidth={1400} textAlign='center' verticalAlign='middle'>
                    <Button circular size='mini' icon='print' title='Print Label' onClick={e => this.handlePrintLabel(e, p)} />
                  </Responsive>
                  <Table.Cell textAlign='center' verticalAlign='middle'>
                    <Button circular size='mini' icon='delete' title='Delete' onClick={e => this.confirmDeleteOpen(e, p)} />
                  </Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </div>
        <Confirm open={this.state.confirmDeleteIsOpen} onCancel={this.confirmDeleteClose} onConfirm={this.handleDeletePart} />
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
