import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from "react-router-dom";
import ReactDOM from 'react-dom';
import { useTranslation, Trans } from "react-i18next";
import _ from 'underscore';
import debounce from "lodash.debounce";
import { Icon, Form, Segment, Breadcrumb, Button, Confirm } from 'semantic-ui-react';
import { FormHeader } from "../components/FormHeader";
import ClearableInput from "../components/ClearableInput";
import PartTypeSelectorMemoized from "../components/PartTypeSelectorMemoized";
import { MountingTypes, GetAdvancedTypeDropdown } from "../common/Types";
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
  const [loadingPartTypes, setLoadingPartTypes] = useState(false);
  const [partTypes, setPartTypes] = useState([]);
  const [allPartTypes, setAllPartTypes] = useState([]);
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationApiName, setAuthorizationApiName] = useState('');
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const mountingTypeOptions = GetAdvancedTypeDropdown(MountingTypes, true);

  const fetchPartMetadata = async (part) => {
    Datasheets.abortController.abort(); // Cancel the previous request
    Datasheets.abortController = new AbortController();
    setLoading(true);
    try {
      const response = await fetchApi(`api/part/info?partNumber=${encodeURIComponent(part.partNumber.trim())}&partType=${part.partType}&mountingType=${part.mountingType}`, {
        signal: Datasheets.abortController.signal
      });
      const responseData = response.data;
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        setAuthorizationApiName(responseData.apiName);
        setAuthorizationUrl(responseData.redirectUrl);
        setConfirmAuthIsOpen(true);
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

  const searchDebounced = useMemo(() => debounce(fetchPartMetadata, DebounceTimeMs), [part]);

  const fetchPartTypes = async () => {
    setLoadingPartTypes(true);
    const response = await fetchApi("api/partType/all");
    const { data } = response;
    const partTypes = _.sortBy(
      data.map((item) => {
        return {
          key: item.partTypeId,
          value: item.partTypeId,
          text: item.name
        };
      }),
      "text"
    );
    setPartTypes(partTypes);
    setAllPartTypes(data);
    setLoadingPartTypes(false);
  };

  useEffect(() => {
    fetchPartTypes();
  }, []);

  const handlePartTypeChange = (e, partType) => { 
    const newPart = {...part, partTypeId: partType.partTypeId, partType: partType.name };
    setPart(newPart);
    if (newPart.partNumber.trim().length > 0)
      searchDebounced(newPart);
  }

  const handleChange = (e, control) => {
    part[control.name] = control.value;
    const newPart = {...part};
    if (newPart.partNumber.trim().length > 0)
      searchDebounced(newPart);
    setPart(newPart);
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
      if (partType) {
        const basePartTypeName = partType.parentPartTypeId && _.find(partTypes, (x) => x.partTypeId === partType.parentPartTypeId)?.name;
        const partTypeName = partType.name;
        if (partType) return getIcon(partTypeName, basePartTypeName)({className: `parttype-${basePartTypeName || partTypeName}`});
      }
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

  const handleAuthRedirect = (e) => {
    e.preventDefault();
    window.location.href = authorizationUrl;
  };

  const renderParts = (parts, column, direction) => {
    const partsWithDatasheets = _.filter(parts, function (x) { return x.datasheetUrls.length > 0 && _.first(x.datasheetUrls).length > 0; });
    return (
      <div id="datasheets">
        <Form>
          <Form.Group>
            <ClearableInput width={5} label={t('label.part', "Part")} required placeholder='LM358' icon='search' focus value={part.partNumber} onChange={handleChange} name='partNumber' />
            <Form.Field width={6}>
              <PartTypeSelectorMemoized 
                label={t('label.partType', "Part Type")}
                name="partTypeId"
                value={part.partTypeId || ""}
                partTypes={allPartTypes} 
                onSelect={handlePartTypeChange}
                loadingPartTypes={loadingPartTypes}
              />
            </Form.Field>
            <Form.Dropdown
              width={4}
              label={t('label.mountingType', "Mounting Type")}
              placeholder={t('label.mountingType', "Mounting Type")}
              search
              selection
              value={(part.mountingType || "")}
              options={mountingTypeOptions}
              onChange={handleChange}
              name="mountingType"
            />
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
      <Confirm
        className="confirm"
        header={t('page.settings.confirm.mustAuthenticateHeader', "Must Authenticate")}
        open={confirmAuthIsOpen}
        onCancel={() => setConfirmAuthIsOpen(false)}
        onConfirm={handleAuthRedirect}
        content={<p>
          <Trans i18nKey="page.settings.confirm.mustAuthenticate" name={authorizationApiName}>
            External Api (<b>{{ name: authorizationApiName }}</b>) is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.
          </Trans>
        </p>
        }
      />
      {contents}
    </div>
  );
}
