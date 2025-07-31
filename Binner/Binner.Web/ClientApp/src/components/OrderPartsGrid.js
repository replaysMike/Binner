import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { createMedia } from "@artsy/fresnel";
import { Button, Checkbox, Dropdown, Pagination, Popup, Image, Icon } from "semantic-ui-react";
import { Clipboard } from "./Clipboard";
import MaterialReactTable from "material-react-table";
import _ from "underscore";
import { Link } from "react-router-dom";
import PropTypes from "prop-types";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData } from "../common/storage";
import { formatCurrency } from "../common/Utils";
import { getIcon } from "../common/partTypes";
import "./PartsGrid2.css";
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

export default function OrderPartsGrid({
    loading = true,
    columns = "partId,imageUrl,description,reference,quantity,cost,totalCost,currency,manufacturerPartNumber,manufacturer,supplierPartNumber,partType,packageType,mountingType,datasheetUrl,productUrl,selected",
    defaultVisibleColumns = "partId,imageUrl,description,reference,quantity,cost,totalCost,currency,manufacturerPartNumber,manufacturer,supplierPartNumber,partType,datasheetUrl,productUrl,selected",
    page = 1,
    totalPages = 1,
    totalRecords = 0,
    parts = [],
    ...rest
  }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'orderPartsGridViewPreferences', location })
  };

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'orderPartsGridViewPreferences', location });
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
  const [_columns, setColumns] = useState(columns);
  const [columnsArray, setColumnsArray] = useState(columns.split(','));
  const [partTypes, setPartTypes] = useState();
  const [columnVisibility, setColumnVisibility] = useState(getViewPreference('columnVisibility') || createDefaultVisibleColumns(columns, defaultVisibleColumns));
  const [columnsVisibleArray, setColumnsVisibleArray] = useState(defaultVisibleColumns.split(','));
  const [columnOrder, setColumnOrder] = useState(getViewPreference('columnOrder') || []);
  const [sorting, setSorting] = useState([]);
  
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
    updatePageSize(null, pageSize);
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
    setTotalPages(totalPages);
  }, [parts, loading, columns, defaultVisibleColumns, page, totalPages]);

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
    if (rest.loadPage) rest.loadPage(e, control.activePage);
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    if (window?.open) window.open(url, "_blank");
  };

  const handleViewPart = (e, part) => {
    if (part.partId && part.manufacturerPartNumber) {
      if (window?.open) window.open(`/inventory/${part.manufacturerPartNumber}:${part.partId}`, "_blank");
    }
  };

  const handleChecked = (e, p) => {
    if (rest.onCheckedChange) rest.onCheckedChange(e, p);
  };

  const handlePageSizeChange = (e, control) => {
    updatePageSize(e, control.value);
    setPage(1);
  };

  const updatePageSize = (e, newPageSize) => {
    setPageSize(newPageSize);
    setViewPreference('pageSize', newPageSize);
    if (rest.onPageSizeChange) rest.onPageSizeChange(e, newPageSize);
  };

  const getColumnSize = (columnName) => {
    switch (columnName) {
      case "description":
        return 220;
      case "reference":
        return 180;
      case "quantity":
        return 140;
      case "manufacturerPartNumber":
        return 250;
      case "partType":
        return 150;
      case "mountingType":
        return 150;
      case "cost":
        return 100;
      case "totalCost":
        return 100;
      case "currency":
        return 100;
      case "supplierPartNumber":
        return 220;
      case "imageUrl":
        return 40;
      case "partId":
        return 40;
      case "actions":
        return 180;
      default:
        return 180;
    }
  };

  const handleSelectAll = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    if (rest.onSelectAll) rest.onSelectAll(e, control);
  };

  const handleSelectNone = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    if (rest.onSelectNone) rest.onSelectNone(e, control);
  };

  const tableColumns = useMemo(() => {

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
        Header: <i key={key}>{translatedColumnName}</i>,
        size: getColumnSize(columnName)
      };
      
      switch(columnName){
        case 'description':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.description} /> <span>{row.original.description}</span></span>)};
        case 'reference':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.reference} /> <span className="text-highlight">{row.original.reference}</span></span>) };
        case 'manufacturerPartNumber':
          return {...def, Cell: ({row}) => (<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>)};
        case 'manufacturer':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.manufacturer} /> <span>{row.original.manufacturer}</span></span>) };
        case 'supplierPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original[columnName]} /> {row.original[columnName]}</span>) };
        case 'partType':
            return {...def, Cell: ({row}) => (
              <div onClick={e => handleSelfLink(e, row.original, columnName)}>
                <div className="icon-container small">{getIconForPart(row.original)} 
                  <div>
                    <span>{row.original.partType}</span>
                  </div>
                </div>
              </div>
            )};
        case 'cost':
          return {...def, Cell: ({row}) => (
            <>{formatCurrency(row.original.cost, row.original.currency || "USD")}</>
          )};
        case 'totalCost':
          return {
            ...def, Cell: ({ row }) => (
              <>{formatCurrency(row.original.totalCost, row.original.currency || "USD")}</>
            )
          };
        case 'currency':
          return { ...def, Cell: ({ row }) => (<span>{row.original[columnName]}</span>) };
        case 'mountingType':
          return { ...def, Cell: ({ row }) => (<span>{row.original[columnName]}</span>) };
        case 'imageUrl':
          return {
            ...def, Header: <i key={key}></i>, columnDefType: 'display', Cell: ({ row }) => (
              <Popup
                content={<p><Image src={row.original.imageUrl} size="small"></Image></p>}
                trigger={columnsArray.includes('imageUrl') && columnsVisibleArray.includes('imageUrl') && row.original.imageUrl && <Image src={row.original.imageUrl} circular inline size="mini" style={{ float: 'right' }} />}
              />
            )
          };
        case 'partId':
          return {
            ...def, Header: <i key={key}></i>, columnDefType: 'display', Cell: ({ row }) => (row.original.partId 
            ? <Popup content={<p>{t('page.orderImport.popup.partExists', "Part exists in your inventory. If selected, quantity will be added to it.")}</p>} trigger={<Icon name="check circle" color="blue" size="large" />} />
              : <Popup content={<p>{t('page.orderImport.popup.newPart', "New part does not exist in your inventory.")}</p>} trigger={<Icon name="plus circle" color="green" size="large" />} />) };
        case 'actions': 
          return {
            ...def, Header: <i key={key}>Select <Link onClick={handleSelectAll}>all</Link> <Link onClick={handleSelectNone}>none</Link></i>, columnDefType: 'display', Cell: ({row}) => (
            <>
              {columnsArray.includes('partId') && columnsVisibleArray.includes('partId') && row.original.partId > 0 && 
                <Popup
                content={<p>{t('button.viewPart', "View Part")}</p>}
                  trigger={<Button circular size='mini' icon='microchip' onClick={e => handleViewPart(e, row.original)} />}
                />}
              
              {columnsArray.includes('datasheetUrl') && columnsVisibleArray.includes('datasheetUrl') && row.original.datasheetUrls?.length > 0 && 
                <Popup
                  content={<p>{t('button.viewDatasheet', "View Datasheet")}</p>}
                  trigger={<Button circular size='mini' icon='file pdf outline' onClick={e => handleVisitLink(e, row.original.datasheetUrls[0])} />}
                />}
              {columnsArray.includes('productUrl') && columnsVisibleArray.includes('productUrl') && row.original.productUrl &&
                <Popup
                  content={<p>{t('button.viewProduct', "View Product")}</p>}
                trigger={<Button circular size='mini' icon='file outline' onClick={e => handleVisitLink(e, row.original.productUrl)} />}
                />}
              <div style={{ float: 'right', marginTop: '4px' }}>
                {columnsArray.includes('selected') && columnsVisibleArray.includes('selected') && 
                <Popup 
                  content={<p>{t('label.importQuestion', "Import?")}</p>}
                  trigger={<Checkbox toggle checked={row.original.selected} onChange={(e) => handleChecked(e, row.original)} data={row.original} />}
                  />}
              </div>
            </>
          )};
        default:
          return def;
      }
    };

    const filterColumns = ['productUrl', 'datasheetUrl', 'selected'];
    const columnNames = _.filter(columnsArray, i => !filterColumns.includes(i));
    if ((columnsArray.includes('datasheetUrl') || columnsArray.includes('productUrl'))
      && (columnsVisibleArray.includes('datasheetUrl') || columnsVisibleArray.includes('productUrl') || columnsVisibleArray.includes('selected')))
      columnNames.push('actions');
    const headers = columnNames.map((columnName, key) => getColumnDefinition(columnName, key));
    
    if (columnOrder.length === 0)
      setColumnOrder(columnsArray)
    
    return headers;
  }, [_parts, partTypes, columnsArray, columnsVisibleArray, columnOrder, navigate, t, rest.onPartClick]);

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
          <MaterialReactTable
            columns={tableColumns}
            data={_parts}
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
            onColumnVisibilityChange={handleColumnVisibilityChange}
            onColumnOrderChange={handleColumnOrderChange}
            onSortingChange={handleSortChange}
            state={{ 
              showProgressBars: isLoading,
              columnVisibility, 
              columnOrder,
              sorting
            }}
            initialState={{ 
              density: "compact", 
              columnPinning: { left: ['actions'] }
            }}
            renderBottomToolbar={(<div className="footer"><Pagination activePage={_page} totalPages={_totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" /></div>)}
            muiTableBodyRowProps={({row}) => ({
              onClick: () => handleRowClick(row),
              hover: false, // important for proper row highlighting on hover
              sx: {
                cursor: 'pointer'
              }
            })}
          />
        </div>
      </MediaContextProvider>
    </div>
  );
}

OrderPartsGrid.propTypes = {
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
  /** The name to save localized settings as */
  settingsName: PropTypes.string,
  /** Provides a function to get the default state */
  onInit: PropTypes.func,
  onCheckedChange: PropTypes.func,
  onSelectAll: PropTypes.func,
  onSelectNone: PropTypes.func,
};
