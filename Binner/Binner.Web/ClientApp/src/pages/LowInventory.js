import React, { Component } from 'react';
import PartsGrid from "../components/PartsGrid";

export class LowInventory extends Component {
  static displayName = LowInventory.name;

  constructor(props) {
    super(props);
    this.state = {
      parts: [],
      keyword: '',
      page: 1,
      records: 50,
      column: null,
      direction: null,
      noRemainingData: false,
      changeTracker: [],
      lastSavedPartId: 0,
      loading: true
    };
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handlePartClick = this.handlePartClick.bind(this);
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

  handleNextPage() {
    const { page, noRemainingData } = this.state;
    if (noRemainingData) return;

    const nextPage = page + 1;
    this.loadParts(nextPage);
  }

  handlePartClick(e, part) {
    this.props.history.push(`/inventory/${part.partNumber}`);
  }
  
  render() {
    const { parts } = this.state;
    const columns =  'partNumber,quantity,lowStockThreshold,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl,delete';
    
    return (
      <div>
        <h1>Low Inventory</h1>
        <PartsGrid parts={parts} columns={columns} loadPage={this.handleNextPage} noRemainingData={this.state.noRemainingData} onPartClick={this.handlePartClick} name='partsGrid' />
      </div>
    );
  }
}
