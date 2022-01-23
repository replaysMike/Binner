import { createMedia } from "@artsy/fresnel";
import { Table, Visibility, Input, Label, Button, Confirm, Modal, Header } from 'semantic-ui-react';
import React, { Component } from 'react';
import _ from 'underscore';
import { Link } from 'react-router-dom';
import PropTypes from 'prop-types';

const AppMedia = createMedia({
  breakpoints: {
    mobile: 0,
    tablet: 768,
    computer: 992,
    largeScreen: 1200,
    widescreen: 1920
  }
});

const mediaStyles = AppMedia.createMediaStyle();
const { Media, MediaContextProvider } = AppMedia;

export default class PartsGrid extends Component {
  static displayName = PartsGrid.name;

  static propTypes = {
    /** Parts listing to render */
    parts: PropTypes.array.isRequired,
    /** Callback to load next page */
    loadPage: PropTypes.func.isRequired,
    /** True if loading data */
    loading: PropTypes.bool,
    /** List of columns to display */
    columns: PropTypes.string,
    /** Event handler when a part is clicked */
    onPartClick: PropTypes.func,
    /** Determine if we should show button for loading more results */
    noRemainingData: PropTypes.bool
  }

  static defaultProps = {
    loading: true,
    columns: 'partNumber,quantity,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl,print,delete'
  }

  constructor(props) {
    super(props);
    this.state = {
      parts: props.parts,
      loading: props.loading,
      selectedPart: null,
      column: null,
      direction: null,
      page: 1,
      records: 50,
      changeTracker: [],
      lastSavedPartId: 0,
      saveMessage: '',
      confirmDeleteIsOpen: false,
      confirmPartDeleteContent: 'Are you sure you want to delete this part?',
      modalHeader: '',
      modalContent: '',
      modalIsOpen: false
    };
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.save = this.save.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLoadPartClick = this.handleLoadPartClick.bind(this);
    this.handleVisitLink = this.handleVisitLink.bind(this);
    this.handlePrintLabel = this.handlePrintLabel.bind(this);
    this.handleSelfLink = this.handleSelfLink.bind(this);
    this.handleDeletePart = this.handleDeletePart.bind(this);
    this.confirmDeleteOpen = this.confirmDeleteOpen.bind(this);
    this.confirmDeleteClose = this.confirmDeleteClose.bind(this);
    this.handleModalClose = this.handleModalClose.bind(this);
    this.displayModalContent = this.displayModalContent.bind(this);
  }

  async componentDidMount() {
  }

  componentWillUnmount() {
  }

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.parts !== this.props.parts) {
      this.setState({ parts: this.props.parts });
    }
  }

  handleSort = (clickedColumn) => () => {
    const { parts, column, direction } = this.state;

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
    const { page } = this.state;
    const nextPage = page + 1;
    this.props.loadPage(nextPage);
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

    const response = await fetch('part', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(part)
    });
    let saveMessage = '';
    if (response.status === 200) {
      lastSavedPartId = part.partId;
      saveMessage = `Saved part ${part.partNumber}!`;
    }
    else {
      console.error('failed to save part', response);
      saveMessage = `Error saving part ${part.partNumber} - ${response.statusText}!`;
      this.displayModalContent(saveMessage, 'Error');
    }
    this.setState({ loading: false, lastSavedPartId, saveMessage });
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
    if (this.props.onPartClick) this.props.onPartClick(e, part);
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
    this.setState({ confirmDeleteIsOpen: true, selectedPart: part, confirmPartDeleteContent: `Are you sure you want to delete part ${part.partNumber}?` });
  }

  confirmDeleteClose(e) {
    e.preventDefault();
    e.stopPropagation();
    this.setState({ confirmDeleteIsOpen: false, selectedPart: null });
  }

  getColumns(columns) {
    let columnsObject = {};
    const cnames = columns.split(',');
    cnames.forEach(c => {
      columnsObject[c.toLowerCase()] = true;
    });
    return columnsObject;
  }

  displayModalContent(content, header = null) {
    this.setState({ modalIsOpen: true, modalContent: content, modalHeader: header });
  }

  handleModalClose() {
    this.setState({ modalIsOpen: false });
  }

  renderParts(parts, column, direction) {
    const { lastSavedPartId } = this.state;
    const columns = this.getColumns(this.props.columns);
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <style>{mediaStyles}</style>
        <MediaContextProvider>
            <Table id="partsGrid" compact celled sortable selectable striped unstackable size='small'>
              <Table.Header>
                <Table.Row>
                  {columns.partnumber && <Table.HeaderCell sorted={column === 'partNumber' ? direction : null} onClick={this.handleSort('partNumber')}>Part</Table.HeaderCell>}
                  {columns.quantity && <Table.HeaderCell sorted={column === 'quantity' ? direction : null} onClick={this.handleSort('quantity')}>Quantity</Table.HeaderCell>}
                  {columns.lowstockthreshold && <Table.HeaderCell sorted={column === 'lowstockthreshold' ? direction : null} onClick={this.handleSort('lowstockthreshold')}>Low Stock</Table.HeaderCell>}
                  {columns.manufacturerpartnumber && <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={this.handleSort('manufacturerPartNumber')}>Manufacturer Part</Table.HeaderCell> }
                  {columns.description && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'description' ? direction : null} onClick={this.handleSort('description')}>{renderChildren ? "Description" : null}</Table.HeaderCell>)}}</Media>}              
                  {columns.location && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>{renderChildren ? "Location" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.binnumber && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'binNumber' ? direction : null} onClick={this.handleSort('binNumber')}>{renderChildren ? "Bin Number" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.binnumber2 && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'binNumber2' ? direction : null} onClick={this.handleSort('binNumber2')}>{renderChildren ? "Bin Number 2" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.cost && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'cost' ? direction : null} onClick={this.handleSort('cost')}>{renderChildren ? "Cost" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.digikeypartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'digiKeyPartNumber' ? direction : null} onClick={this.handleSort('digiKeyPartNumber')}>{renderChildren ? "DigiKey Part" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.mouserpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'mouserPartNumber' ? direction : null} onClick={this.handleSort('mouserPartNumber')}>{renderChildren ? "Mouser Part" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.datasheeturl && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'datasheetUrl' ? direction : null} onClick={this.handleSort('datasheetUrl')}>{renderChildren ? "Datasheet" : null}</Table.HeaderCell>)}}</Media> }
                  {columns.print && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className}></Table.HeaderCell>)}}</Media> }
                  {columns.delete && <Table.HeaderCell></Table.HeaderCell>}
                </Table.Row>
              </Table.Header>
              <Table.Body>
                {parts.map(p =>
                  <Table.Row key={p.partId} onClick={e => this.handleLoadPartClick(e, p)}>
                    {columns.partnumber && <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>}
                    {columns.quantity && <Table.Cell>
                      <Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed50' onChange={this.handleChange} onClick={e => e.stopPropagation()} onBlur={this.saveColumn} />
                    </Table.Cell>}
                    {columns.lowstockthreshold && <Table.Cell>
                      <Input value={p.lowStockThreshold} data={p.partId} name='lowStockThreshold' className='borderless fixed50' onChange={this.handleChange} onClick={e => e.stopPropagation()} onBlur={this.saveColumn} />
                    </Table.Cell>}
                    {columns.manufacturerpartnumber && <Table.Cell>
                      {p.manufacturerPartNumber}
                    </Table.Cell> }
                    {columns.description && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <span className='truncate small' title={p.description}>{p.description}</span> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.location && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <Link to={`inventory?by=location&value=${p.location}`} onClick={this.handleSelfLink}><span className='truncate'>{p.location}</span></Link> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.binnumber && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <Link to={`inventory?by=binNumber&value=${p.binNumber}`} onClick={this.handleSelfLink}>{p.binNumber}</Link> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.binnumber2 && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <Link to={`inventory?by=binNumber2&value=${p.binNumber2}`} onClick={this.handleSelfLink}>{p.binNumber2}</Link> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.cost && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? "$" + p.cost.toFixed(2) : null}
                    </Table.Cell>)}}</Media> }
                    {columns.digikeypartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <span className='truncate'>{p.digiKeyPartNumber}</span> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.mouserpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                      {renderChildren ? <span className='truncate'>{p.mouserPartNumber}</span> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.datasheeturl && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className} textAlign='center' verticalAlign='middle'>
                      {renderChildren ? p.datasheetUrl && <button onClick={e => this.handleVisitLink(e, p.datasheetUrl)}>View</button> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.print && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className} textAlign='center' verticalAlign='middle'>
                      {renderChildren ? <Button circular size='mini' icon='print' title='Print Label' onClick={e => this.handlePrintLabel(e, p)} /> : null}
                    </Table.Cell>)}}</Media> }
                    {columns.delete && <Table.Cell textAlign='center' verticalAlign='middle'>
                      <Button circular size='mini' icon='delete' title='Delete' onClick={e => this.confirmDeleteOpen(e, p)} />
                    </Table.Cell>}
                  </Table.Row>
                )}
              </Table.Body>
            </Table>
            {!this.props.noRemainingData && <Button onClick={this.handleNextPage}>Load More Parts</Button>}
            {this.props.noRemainingData && <Button disabled={true}>No Additional Parts</Button>}
        </MediaContextProvider>
        <Confirm open={this.state.confirmDeleteIsOpen} onCancel={this.confirmDeleteClose} onConfirm={this.handleDeletePart} content={this.state.confirmPartDeleteContent} />
        <Modal open={this.state.modalIsOpen} onCancel={this.handleModalClose} onClose={this.handleModalClose}>
          {this.state.modalHeader && <Header>{this.state.modalHeader}</Header>}
          <Modal.Content>{this.state.modalContent}</Modal.Content>
          <Modal.Actions><Button onClick={this.handleModalClose}>OK</Button></Modal.Actions>
        </Modal>
      </Visibility>
    );
  }

  render() {
    const { parts, column, direction } = this.state;

    return this.renderParts(parts, column, direction);
  }

}