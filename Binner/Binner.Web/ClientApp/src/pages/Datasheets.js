import React, { useState, useMemo } from 'react';
import { useNavigate } from "react-router-dom";
import ReactDOM from 'react-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import debounce from "lodash.debounce";
import { Form, Segment, Breadcrumb, Button } from 'semantic-ui-react';
import { FormHeader } from "../components/FormHeader";
import ClearableInput from "../components/ClearableInput";
import { getIcon } from "../common/partTypes";
import { fetchApi } from '../common/fetchApi';
import { getLocalData, setLocalData } from "../common/storage";
import { Clipboard } from "../components/Clipboard";
import MaterialReactTable from "material-react-table";
import "./Datasheets.css";

export function Datasheets (props) {
  const DebounceTimeMs = 400;
  const { t } = useTranslation();
  const navigate = useNavigate();
  Datasheets.abortController = new AbortController();

  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'datasheets' })
  };

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'datasheets' });
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

  const defaultColumns = "manufacturerPartNumber,manufacturer,partType,website,datasheetUrls,packageType,status";
  const [columnsArray, setColumnsArray] = useState(defaultColumns.split(','));
  const [columnVisibility, setColumnVisibility] = useState(getViewPreference('columnVisibility') || createDefaultVisibleColumns(defaultColumns, defaultColumns));
  const [columnsVisibleArray, setColumnsVisibleArray] = useState(defaultColumns.split(','));
  const [columnOrder, setColumnOrder] = useState(getViewPreference('columnOrder') || []);
  const [parts, setParts] = useState([]);
  const [part, setPart] = useState({ partNumber: '', partType: '', mountingType: '' });
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [loading, setLoading] = useState(false);
  const [partTypes, setPartTypes] = useState([]);
  const [mountingTypes] = useState([
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
  ]);

  const fetchPartMetadata = async (input) => {
    Datasheets.abortController.abort(); // Cancel the previous request
    Datasheets.abortController = new AbortController();
    setLoading(true);
    try {
      const response = await fetchApi(`api/part/info?partNumber=${input}&partType=${part.partType}&mountingType=${part.mountingType}`, {
        signal: Datasheets.abortController.signal
      });
      const responseData = response.data;
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }
      const { response: data } = responseData;
      setParts(data.parts);
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
    setLoading(false);
  };

  const searchDebounced = useMemo(() => debounce(fetchPartMetadata, DebounceTimeMs), []);

  const fetchPartTypes = async () => {
    setLoading(true);
    const response = await fetchApi('api/partType/list');
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
    setPartTypes(partTypes);
    setLoading(false);
  };

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

  const handleChange = (e, control) => {
    part[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (control.value && control.value.length > 0)
          searchDebounced(control.value);
        break;
      case 'partType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          searchDebounced(control.value);
        break;
      case 'mountingType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          searchDebounced(control.value);
        break;
      default:
        break;
    }
    setPart({...part});
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  };

  const handleHighlightAndVisit = (e, url) => {
    handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains('positive')) row.classList.remove('positive');
    }
    targetNode.classList.toggle('positive');
  };

  const getHostnameFromRegex = (url) => {
    // run against regex
    const matches = url.match(/^https?:\/\/([^/?#]+)(?:[/?#]|$)/i);
    // extract hostname (will be null if no match is found)
    return matches && matches[1];
  };

  const getColumnSize = (columnName) => {
    switch (columnName) {
      case "manufacturerPartNumber":
        return 200;
      case "partType":
        return 150;
      case "datasheet":
        return 200;
      case "package":
        return 150;
      case "status":
        return 175;
      case "actions":
        return 140;
      default:
        return 180;
    }
  };  

  const tableColumns = useMemo(() => {
    const handleSelfLink = (e, part, propertyName) => {
      e.preventDefault();
      e.stopPropagation();
      if (part[propertyName]) {
        const url = `${props.visitUrl}?by=${propertyName}&value=${part[propertyName]}`;
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
      const translatedColumnName = t(`page.datasheet.${columnName}`, `i18 ${columnName}`);
  
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
        case 'partType':
          return {...def, Cell: ({row}) => (
            <div onClick={e => handleSelfLink(e, row.original, columnName)}>
              <div className="icon-container small">{getIconForPart(row.original)} 
                <div>
                  {row.original.partType}
                </div>
              </div>
            </div>
          )};
        case 'website':
          return {...def, Cell: ({row}) => (<span>{row.original.swarmPartNumberManufacturerId ? "Binner Swarm" : getHostnameFromRegex(_.first(row.original.datasheetUrls))}</span>)};
        case 'status':
          return {...def, Cell: ({row}) => (<span>{row.original.status === "Inactive" ? <span style={{color: '#bbb'}}>{t(row.original.status.toLowerCase(), row.original.status)}</span> : <b>{t(row.original.status.toLowerCase(), row.original.status)}</b>}</span>)};
        case 'actions': 
          return {...def, Header: <i key={key}>{t('page.datasheet.datasheets', "Datasheets")}</i>, columnDefType: 'display', Cell: ({row}) => (
            <>
              {columnsArray.includes('datasheetUrls') && columnsVisibleArray.includes('datasheetUrls') 
                && row.original.datasheetUrls && row.original.datasheetUrls.map((url, dindex) =>
                  <Button key={dindex} circular size='mini' icon='file pdf outline' title='View PDF' onClick={e => handleHighlightAndVisit(e, url)} />
                )}
            </>
          )};
        default:
          return def;
      }
    };

    const filterColumns = ['datasheetUrls'];
    const columnNames = _.filter(columnsArray, i => !filterColumns.includes(i));
    columnNames.push('actions');
    const headers = columnNames.map((columnName, key) => getColumnDefinition(columnName, key));

    if (columnOrder.length === 0)
      setColumnOrder(columnsArray)
       
    return headers;
  }, [/*columns, */partTypes, columnsArray, columnsVisibleArray, columnOrder, navigate, t]);

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

  const renderParts = (parts, column, direction) => {
    const partsWithDatasheets = _.filter(parts, function (x) { return x.datasheetUrls.length > 0 && _.first(x.datasheetUrls).length > 0; });
    return (
      <div id="datasheets">
        <Form>
          <Form.Group>
            <ClearableInput label={t('label.part', "Part")} required placeholder='LM358' icon='search' focus value={part.partNumber} onChange={handleChange} name='partNumber' />
            <Form.Dropdown label={t('label.partType', "Part Type")} placeholder='Part Type' search selection value={part.partType} options={partTypes} onChange={handleChange} name='partType' />
            <Form.Dropdown label={t('label.mountingType', "Mounting Type")} placeholder='Mounting Type' search selection value={part.mountingType} options={mountingTypes} onChange={handleChange} name='mountingType' />
          </Form.Group>
        </Form>
        <Segment loading={loading}>
          <MaterialReactTable
              columns={tableColumns}
              data={partsWithDatasheets}
              //enableRowSelection /** disabled until I can figure out how to pin it */
              enableGlobalFilter={false}
              enableColumnOrdering
              enableColumnResizing
              enablePinning
              enableStickyHeader
              enableStickyFooter
              enableDensityToggle
              enableHiding
              onColumnVisibilityChange={handleColumnVisibilityChange}
              onColumnOrderChange={handleColumnOrderChange}
              state={{ 
                columnVisibility, 
                columnOrder
              }}
              initialState={{ 
                density: "compact", 
                columnPinning: { left: ['manufacturerPartNumber'], right: ['actions'] },
                pagination: { pageSize: 25 }
              }}
              labelRowsPerPage={t('label.rowsPerPage', "Rows per page")}
            />
        </Segment>
      </div>
    );
  };

  const contents = renderParts(parts, column, direction);

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.datasheet.title', "Datasheet Search")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.datasheet.title', "Datasheet Search")} to="/">
      {t('page.datasheet.description', "Search for datasheets by part number.")}
			</FormHeader>
      {contents}
    </div>
  );
}
