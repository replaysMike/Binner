import React, { useState, useEffect } from 'react';
import { createMedia } from "@artsy/fresnel";
import { Table, Icon, Input, Label, Button, Confirm, Modal, Header, Dropdown, Pagination, Popup, Dimmer, Loader } from 'semantic-ui-react';
import _ from 'underscore';
import { Link } from 'react-router-dom';
import PropTypes from 'prop-types';
import { fetchApi } from '../common/fetchApi';
import "./PartsGrid.css";

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

export default function PartsGrid(props) {
  const [parts, setParts] = useState(props.parts);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(props.totalPages);
  const [loading, setLoading] = useState(props.loading);
  const [selectedPart, setSelectedPart] = useState(props.selectedPart);
  const [editable, setEditable] = useState(props.editable);
  const [visitable, setVisitable] = useState(props.visitable);
  const [column, setColumn] = useState(null);
  const [columns, setColumns] = useState(props.columns);
  const [direction, setDirection] = useState(null);
  const [changeTracker, setChangeTracker] = useState([]);
  const [lastSavedPartId, setLastSavedPartId] = useState(0);
  const [saveMessage, setSaveMessage] = useState('');
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState('Are you sure you want to delete this part?');
  const [modalHeader, setModalHeader] = useState('');
  const [modalContent, setModalContent] = useState('');
  const [modalIsOpen, setModalIsOpen] = useState(false);
  const loadPage = props.loadPage;
  const onPartClick = props.onPartClick;
  const itemsPerPageOptions = [
    { key: 1, text: '10', value: 10 },
    { key: 2, text: '25', value: 25 },
    { key: 3, text: '50', value: 50 },
    { key: 4, text: '100', value: 100 },
  ];

  useEffect(() => {
    setParts(props.parts);
    setLoading(props.loading);
    setColumns(props.columns);
    setPage(props.page);
    setSelectedPart(props.selectedPart);
    setEditable(props.editable);
    setVisitable(props.visitable);
    setTotalPages(Math.ceil(props.totalPages));
  }, [props]);

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setParts(_.sortBy(parts, [clickedColumn]));
      setDirection('ascending');
    } else {
      setParts(parts.reverse());
      setDirection(direction === 'ascending' ? 'descending' : 'ascending');
    }
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
    loadPage(e, control.activePage);
  };

  const save = async (part) => {
    setLoading(true);
    let lastSavedPartId = 0;
    part.partTypeId = part.partTypeId + '';
    part.mountingTypeId = part.mountingTypeId + '';
    part.quantity = Number.parseInt(part.quantity) || 0;
    part.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    part.cost = Number.parseFloat(part.cost) || 0.00;
    part.projectId = Number.parseInt(part.projectId) || null;

    const response = await fetchApi('part', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(part)
    });
    let saveMessage = '';
    if (response.responseObject.status === 200) {
      lastSavedPartId = part.partId;
      saveMessage = `Saved part ${part.partNumber}!`;
    }
    else {
      console.error('failed to save part', response.data);
      saveMessage = `Error saving part ${part.partNumber} - ${response.statusText}!`;
      displayModalContent(saveMessage, 'Error');
    }
    setLoading(false);
    setLastSavedPartId(lastSavedPartId);
    setSaveMessage(saveMessage);
  };

  const saveColumn = async (e) => {
    changeTracker.forEach(async (val) => {
      const part = _.find(parts, { partId: val.partId });
      if (part)
        await save(part);
    });
    setParts(parts);
    setChangeTracker([]);
  };

  const handleChange = (e, control) => {
    const part = _.find(parts, { partId: control.data });
    let changes = [...changeTracker];
    if (part) {
      part[control.name] = control.value;
      if (_.where(changes, { partId: part.partId }).length === 0)
        changes.push({ partId: part.partId });
    }
    setParts(parts);
    setChangeTracker(changes);
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  };

  const handleSelfLink = (e) => {
    e.stopPropagation();
  };

  const handleLoadPartClick = (e, part) => {
    if (onPartClick) onPartClick(e, part);
  };

  const handlePrintLabel = async (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`part/print?partNumber=${part.partNumber}`, { method: 'POST' });
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`part`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ partId: selectedPart.partId })
    });
    const partsDeleted = _.without(parts, _.findWhere(parts, { partId: selectedPart.partId }));
    setConfirmDeleteIsOpen(false);
    setParts(partsDeleted);
    setSelectedPart(null);
  };

  const confirmDeleteOpen = (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(true);
    setSelectedPart(part);
    setConfirmPartDeleteContent(`Are you sure you want to delete part ${part.partNumber}?`);
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
  };

  const getColumns = (columns) => {
    let columnsObject = {};
    const cnames = columns.split(',');
    cnames.forEach(c => {
      columnsObject[c.toLowerCase()] = true;
    });
    return columnsObject;
  };

  const displayModalContent = (content, header = null) => {
    setModalIsOpen(true);
    setModalContent(content);
    setModalHeader(header);
  };

  const handleModalClose = () => {
    setModalIsOpen(false);
  };

  const handlePageSizeChange = (e, control) => {
    setPageSize(control.value);
    if (props.onPageSizeChange)
      props.onPageSizeChange(e, control.value);
  };

  const renderParts = (parts, column, direction) => {
    const col = getColumns(columns);
    return (
      <div>
        <style>{mediaStyles}</style>
        <MediaContextProvider>
          <div style={{float: 'right', verticalAlign: 'middle', fontSize: '0.9em'}}>
            <Dropdown 
              selection
              options={itemsPerPageOptions}
              value={pageSize}
              className='small labeled'
              onChange={handlePageSizeChange}
            />
            <span>records per page</span>
          </div>
          <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size='mini' />

          <Dimmer.Dimmable as={Table} dimmed={loading} id="partsGrid" compact celled sortable selectable striped unstackable size='small'>
            <Table.Header>
              <Table.Row>
                {col.partnumber && <Table.HeaderCell sorted={column === 'partNumber' ? direction : null} onClick={handleSort('partNumber')}>Part</Table.HeaderCell>}
                {col.quantity && <Table.HeaderCell sorted={column === 'quantity' ? direction : null} onClick={handleSort('quantity')}>Quantity</Table.HeaderCell>}
                {col.lowstockthreshold && <Table.HeaderCell sorted={column === 'lowstockthreshold' ? direction : null} onClick={handleSort('lowstockthreshold')}>Low Stock</Table.HeaderCell>}
                {col.manufacturerpartnumber && <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={handleSort('manufacturerPartNumber')}>Manufacturer Part</Table.HeaderCell> }
                {col.description && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'description' ? direction : null} onClick={handleSort('description')}>{renderChildren ? "Description" : null}</Table.HeaderCell>)}}</Media>}              
                {col.location && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'location' ? direction : null} onClick={handleSort('location')}>{renderChildren ? "Location" : null}</Table.HeaderCell>)}}</Media> }
                {col.binnumber && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'binNumber' ? direction : null} onClick={handleSort('binNumber')}>{renderChildren ? "Bin Number" : null}</Table.HeaderCell>)}}</Media> }
                {col.binnumber2 && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'binNumber2' ? direction : null} onClick={handleSort('binNumber2')}>{renderChildren ? "Bin Number 2" : null}</Table.HeaderCell>)}}</Media> }
                {col.cost && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'cost' ? direction : null} onClick={handleSort('cost')}>{renderChildren ? "Cost" : null}</Table.HeaderCell>)}}</Media> }
                {col.digikeypartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'digiKeyPartNumber' ? direction : null} onClick={handleSort('digiKeyPartNumber')}>{renderChildren ? "DigiKey Part" : null}</Table.HeaderCell>)}}</Media> }
                {col.mouserpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'mouserPartNumber' ? direction : null} onClick={handleSort('mouserPartNumber')}>{renderChildren ? "Mouser Part" : null}</Table.HeaderCell>)}}</Media> }
                {col.arrowpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'arrowPartNumber' ? direction : null} onClick={handleSort('arrowPartNumber')}>{renderChildren ? "Arrow Part" : null}</Table.HeaderCell>)}}</Media> }
                {col.datasheeturl && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.HeaderCell className={className} sorted={column === 'datasheetUrl' ? direction : null} onClick={handleSort('datasheetUrl')}>{renderChildren ? "Datasheet" : null}</Table.HeaderCell>)}}</Media> }
                {col.print && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.HeaderCell className={className}></Table.HeaderCell>)}}</Media> }
                {col.delete && <Table.HeaderCell></Table.HeaderCell>}
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {loading && <Table.Row key={-1}><Table.Cell colSpan={13}><Dimmer active={loading} inverted><Loader /></Dimmer></Table.Cell></Table.Row>}
              {parts.length > 0 
              ? parts.map((p, key) =>
                <Table.Row key={key} onClick={e => handleLoadPartClick(e, p)} className={selectedPart === p ? "selected" : ""}>
                  {col.partnumber && (lastSavedPartId === p.partId
                    ? <Table.Cell><Label ribbon={lastSavedPartId === p.partId}>{p.partNumber}</Label></Table.Cell>
                    :<Table.Cell>{p.partNumber}</Table.Cell>
                  )}
                  {col.quantity && <Table.Cell>
                    <Popup 
                      hideOnScroll
                      position="bottom left"
                      content={`The quantity of parts currently in stock.`}
                      trigger={editable ? <Input value={p.quantity} data={p.partId} name='quantity' className='borderless fixed50' onChange={handleChange} onClick={e => e.stopPropagation()} onBlur={saveColumn} /> : <span>{p.quantity}</span>}
                    />                      
                  </Table.Cell>}
                  {col.lowstockthreshold && <Table.Cell>
                    <Popup 
                      hideOnScroll
                      position="bottom left"
                      content={`Quantities below this value will indicate the part is low on stock.`}
                      trigger={<Input value={p.lowStockThreshold} data={p.partId} name='lowStockThreshold' className='borderless fixed50' onChange={handleChange} onClick={e => e.stopPropagation()} onBlur={saveColumn} />}
                    />                      
                  </Table.Cell>}
                  {col.manufacturerpartnumber && <Table.Cell>
                    {p.manufacturerPartNumber}
                  </Table.Cell> }
                  {col.description && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {renderChildren ? <span className='truncate small' title={p.description}>{p.description}</span> : null}
                  </Table.Cell>)}}</Media> }
                  {col.location && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {visitable ? (renderChildren ? <Link to={`/inventory?by=location&value=${p.location}`} onClick={handleSelfLink}><span className='truncate'>{p.location}</span></Link> : null) : <span>{p.location}</span>}
                  </Table.Cell>)}}</Media> }
                  {col.binnumber && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {visitable ? (renderChildren ? <Link to={`/inventory?by=binNumber&value=${p.binNumber}`} onClick={handleSelfLink}>{p.binNumber}</Link> : null) : <span>{p.binNumber}</span>}
                  </Table.Cell>)}}</Media> }
                  {col.binnumber2 && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {visitable ? (renderChildren ? <Link to={`/inventory?by=binNumber2&value=${p.binNumber2}`} onClick={handleSelfLink}>{p.binNumber2}</Link> : null) : <span>{p.binNumber2}</span>}
                  </Table.Cell>)}}</Media> }
                  {col.cost && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {renderChildren ? "$" + p.cost.toFixed(2) : null}
                  </Table.Cell>)}}</Media> }
                  {col.digikeypartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {renderChildren ? <span className='truncate'>{p.digiKeyPartNumber}</span> : null}
                  </Table.Cell>)}}</Media> }
                  {col.mouserpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {renderChildren ? <span className='truncate'>{p.mouserPartNumber}</span> : null}
                  </Table.Cell>)}}</Media> }
                  {col.arrowpartnumber && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className}>
                    {renderChildren ? <span className='truncate'>{p.arrowPartNumber}</span> : null}
                  </Table.Cell>)}}</Media> }
                  {col.datasheeturl && <Media greaterThan="computer">{(className, renderChildren) => { return (<Table.Cell className={className} textAlign='center' verticalAlign='middle'>
                    {renderChildren ? p.datasheetUrl && <Button circular size='mini' icon='file pdf outline' title='View PDF' onClick={e => handleVisitLink(e, p.datasheetUrl)} /> : null}
                  </Table.Cell>)}}</Media> }
                  {col.print && <Media greaterThan="tablet">{(className, renderChildren) => { return (<Table.Cell className={className} textAlign='center' verticalAlign='middle'>
                    {renderChildren ? <Button circular size='mini' icon='print' title='Print Label' onClick={e => handlePrintLabel(e, p)} /> : null}
                  </Table.Cell>)}}</Media> }
                  {col.delete && <Table.Cell textAlign='center' verticalAlign='middle'>
                    <Button circular size='mini' icon='delete' title='Delete part' onClick={e => confirmDeleteOpen(e, p)} />
                  </Table.Cell>}
                </Table.Row>
              )
            : (<Table.Row><Table.Cell colSpan={13} textAlign="center">{props.children && props.children.length > 0 ? props.children : "No results."}</Table.Cell></Table.Row>)}
            </Table.Body>
          </Dimmer.Dimmable>
          <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size='mini' />
        </MediaContextProvider>
        <Confirm open={confirmDeleteIsOpen} onCancel={confirmDeleteClose} onConfirm={handleDeletePart} content={confirmPartDeleteContent} />
        <Modal open={modalIsOpen} onCancel={handleModalClose} onClose={handleModalClose}>
          {modalHeader && <Header>{modalHeader}</Header>}
          <Modal.Content>{modalContent}</Modal.Content>
          <Modal.Actions><Button onClick={handleModalClose}>OK</Button></Modal.Actions>
        </Modal>
      </div>
    );
  };
  return renderParts(parts, column, direction);
};

PartsGrid.propTypes = {
  /** Parts listing to render */
  parts: PropTypes.array.isRequired,
  /** Callback to load next page */
  loadPage: PropTypes.func.isRequired,
  /** Page number */
  page: PropTypes.number.isRequired,
  /** Total pages */
  totalPages: PropTypes.number.isRequired, 
  /** True if loading data */
  loading: PropTypes.bool,
  /** List of columns to display */
  columns: PropTypes.string,
  /** Event handler when a part is clicked */
  onPartClick: PropTypes.func,
  /** Event handler when page size is changed */
  onPageSizeChange: PropTypes.func,
  /** Highlight the selected part if provided */
  selectedPart: PropTypes.object,
  /** True if edit options are exposed */
  editable: PropTypes.bool,
  /** True if links are exposed */
  visitable: PropTypes.bool
};

PartsGrid.defaultProps = {
  loading: true,
  columns: 'partNumber,quantity,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,datasheetUrl,print,delete',
  page: 1,
  totalPages: 1,
  editable: true,
  visitable: true
};