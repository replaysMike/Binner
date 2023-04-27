import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { createMedia } from "@artsy/fresnel";
import { Button, Confirm, Modal, Header, Dropdown, Pagination, Popup } from "semantic-ui-react";
import { Clipboard } from "./Clipboard";
import MaterialReactTable from "material-react-table";
import _ from "underscore";
import { Link } from "react-router-dom";
import PropTypes from "prop-types";
import { fetchApi } from "../common/fetchApi";
import { AppEvents, Events } from "../common/events";
import { formatCurrency } from "../common/Utils";
import { getIcon } from "../common/partTypes";
import "./PartsGrid2.css";

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

export default function PartsGrid2(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const getViewPreference = (preferenceName) => {
    const preferences = JSON.parse(localStorage.getItem(`partsGridViewPreferences`));
    if (preferences) {
      const prefLocation = props.settingsName || location.pathname?.toLowerCase().replaceAll('/', '') || 'root';
      const prefLocationSettings = preferences[prefLocation];
      if (prefLocationSettings){
        const val = prefLocationSettings[preferenceName];
        return val;
      }
    }
    return null;
  };

  const setViewPreference = (preferenceName, value) => {
    /**
     {
      location: {
        // preferences
      }
     }
     */
    const currentViewPreferences = JSON.parse(localStorage.getItem(`partsGridViewPreferences`))
    const prefLocation = props.settingsName || location.pathname?.toLowerCase().replaceAll('/', '') || 'root';
    let newViewPreferences = {...currentViewPreferences };
    newViewPreferences[prefLocation] = {...newViewPreferences[prefLocation], [preferenceName]: value};
    localStorage.setItem('partsGridViewPreferences', JSON.stringify(newViewPreferences));
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

  const [parts, setParts] = useState(props.parts);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [totalPages, setTotalPages] = useState(props.totalPages);
  const [isLoading, setIsLoading] = useState(props.loading);
  const [selectedPart, setSelectedPart] = useState(props.selectedPart);
  const [editable, setEditable] = useState(props.editable);
  const [visitable, setVisitable] = useState(props.visitable);
  const [column, setColumn] = useState(null);
  const [columns, setColumns] = useState(props.columns);
  const [columnsArray, setColumnsArray] = useState(props.columns.split(','));
  const [direction, setDirection] = useState(null);
  const [changeTracker, setChangeTracker] = useState([]);
  const [lastSavedPartId, setLastSavedPartId] = useState(0);
  const [saveMessage, setSaveMessage] = useState("");
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [modalHeader, setModalHeader] = useState("");
  const [modalContent, setModalContent] = useState("");
  const [modalIsOpen, setModalIsOpen] = useState(false);
  const [partTypes, setPartTypes] = useState();
  const [columnVisibility, setColumnVisibility] = useState(getViewPreference('columnVisibility') || createDefaultVisibleColumns(props.columns, props.defaultVisibleColumns));
  const [columnsVisibleArray, setColumnsVisibleArray] = useState(props.defaultVisibleColumns.split(','));
  const [columnOrder, setColumnOrder] = useState(getViewPreference('columnOrder') || []);
  const [sorting, setSorting] = useState([]);
  const loadPage = props.loadPage;
  const onPartClick = props.onPartClick;
  const itemsPerPageOptions = [
    { key: 1, text: "25", value: 25 },
    { key: 2, text: "50", value: 50 },
    { key: 3, text: "100", value: 100 },
    { key: 4, text: "200", value: 200 }
  ];

  const loadPartTypes = useCallback((parentPartType = "") => {
    setIsLoading(true);
    if (parentPartType === undefined || parentPartType === null) parentPartType = "";
    fetchApi(`api/partType/all?parent=${parentPartType}`).then((response) => {
      const { data } = response;

      setPartTypes(data);
      setIsLoading(false);
    });
  }, []);

  useEffect(() => {
    loadPartTypes();
  }, [loadPartTypes]);

  useEffect(() => {
    setParts(props.parts);
    setIsLoading(props.loading);
    setColumns(props.columns);
    setColumnsArray(props.columns.split(','));
    setColumnsVisibleArray(props.defaultVisibleColumns.split(','));
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
      setDirection("ascending");
    } else {
      setParts(parts.reverse());
      setDirection(direction === "ascending" ? "descending" : "ascending");
    }
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
    loadPage(e, control.activePage);
  };

  const save = async (part) => {
    setIsLoading(true);
    let lastSavedPartId = 0;
    part.partTypeId = part.partTypeId + "";
    part.mountingTypeId = part.mountingTypeId + "";
    part.quantity = Number.parseInt(part.quantity) || 0;
    part.lowStockThreshold = Number.parseInt(part.lowStockThreshold) || 0;
    part.cost = Number.parseFloat(part.cost) || 0.0;
    part.projectId = Number.parseInt(part.projectId) || null;

    const response = await fetchApi("api/part", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(part)
    });
    let saveMessage = "";
    if (response.responseObject.status === 200) {
      lastSavedPartId = part.partId;
      saveMessage = `Saved part ${part.partNumber}!`;
    } else {
      console.error("failed to save part", response.data);
      saveMessage = t("comp.partsGrid.error.failedSave", "Error saving part {{partNumber}} - {{statusText}}", { partNumber: part.partNumber, statusText: response.statusText });
      displayModalContent(saveMessage, "Error");
    }
    setIsLoading(false);
    setLastSavedPartId(lastSavedPartId);
    setSaveMessage(saveMessage);
  };

  const saveColumn = async (e) => {
    changeTracker.forEach(async (val) => {
      const part = _.find(parts, { partId: val.partId });
      if (part) await save(part);
    });
    setParts(parts);
    setChangeTracker([]);
  };

  const handleChange = (e, control) => {
    const part = _.find(parts, { partId: control.data });
    let changes = [...changeTracker];
    if (part) {
      part[control.name] = control.value;
      if (_.where(changes, { partId: part.partId }).length === 0) changes.push({ partId: part.partId });
    }
    setParts([...parts]);
    setChangeTracker(changes);
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const handlePrintLabel = async (e, part) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`api/part/print?partNumber=${part.partNumber}`, { method: "POST" });
  };

  const handleDeletePart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`api/part`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
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
    if (props.onPageSizeChange) props.onPageSizeChange(e, control.value);
  };

  const getColumnSize = (columnName) => {
    switch (columnName) {
      case "partNumber":
        return 180;
      case "quantity":
        return 140;
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
      case "actions":
        return 150;
      default:
        return 180;
    }
  };  

  const tableColumns = useMemo(() => {
    
    const handleLoadPartClick = (e, part) => {
      if (onPartClick) onPartClick(e, part);
    };

    const handleSelfLink = (e, part, propertyName) => {
      e.preventDefault();
      e.stopPropagation();
      if (part[propertyName]) {
        const url = `/inventory?by=${propertyName}&value=${part[propertyName]}`;
        navigate(url);
      }
    };

    const getIconForPart = (p) => {
      const partType = _.find(partTypes, (x) => x.partTypeId === p.partTypeId);
      const basePartTypeName = partType.parentPartTypeId && _.find(partTypes, (x) => x.partTypeId === partType.parentPartTypeId)?.name;
      const partTypeName = partType.name;
      if (partType) return getIcon(partTypeName, basePartTypeName)({className: `parttype-${basePartTypeName || partTypeName}`});
      return "";
    };

    const getColumnDefinition = (columnName, key) => {
      const translatedColumnName = t(`comp.partsGrid.${columnName}`, `i18 ${columnName}`);
  
      const def = {
        accessorKey: columnName,
        header: translatedColumnName,
        Header: <i key={key}>{translatedColumnName}</i>,
        size: getColumnSize(columnName)
      };
      
      switch(columnName){
        case 'partNumber':
          return {...def, Cell: ({row}) => (<span><Clipboard text={row.original.partNumber} /> <span className="text-highlight">{row.original.partNumber}</span></span>)};
        case 'manufacturerPartNumber':
          return {...def, Cell: ({row}) => (<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>)};
        case 'description':
          return {...def, Cell: ({row}) => (<Popup hoverable content={row.original[columnName]} trigger={<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>} />)};
        case 'partType':
            return {...def, Cell: ({row}) => (
              <div onClick={e => handleSelfLink(e, row.original, columnName)}>
                <div className="icon-container small">{getIconForPart(row.original)} 
                  <div>
                    <Link to={`/inventory?by=partType&value=${row.original.partType}`} onClick={e => handleSelfLink(e, row.original, columnName)}>
                      {row.original.partType}
                    </Link>
                  </div>
                </div>
              </div>
            )};
        case 'binNumber':
        case 'binNumber2':
        case 'location':
          return {...def, Cell: ({row}) => (
            <div onClick={e => handleSelfLink(e, row.original, columnName)}>
              <Link to={`/inventory?by=${columnName}&value=${row.original[columnName]}`} onClick={e => handleSelfLink(e, row.original, columnName)}>
                <span className='truncate'>{row.original[columnName]}</span>
              </Link>
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
  }, [/*columns, */partTypes, columnsArray, columnsVisibleArray, columnOrder, navigate, t, onPartClick]);

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
        if(props.onSortChange)
          props.onSortChange(sortBy, sortDirection);
        setSorting(newColumnSorting);
      } else {
        setSorting([]);
        if(props.onSortChange)
          props.onSortChange(null, null);
      }
    }
  };

  const handleRowClick = (row) => {
    if (onPartClick) onPartClick(row, row.original);
  };

  return (
    <div>
      <style>{mediaStyles}</style>
      <MediaContextProvider>
        <div style={{ float: "right", verticalAlign: "middle", fontSize: "0.9em" }}>
          <Dropdown selection options={itemsPerPageOptions} value={pageSize} className="small labeled" onChange={handlePageSizeChange} />
          <span>{t("comp.partsGrid.recordsPerPage", "records per page")}</span>
        </div>
        <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" />

        <div id="partsGrid" style={{marginTop: '5px'}}>
          {<MaterialReactTable
            columns={tableColumns}
            data={parts}
            //enableRowSelection /** disabled until I can figure out how to pin it */
            enableGlobalFilter={false}
            enableFilters={false}
            enablePagination={false}
            enableColumnOrdering
            enableColumnResizing
            enablePinning
            enableStickyHeader
            enableStickyFooter
            enableDensityToggle
            enableHiding
            manualSorting
            state={{ 
              columnVisibility, 
              columnOrder,
              sorting
            }}
            onColumnVisibilityChange={handleColumnVisibilityChange}
            onColumnOrderChange={handleColumnOrderChange}
            onSortingChange={handleSortChange}
            initialState={{ 
              density: "compact", 
              columnPinning: { left: ['partNumber'], right: ['actions'] }
            }}
            renderBottomToolbar={(<div className="footer"><Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" /></div>)}
            muiTableBodyRowProps={({row}) => ({
              onClick: () => handleRowClick(row),
              selected: selectedPart === row.original,
              hover: false, // important for proper row highlighting on hover
              sx: {
                cursor: 'pointer'
              }
            })}
          />}
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

PartsGrid2.propTypes = {
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
  /** Event handler when sort is changed */
  onSortChange: PropTypes.func,
  /** Highlight the selected part if provided */
  selectedPart: PropTypes.object,
  /** True if edit options are exposed */
  editable: PropTypes.bool,
  /** True if links are exposed */
  visitable: PropTypes.bool,
  /** The name to save localized settings as */
  settingsName: PropTypes.string
};

PartsGrid2.defaultProps = {
  loading: true,
  columns: "partNumber,quantity,lowStockThreshold,manufacturerPartNumber,description,partType,packageType,mountingType,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,arrowPartNumber,datasheetUrl,print,delete",
  defaultVisibleColumns: "partNumber,quantity,manufacturerPartNumber,description,partType,location,binNumber,binNumber2,cost,datasheetUrl,print,delete",
  page: 1,
  totalPages: 1,
  editable: true,
  visitable: true
};
