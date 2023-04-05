import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Table, Form, Segment } from 'semantic-ui-react';
import { fetchApi } from '../common/fetchApi';

export class Datasheets extends Component {
  static displayName = Datasheets.name;

  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.fetchPartMetadata.bind(this), 500);
    this.state = {
      parts: [],
      part: {
        partNumber: '',
        partType: '',
        mountingType: '',
      },
      partTypes: [],
      keyword: '',
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      loading: false,
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
    };
    this.fetchPartTypes = this.fetchPartTypes.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleVisitLink = this.handleVisitLink.bind(this);
    this.handleHighlightAndVisit = this.handleHighlightAndVisit.bind(this);
  }

  async componentDidMount() {
    await this.fetchPartTypes();
  }

  async fetchPartMetadata(input) {
    Datasheets.abortController.abort(); // Cancel the previous request
    Datasheets.abortController = new AbortController();
    const { part } = this.state;
    this.setState({ loading: true });
    try {
      const response = await fetchApi(`part/info?partNumber=${input}&partType=${part.partType}&mountingType=${part.mountingType}`, {
        signal: Datasheets.abortController.signal
      });
      const responseData = response.data;
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }
      const { response: data } = responseData;
      this.setState({ parts: data.parts, loading: false });
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  }

  async fetchPartTypes() {
    const response = await fetchApi('partType/list');
    const { data } = response;
    const blankRow = { key: 999, value: null, text: '' };
    let partTypes = _.sortBy(data.map((item) => {
      return {
        key: item.partTypeId,
        value: item.name,
        text: item.name,
      };
    }), 'text');
    partTypes.unshift(blankRow);
    this.setState({ partTypes, loading: false });
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

  handleChange(e, control) {
    const { part } = this.state;
    part[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (control.value && control.value.length > 0)
          this.searchDebounced(control.value);
        break;
      case 'partType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          this.searchDebounced(control.value);
        break;
      case 'mountingType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          this.searchDebounced(control.value);
        break;
      default:
        break;
    }
    this.setState({ part });
  }

  getMountingTypeById(mountingTypeId) {
    switch (mountingTypeId) {
      default:
      case 1:
        return 'through hole';
      case 2:
        return 'surface mount';
    }
  }

  handleVisitLink(e, url) {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
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

  getHostnameFromRegex(url) {
    // run against regex
    const matches = url.match(/^https?:\/\/([^/?#]+)(?:[/?#]|$)/i);
    // extract hostname (will be null if no match is found)
    return matches && matches[1];
  }

  renderParts(parts, column, direction) {
    const { part, partTypes, mountingTypes, loading } = this.state;
    const partsWithDatasheets = _.filter(parts, function (x) { return x.datasheetUrls.length > 0 && _.first(x.datasheetUrls).length > 0; });
    return (
      <div>
        <Form>
          <Form.Group>
            <Form.Input label='Part' required placeholder='LM358' icon='search' focus value={part.partNumber} onChange={this.handleChange} name='partNumber' />
            <Form.Dropdown label='Part Type' placeholder='Part Type' search selection value={part.partType} options={partTypes} onChange={this.handleChange} name='partType' />
            <Form.Dropdown label='Mounting Type' placeholder='Mounting Type' search selection value={part.mountingType} options={mountingTypes} onChange={this.handleChange} name='mountingType' />
          </Form.Group>
        </Form>
        <Segment loading={loading}>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={this.handleSort('manufacturerPartNumber')}>Part</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'manufacturer' ? direction : null} onClick={this.handleSort('manufacturer')}>Manufacturer</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'website' ? direction : null} onClick={this.handleSort('website')}>Website</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'datasheetUrl' ? direction : null} onClick={this.handleSort('datasheetUrl')}>Datasheet</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'package' ? direction : null} onClick={this.handleSort('package')}>Package</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'status' ? direction : null} onClick={this.handleSort('status')}>Status</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {partsWithDatasheets.map((p, i) =>
                <Table.Row key={i}>
                  <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                  <Table.Cell>{p.manufacturer}</Table.Cell>
                  <Table.Cell>{this.getHostnameFromRegex(_.first(p.datasheetUrls))}</Table.Cell>
                  <Table.Cell>{p.datasheetUrls.map((url, dindex) =>
                    <a href={url} alt={url} key={dindex} onClick={e => this.handleHighlightAndVisit(e, url)}>View Datasheet</a>
                  )}
                  </Table.Cell>
                  <Table.Cell>{p.packageType.replace(/\([^()]*\)/g, '')}</Table.Cell>
                  <Table.Cell>{p.status}</Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </Segment>
      </div>
    );
  }

  render() {
    const { parts, column, direction } = this.state;
    let contents = this.renderParts(parts, column, direction);

    return (
      <div>
        <h1>Datasheet Search</h1>
        {contents}
      </div>
    );
  }
}
