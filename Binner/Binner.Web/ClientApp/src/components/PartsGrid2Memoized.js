import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { createMedia } from "@artsy/fresnel";
import { Button, Confirm, Modal, Header, Dropdown, Pagination, Popup } from "semantic-ui-react";
import { Clipboard } from "./Clipboard";
import { MaterialReactTable, useMaterialReactTable } from "material-react-table";
import _ from "underscore";
import { Link } from "react-router-dom";
import PropTypes from "prop-types";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData } from "../common/storage";
import { formatCurrency } from "../common/Utils";
import { getIcon } from "../common/partTypes";
import "./PartsGrid2.css";
import { toast } from "react-toastify";
import { RecordSize } from "./RecordSize";

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

export default function PartsGrid2Memoized({
    loading = true,
    columns = "partNumber,partId,quantity,value,lowStockThreshold,manufacturerPartNumber,description,partType,packageType,mountingType,location,binNumber,binNumber2,cost,digiKeyPartNumber,mouserPartNumber,arrowPartNumber,tmePartNumber,datasheetUrl,print,delete,symbolName,footprintName,extensionValue1,extensionValue2",
    defaultVisibleColumns = "partNumber,quantity,value,manufacturerPartNumber,description,partType,location,binNumber,binNumber2,cost,datasheetUrl,print,delete",
    page = 1,
    totalPages = 1,
    totalRecords = 0,
    selectedPart,
    editable = true,
    visitable = true,
    visitUrl = '/inventory',
    keyword = '',
    by = [],
    byValue = [],
    parts = [],
    disabledPartIds = [],
    enableMultiSelect = false,
    ...rest
  }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();

  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'partsGridViewPreferences', location })
  };

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'partsGridViewPreferences', location });
  };

  const createDefaultVisibleColumns = (columns, defaultVisibleColumns) => {
    const columnsArray = columns.split(',');
    const visibleColumnsArray = defaultVisibleColumns.split(',');
    let result = {};
    for(let i = 0; i < columnsArray.length; i++) {
      result[columnsArray[i]] = visibleColumnsArray.includes(columnsArray[i]);
    }
    return result;
  };

  const [_parts, setParts] = useState(parts);
  const [_page, setPage] = useState(page);
  const [pageSize, setPageSize] = useState(getViewPreference('pageSize') || 25);
  const [_totalPages, setTotalPages] = useState(totalPages);
  const [isLoading, setIsLoading] = useState(loading);
  const [_selectedPart, setSelectedPart] = useState(selectedPart);
  const [_editable, setEditable] = useState(editable);
  const [_visitable, setVisitable] = useState(visitable);
  const [_columns, setColumns] = useState(columns);
  const [columnsArray, setColumnsArray] = useState(columns.split(','));
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [modalHeader, setModalHeader] = useState("");
  const [modalContent, setModalContent] = useState("");
  const [modalIsOpen, setModalIsOpen] = useState(false);
  const [partTypes, setPartTypes] = useState();
  const [columnVisibility, setColumnVisibility] = useState(getViewPreference('columnVisibility') || createDefaultVisibleColumns(columns, defaultVisibleColumns));
  const [columnsVisibleArray, setColumnsVisibleArray] = useState(defaultVisibleColumns.split(','));
  const [columnOrder, setColumnOrder] = useState(getViewPreference('columnOrder') || []);
  const [sorting, setSorting] = useState([]);
  const [_disabledPartIds, setDisabledPartIds] = useState(disabledPartIds);
  const [rowSelection, setRowSelection] = useState({});

  const loadPartTypes = useCallback((parentPartType = "") => {
    setIsLoading(true);
    if (parentPartType === undefined || parentPartType === null) parentPartType = "";
    fetchApi(`/api/partType/all?parent=${parentPartType}`).then((response) => {
      const { data } = response;

      setPartTypes(data);
      setIsLoading(false);
    });
  }, []);

  useEffect(() => {
    if (rest.onInit) rest.onInit({ pageSize});
    loadPartTypes();
  }, [loadPartTypes]);

  useEffect(() => {
    setParts(parts);
    setIsLoading(loading);
    setColumns(columns);
    setColumnsArray(columns.split(','));
    setColumnsVisibleArray(defaultVisibleColumns.split(','));
    setPage(page);
    setSelectedPart(selectedPart);
    setEditable(editable);
    setVisitable(visitable);
    setTotalPages(totalPages);
  }, [parts, loading, columns, defaultVisibleColumns, page, selectedPart, editable, visitable, totalPages]);

  useEffect(() => {
    setDisabledPartIds(disabledPartIds);
  }, [disabledPartIds]);

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
    if (rest.loadPage) rest.loadPage(e, control.activePage);
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const handlePrintLabel = async (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`/api/part/print?partNumber=${encodeURIComponent(part.partNumber.trim())}`, { method: "POST" });
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (!_selectedPart) {
      toast.error('Error - no part selected!');
      return;
    }
    
    await fetchApi(`/api/part`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ partId: _selectedPart.partId })
    });
    const partsDeleted = _.without(_parts, _.findWhere(_parts, { partId: _selectedPart.partId }));
    setConfirmDeleteIsOpen(false);
    setParts(partsDeleted);
    setSelectedPart(null);
  };

  const confirmDeleteOpen = (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    setSelectedPart(part);
    setConfirmDeleteIsOpen(true);
    setConfirmPartDeleteContent(
      <p>
        <Trans i18nKey="confirm.deletePart" name={part.partNumber}>
        Are you sure you want to delete part <b>{{name: part.partNumber }}</b>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPart(null);
  };

  const handleModalClose = () => {
    setModalIsOpen(false);
  };

  useEffect(() => {
    // fire events when selected rows changes
    var partIds = Object.keys(rowSelection).map(i => parseInt(i));
    const parts = _parts.filter(p => partIds.includes(p.partId));
    if (rest.onSelectedPartsChange) rest.onSelectedPartsChange(null, parts);
  }, [rowSelection]);

  const handlePageSizeChange = (e, control) => {
    setPageSize(control.value);
    setPage(1);
    setViewPreference('pageSize', control.value);
    if (rest.onPageSizeChange) rest.onPageSizeChange(e, control.value);
  };

  const getColumnSize = (columnName) => {
    switch (columnName) {
      case "partNumber":
        return 180;
      case "partId":
        return 180;
      case "quantity":
        return 140;
      case "value":
        return 100;
      case "lowStockThreshold":
        return 220;
      case "manufacturerPartNumber":
        return 250;
      case "description":
        return 220;
      case "partType":
        return 150;
      case "location":
        return 150;
      case "binNumber":
        return 175;
      case "binNumber2":
        return 175;
      case "cost":
        return 100;
      case "digikeyPartNumber":
        return 220;
      case "mouserPartNumber":
        return 220;
      case "arrowPartNumber":
        return 220;
      case "tmePartNumber":
        return 220;
      case "actions":
        return 150;
      default:
        return 180;
    }
  };

  const indexOfBy = (filterBy) => {
    for(let i = 0; i < by.length; i++) {
      if (by[i] === filterBy) {
        return i;
      }
    }
    return -1;
  };

  const createFilterBy = (filterByToAdd, filterByValueToAdd) => {
    const newFilterBy = [...by];
    const newFilterByValue = [...byValue];
    
    if (filterByValueToAdd?.length > 0 && !newFilterByValue.includes(filterByValueToAdd)) {
      newFilterBy.push(filterByToAdd);
      newFilterByValue.push(filterByValueToAdd);
    }
    return `by=${newFilterBy.join(',')}&value=${newFilterByValue.join(',')}`;
  };

  const tableColumns = useMemo(() => {
    const handleSelfLink = (e, part, propertyName) => {
      e.preventDefault();
      e.stopPropagation();
      if (part[propertyName]) {
        const url = `${visitUrl}?${createFilterBy(propertyName, part[propertyName])}&keyword=${keyword}&_=${Math.random()}`;
        navigate(url);
      }
    };

    const getIconForPart = (p) => {
      const partType = _.find(partTypes, (x) => x.partTypeId === p.partTypeId);
      if (!partType) {
        // part type wasn't found, bad data
        return "";
      }
      const basePartTypeName = partType.parentPartTypeId && _.find(partTypes, (x) => x.partTypeId === partType.parentPartTypeId)?.name;
      const partTypeName = partType.name;
      if (partType) return getIcon(partTypeName, basePartTypeName)({className: `parttype-${basePartTypeName || partTypeName}`});
      return "";
    };

    const getColumnDefinition = (columnName, key) => {
      const translatedColumnName = t(`comp.partsGrid.${columnName}`, `${columnName}`);
  
      const def = {
        accessorKey: columnName,
        header: translatedColumnName,
        Header: <Popup content={<p>{translatedColumnName}</p>} trigger={<i key={key}>{translatedColumnName}</i>} />,
        size: getColumnSize(columnName)
      };
      
      switch(columnName){
        case 'partNumber':
          return {...def, Cell: ({row}) => (<span><Clipboard text={row.original.partNumber} /> <span className="text-highlight">{row.original.partNumber}</span></span>)};
        case 'partId':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.partId.toString()} /> <span className="text-highlight">{row.original.partId}</span></span>) };
        case 'manufacturerPartNumber':
          return {...def, Cell: ({row}) => (<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>)};
        case 'description':
          return {...def, Cell: ({row}) => (<Popup hoverable content={row.original[columnName]} trigger={<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>} />)};
        case 'partType':
            return {...def, Cell: ({row}) => (
              <div onClick={e => handleSelfLink(e, row.original, columnName)}>
                <div className="icon-container small">{getIconForPart(row.original)} 
                  <div>
                    {by.includes(columnName) && byValue.includes(row.original[columnName])
                    ? <span>{row.original.partType}</span>
                    :  <Link style={{minWidth: '25px'}} to={`${visitUrl}?${createFilterBy(columnName, row.original.partType)}&keyword=${keyword}`} onClick={e => handleSelfLink(e, row.original, columnName)}>
                        {row.original.partType}
                      </Link>
                    }
                  </div>
                </div>
              </div>
            )};
        case 'binNumber':
        case 'binNumber2':
        case 'location':
          return {...def, Cell: ({row}) => (
            <div onClick={e => handleSelfLink(e, row.original, columnName)}>
              {by.includes(columnName) && byValue.includes(row.original[columnName])
              ? <span className='truncate'>{row.original[columnName]}</span>
              : <Link style={{minWidth: '25px'}} to={`${visitUrl}?${createFilterBy(columnName, row.original[columnName])}&keyword=${keyword}`} onClick={e => handleSelfLink(e, row.original, columnName)}>
                  <span className='truncate'>{row.original[columnName]}</span>
                </Link> 
              }
            </div>
          )};
        case 'cost':
          return {...def, Cell: ({row}) => (
            <>{formatCurrency(row.original.cost, row.original.currency || "USD")}</>
          )};
        case 'actions': 
          return {...def, Header: <i key={key}></i>, columnDefType: 'display', Cell: ({row}) => (
            <>
              {columnsArray.includes('datasheetUrl') && columnsVisibleArray.includes('datasheetUrl') && <Button circular size='mini' icon='file pdf outline' title='View PDF' onClick={e => handleVisitLink(e, row.original.datasheetUrl)} />}
              {columnsArray.includes('print') && columnsVisibleArray.includes('print') && <Button circular size='mini' icon='print' title='Print Label' onClick={e => handlePrintLabel(e, row.original)} />}
              {columnsArray.includes('delete') && columnsVisibleArray.includes('delete') && <Button circular size='mini' icon='delete' title='Delete part' onClick={e => confirmDeleteOpen(e, row.original)} />}
            </>
          )};
        default:
          return def;
      }
    };

    const filterColumns = ['datasheetUrl', 'print', 'delete'];
    const columnNames = _.filter(columnsArray, i => !filterColumns.includes(i));
    if ((columnsArray.includes('datasheetUrl') || columnsArray.includes('print') || columnsArray.includes('delete'))
      && (columnsVisibleArray.includes('datasheetUrl') || columnsVisibleArray.includes('print') || columnsVisibleArray.includes('delete')))
      columnNames.push('actions');
    const headers = columnNames.map((columnName, key) => getColumnDefinition(columnName, key));
    
    if (columnOrder.length === 0)
      setColumnOrder(columnsArray)
    
    return headers;
  }, [_parts, partTypes, columnsArray, columnsVisibleArray, columnOrder, navigate, t, rest.onPartClick, _selectedPart]); // end useMemo

  const handleColumnVisibilityChange = (item) => {
    let newColumnVisibility = {...columnVisibility};
    if (typeof item === 'function') {
      newColumnVisibility = { ...columnVisibility, ...item() };
    } else if (typeof item === 'object') {
      newColumnVisibility = { ...columnVisibility, ...item };
    }
    setColumnVisibility({...newColumnVisibility});
    setViewPreference('columnVisibility', newColumnVisibility);
  };

  const handleColumnOrderChange= (items) => {
    const newColumnOrder = [...items];
    setColumnOrder(newColumnOrder);
    if (newColumnOrder && newColumnOrder.length > 0) {
      setViewPreference('columnOrder', newColumnOrder);
    }
  };

  const handleSortChange = (item) => {
    let newColumnSorting = [];
    if (typeof item === "function") {
      const sortValue = item();
      if (sortValue.length > 0) {
        const sortBy = sortValue[0].id;
        const sortDirection = sortValue[0].desc ? 'Descending' : 'Ascending';
        newColumnSorting = sortValue;
        if(rest.onSortChange)
          rest.onSortChange(sortBy, sortDirection);
        setSorting(newColumnSorting);
      } else {
        setSorting([]);
        if(rest.onSortChange)
          rest.onSortChange(null, null);
      }
    }
  };

  const handleRowClick = (row) => {
    if (rest.onPartClick) rest.onPartClick(row, row.original);
  };

  const table = useMaterialReactTable({
    columns: tableColumns,
    data: _parts,
    enableRowSelection: (row) => {
      // disable selection of parts that are in the disabled list
      const canSelect = enableMultiSelect && !_disabledPartIds.includes(row.original.partId);
      return canSelect;
    },
    enableBatchRowSelection: enableMultiSelect,
    enableMultiRowSelection: enableMultiSelect,
    //rowPinningDisplayMode: "select-top",
    enableGlobalFilter: false,
    enableFilters: false,
    enablePagination: false,
    enableColumnOrdering: true,
    enableColumnResizing: true,
    enableStickyHeader: true,
    enableStickyFooter: true,
    enableDensityToggle: true,
    enableHiding: true,
    //enableEditing: _enableEditing,
    //editDisplayMode: 'table',
    manualSorting: true, // enable server side sorting
    onRowSelectionChange: setRowSelection,
    onColumnVisibilityChange: handleColumnVisibilityChange,
    onColumnOrderChange: handleColumnOrderChange,
    onSortingChange: handleSortChange,
    //onEditingCellChange={({cell, column, row, table}) => { if (onChange) onChange(cell, column, row, table); }}
    //onEditingRowSave: handleSaveColumn,
    getRowId: (row) => row.partId,
    muiTableBodyRowProps: ({ row }) => ({
      onClick: () => handleRowClick(row),
      title: _disabledPartIds.includes(row.original.partId) ? t("comp.partsGrid.alreadyInBom", 'Already in your BOM') : '',
      className: _disabledPartIds.includes(row.original.partId) ? 'disabled-highlight' : '',
      //selected: _selectedPart === row.original,
      hover: false, // important for proper row highlighting on hover
      sx: {
        cursor: 'pointer',
      }
    }),
    muiTableBodyCellProps: ({ cell, column, row, table }) => ({
      onClick: () => {
        // enable edit on single click
        //console.log('click', cell, row, table);
        //table.setEditingCell(cell);
        //table.setEditingRow(row);
        //setEnableEditing(true);
        //if (rest.onEnableEditingChange) rest.onEnableEditingChange(true);
      },
      sx: {
        className: _disabledPartIds.includes(row.original.partId) ? 'disabled' : '',
      },
    }),
    state: {
      showProgressBars: isLoading,
      columnVisibility,
      columnOrder,
      sorting,
      rowSelection
    },
    initialState: {
      density: "compact",
        columnPinning: { left: ['mrt-row-select', 'partNumber'], right: ['actions'] }
    },
    renderBottomToolbar: (<div className="footer"><Pagination activePage={_page} totalPages={_totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" /></div>)
  });

  return (
    <div id="partsGrid">
      <style>{mediaStyles}</style>
      <MediaContextProvider>
        <div className="header">
          <Pagination activePage={_page} totalPages={_totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" />
          <div>
            <span>{t("label.totalRecords", "Total records:")} {totalRecords}</span>
          </div>
          <RecordSize value={pageSize} onChange={handlePageSizeChange} />
        </div>

        <div style={{marginTop: '5px'}}>
          <MaterialReactTable table={table} />
        </div>
      </MediaContextProvider>
      <Confirm 
        className="confirm"
        header={t("confirm.header.deletePart", "Delete Part")}
        open={confirmDeleteIsOpen} 
        onCancel={confirmDeleteClose} 
        onConfirm={handleDeletePart} 
        content={confirmPartDeleteContent}
      />
      <Modal open={modalIsOpen} onCancel={handleModalClose} onClose={handleModalClose}>
        {modalHeader && <Header>{modalHeader}</Header>}
        <Modal.Content>{modalContent}</Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{"comp.partsGrid.ok"}</Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

PartsGrid2Memoized.propTypes = {
  /** Parts listing to render */
  parts: PropTypes.array.isRequired,
  /** Callback to load next page */
  loadPage: PropTypes.func.isRequired,
  /** Page number */
  page: PropTypes.number.isRequired,
  /** Total pages */
  totalPages: PropTypes.number.isRequired,
  /** Total records */
  totalRecords: PropTypes.number,
  /** True if loading data */
  loading: PropTypes.bool,
  /** List of columns to display */
  columns: PropTypes.string,
  /** Event handler when a part is clicked */
  onPartClick: PropTypes.func,
  /** Event handler when page size is changed */
  onPageSizeChange: PropTypes.func,
  /** Event handler when sort is changed */
  onSortChange: PropTypes.func,
  /** Highlight the selected part if provided */
  selectedPart: PropTypes.object,
  /** True if edit options are exposed */
  editable: PropTypes.bool,
  /** True if links are exposed */
  visitable: PropTypes.bool,
  /** The name to save localized settings as */
  settingsName: PropTypes.string,
  /** The link to use when clicking on items to filter by */
  visitUrl: PropTypes.string,
  /** Provides a function to get the default state */
  onInit: PropTypes.func,
  keyword: PropTypes.string,
  by: PropTypes.array,
  byValue: PropTypes.array,
  disabledPartIds: PropTypes.array,
  /** Enable multiple selection options */
  enableMultiSelect: PropTypes.bool,
  /** Event handler when selected parts changed */
  onSelectedPartsChange: PropTypes.func,
};
