import React, { Component } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Input, Button, Icon } from 'semantic-ui-react';
import { getQueryVariable } from '../common/query';
import PartsGrid from '../components/PartsGrid';

class Search extends Component {
  static displayName = Search.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.searchDebounced = AwesomeDebouncePromise(this.search.bind(this), 1200);
    this.state = {
      parts: [],
      selectedPart: null,
      keyword: getQueryVariable(window.location.search, "keyword") || "",
      by: getQueryVariable(window.location.search, "by") || "",
      byValue: getQueryVariable(window.location.search, "value") || "",
      page: 1,
      records: 50,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      lastSavedPartId: 0,
      loading: true,
      saveMessage: "",
      confirmDeleteIsOpen: false,
    };
    this.loadParts = this.loadParts.bind(this);
    this.search = this.search.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handleSearch = this.handleSearch.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handlePartClick = this.handlePartClick.bind(this);
    this.removeFilter = this.removeFilter.bind(this);
  }

  async componentDidMount() {
    if (this.state.keyword && this.state.keyword.length > 0) await this.search(this.state.keyword);
    else await this.loadParts(this.state.page);
  }

  componentWillUnmount() {
    Search.abortController.abort();
  }

  UNSAFE_componentWillReceiveProps(nextProps) {
    // if the path changes due to a new search via query, reset the keyword and perform the search
    // the component will not be recreated when this happens, only during rerender
    if (nextProps.location.search !== window.location.search) {
      const keyword = getQueryVariable(nextProps.location.search, "keyword") || "";
      const by = getQueryVariable(nextProps.location.search, "by") || "";
      const byValue = getQueryVariable(nextProps.location.search, "value") || "";

      if (keyword && keyword.length > 0) {
        // if there's a keyword we should clear binning (because they use different endpoints)
        this.setState({ keyword, by: "", byValue: "", page: 1 });
        this.search(keyword);
      } else if (by && by.length > 0) {
        // likewise, clear keyword if we're in a bin search
        this.setState({ by, byValue, keyword: "", page: 1 });
        this.loadParts(this.state.page, true, by, byValue);
      } else {
        this.setState({ page: 1 });
        this.loadParts(this.state.page, true, "", "");
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
    this.setState({by: '', byValue: ''}) // if there's a keyword we should clear binning (because they use different endpoints)
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

  handlePartClick(e, part) {
    this.props.history(`/inventory/${part.partNumber}`);
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
      default:
          break;
    }
    this.setState({ keyword: control.value });
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

  removeFilter(e) {
    e.preventDefault();
    this.setState({ by: '', byValue: '' });
    this.props.history(`/inventory`);
  }

  render() {
    const { parts, loading, keyword, by, byValue } = this.state;
    return (
      <div>
        <h1>Inventory</h1>
        <Input placeholder='Search' icon='search' focus value={keyword} onChange={this.handleSearch} name='keyword' />
        <div style={{ paddingTop: '10px', marginBottom: '10px' }}>
          {by && <Button primary size='mini' onClick={this.removeFilter}><Icon name='delete' />{by}: {byValue}</Button>}
        </div>
        <PartsGrid parts={parts} loading={loading} loadPage={this.handleNextPage} noRemainingData={this.state.noRemainingData} onPartClick={this.handlePartClick} name='partsGrid' />
      </div>
    );
  }
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <Search {...props} params={useParams()} history={useNavigate()} location={window.location} />
);