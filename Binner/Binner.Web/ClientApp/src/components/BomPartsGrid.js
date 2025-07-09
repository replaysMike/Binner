import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { createMedia } from "@artsy/fresnel";
import { Button, Checkbox, Dropdown, Pagination, Popup, Image, Icon, Form, TextArea } from "semantic-ui-react";
import { Clipboard } from "./Clipboard";
import MaterialReactTable from "material-react-table";
import _ from "underscore";
import { Link } from "react-router-dom";
import PropTypes from "prop-types";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData } from "../common/storage";
import { formatCurrency, getCurrencySymbol } from "../common/Utils";
import { getIcon } from "../common/partTypes";
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

export default function BomPartsGrid({
    loading = true,
  columns = "pcb,partName,manufacturerPartNumber,partType,cost,totalCost,quantity,quantityAvailable,leadTime,referenceId,schematicReferenceId,description,customDescription,notes,imageUrl,reference,currency,manufacturer,supplierPartNumber,packageType,mountingType,datasheetUrl,productUrl,selected,location,binNumber,binNumber2,extensionValue1,extensionValue2,digiKeyPartNumber,mouserPartNumber,arrowPartNumber,tmePartNumber,footprintName,symbolName,keywords,customFields",
  defaultVisibleColumns = "pcb,partName,manufacturerPartNumber,partType,cost,totalCost,quantity,quantityAvailable,leadTime,referenceId,schematicReferenceId,description,customDescription,notes,datasheetUrl,productUrl,selected,location,binNumber,binNumber2",
    page = 1,
    totalPages = 1,
    totalRecords = 0,
    project = null,
    parts = [],
    ...rest
  }) {
  const PopupDelayMs = 500;
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
  const itemsPerPageOptions = [
    { key: 1, text: "10", value: 10 },
    { key: 2, text: "25", value: 25 },
    { key: 3, text: "50", value: 50 },
    { key: 4, text: "100", value: 100 },
    { key: 5, text: "200", value: 200 },
    { key: 6, text: "500", value: 500 },
  ];

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

  const handleChecked = (e, part) => {
    part.selected = !part.selected;
    if (rest.onCheckedChange) rest.onCheckedChange(e, part);
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
      case "pcb":
        return 140;
      case "partName":
        return 220;
      case "description":
        return 220;
      case "customDescription":
        return 220;
      case "notes":
        return 220;
      case "schematicReferenceId":
        return 220;
      case "reference":
        return 190;
      case "referenceId":
        return 220;
      case "quantity":
        return 150;
      case "quantityAvailable":
        return 150;
      case "manufacturerPartNumber":
        return 250;
      case "partType":
        return 150;
      case "mountingType":
        return 150;
      case "cost":
        return 110;
      case "totalCost":
        return 160;
      case "currency":
        return 100;
      case "supplierPartNumber":
        return 220;
      case "imageUrl":
        return 40;
      case "partId":
        return 40;
      case "location":
        return 180;
      case "binNumber":
        return 180;
      case "binNumber2":
        return 180;
      case "footprintName":
        return 280;
      case "symbolName":
        return 280;
      case "customFields":
        return 280;
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

  const handleSaveColumn = async (e, control, part) => {
    e.preventDefault();
    e.stopPropagation();
    if (rest.onSaveInlineChange) rest.onSaveInlineChange(e, control, part);
  };

  const handlePartsInlineChange = (e, control, part) => {
    e.preventDefault();
    e.stopPropagation();
    if (rest.onRowEditChange) rest.onRowEditChange(e, control, part);
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
        Header: <span key={key}>{translatedColumnName}</span>,
        size: getColumnSize(columnName)
      };
      
      switch(columnName){
        case 'pcb':
          return { ...def, 
            Header: <Popup
                      key={key}
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.pcb">
                            Indicates which PCB your part is assigned to. A PCB assignment is optional, all unassigned parts will appear in the <b>All</b> tab.
                          </Trans>
                        </p>
                      }
                      trigger={<span>{t("label.pcb", "PCB")}</span>}
                    />,
            Cell: ({ row }) => (<span>{_.find(project?.pcbs, (x) => x.pcbId === row.original.pcbId)?.name}</span>) };
        case 'partName':
          return { ...def, 
            Header: <span key={key}>{t("label.partNumber", "Part Number")}</span>,
            Cell: ({ row }) => (<div><Clipboard text={row.original.partName} /> 
            {row.original.part 
                ? (<Link to={`/inventory/${encodeURIComponent(row.original.part.partNumber)}`}>{row.original.part.partNumber}</Link>) 
                : (<Popup
                  wide
                  mouseEnterDelay={PopupDelayMs}
                  content={
                    <p>
                      <Trans i18nKey="popup.bom.unassociatedPartName">
                        Edit the name of your unassociated <Icon name="microchip" /> part
                      </Trans>
                    </p>
                  }
                  trigger={
                    <Form.Input
                      type="text"
                      transparent
                      name="partName"
                      onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                      onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                      value={row.original.partName || 0}
                      className="inline-editable"
                    />
                  }
                />)}
            </div>)};
        case 'description':
          return { ...def, Cell: ({ row }) => (<div><Clipboard text={row.original.part?.description} /> 
          <Popup 
            wide
            mouseEnterDelay={PopupDelayMs}
            hoverable 
            content={<p>{row.original.part?.description}</p>} 
            trigger={<span>{row.original.part?.description}</span>} 
          />
          </div>) };
        case 'customDescription':
          return { ...def, Cell: ({ row }) => (<div>
            <Popup
              wide
              mouseEnterDelay={PopupDelayMs}
              content={<p>{t("popup.bom.customDescription", "Provide a custom description")}</p>}
              trigger={
                <Form.Field
                  type="text"
                  control={TextArea}
                  name="customDescription"
                  onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                  onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                  value={row.original.customDescription || ""}
                  className="transparent inline-editable"
                />}/></div>) };
        case 'notes':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Popup
                wide
                mouseEnterDelay={PopupDelayMs}
                content={<p>{t("popup.bom.customNote", "Provide a custom note")}</p>}
                trigger={
                  <Form.Field
                    type="text"
                    control={TextArea}
                    name="notes"
                    onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                    onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                    value={row.original.notes || ""}
                    className="transparent inline-editable"
                  />}
                />
              </div>)
          };
        case 'manufacturerPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>)};
        case 'manufacturer':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part.manufacturer} /> {row.original.part.manufacturer}</span>) };
        case 'supplierPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>) };
        case 'digiKeyPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>) };
        case 'mouserPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>) };
        case 'arrowPartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>) };
        case 'tmePartNumber':
          return { ...def, Cell: ({ row }) => (<span><Clipboard text={row.original.part[columnName]} /> {row.original.part[columnName]}</span>) };
        case 'leadTime':
          return { ...def, Cell: ({ row }) => (<span>{row.original.part[columnName]}</span>) };
        case 'location':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Form.Input
                type="text"
                transparent
                name="location"
                onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                onChange={(e, control) => handlePartsInlineChange(e, control, row.original.part)}
                value={row.original.part.location || ""}
                className="inline-editable"
              />
            </div>) };
        case 'binNumber':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Form.Input
                type="text"
                transparent
                name="binNumber"
                onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                onChange={(e, control) => handlePartsInlineChange(e, control, row.original.part)}
                value={row.original.part.binNumber || ""}
                className="inline-editable"
              />
          </div>) };
        case 'binNumber2':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Form.Input
                type="text"
                transparent
                name="binNumber2"
                onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                onChange={(e, control) => handlePartsInlineChange(e, control, row.original.part)}
                value={row.original.part.binNumber2 || ""}
                className="inline-editable"
              />
            </div>) };
        case 'footprintName':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Clipboard text={row.original.part[columnName]} />
              <Form.Input
                type="text"
                transparent
                name="footprintName"
                onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                onChange={(e, control) => handlePartsInlineChange(e, control, row.original.part)}
                value={row.original.part.footprintName || ""}
                className="inline-editable"
              />
            </div>)
          };
        case 'symbolName':
          return {
            ...def, Cell: ({ row }) => (<div>
              <Clipboard text={row.original.part[columnName]} />
              <Form.Input
                type="text"
                transparent
                name="symbolName"
                onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                onChange={(e, control) => handlePartsInlineChange(e, control, row.original.part)}
                value={row.original.part.symbolName || ""}
                className="inline-editable"
              />
            </div>)
          };
        case 'customFields':
          return { ...def, Cell: ({ row }) => (<div className="customfields">{row.original.part.customFields.map((i, k) => (<span key={k} className="customfield"><i>{i.field}</i>={i.value}</span>))}</div>) };
        case 'referenceId':
          return { ...def, 
            Header: <Popup
                      key={key}
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.referenceId">
                            Your custom <Icon name="terminal" />
                            reference Id(s) you can use for identifying this part.
                          </Trans>
                        </p>
                      }
                      trigger={<span>{t("label.referenceIds", "Reference Id(s)")}</span>}
                    />,
            Cell: ({ row }) => (<div><Clipboard text={row.original.referenceId} />
              <Popup
                wide="very"
                mouseEnterDelay={PopupDelayMs}
                content={
                  <p>
                    <Trans i18nKey="popup.bom.referenceId">
                      Edit your custom <Icon name="terminal" /> reference Id(s) you can use for identifying this part.
                    </Trans>
                  </p>
                }
                trigger={<Form.Input
                  type="text"
                  transparent
                  name="referenceId"
                  onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                  onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                  value={row.original.referenceId || ""}
                  className="inline-editable"
                />} />
              </div>)};
        case 'schematicReferenceId':
          return { ...def, 
            Header: <Popup
                      key={key}
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.schematicReferenceId">
                            Your custom <Icon name="hashtag" /> schematic reference Id(s) that identify the part on the PCB silkscreen.
                            <br />
                            Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                          </Trans>
                        </p>
                      }
                      trigger={<span>{t("label.schematicReferenceIds", "Schematic Reference Id(s)")}</span>}
                    />, 
            Cell: ({ row }) => (<div><Clipboard text={row.original.schematicReferenceId} />
              <Popup
                wide="very"
                mouseEnterDelay={PopupDelayMs}
                content={
                  <p>
                    <Trans i18nKey="popup.bom.editSchematicReferenceId">
                      Edit your custom <Icon name="hashtag" /> schematic reference Id(s) that identify the part on the PCB silkscreen.
                      <br />
                      Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                    </Trans>
                  </p>
                }
                trigger={<Form.Input
                          type="text"
                          transparent
                          name="schematicReferenceId"
                          onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                          value={row.original.schematicReferenceId || ""}
                          className="inline-editable"
                        />}
              />
            </div>) };
        case 'quantity':
          return { ...def, 
            Header: <Popup
                      mouseEnterDelay={PopupDelayMs}
                      content={<p>{t("popup.bom.quantity", "The quantity of parts required for a single PCB")}</p>}
                      trigger={<span>{t("label.quantity", "Quantity")}</span>}
                    />,
            Cell: ({ row }) => (<div>
              <Popup
                wide
                mouseEnterDelay={PopupDelayMs}
                content={
                  <p>
                    <Trans i18nKey="popup.bom.bomQuantity">
                      Edit the <Icon name="clipboard list" /> BOM quantity required
                    </Trans>
                  </p>
                }
                trigger={
                  <Form.Input
                    type="text"
                    transparent
                    name="quantity"
                    onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                    onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                    value={row.original.quantity || 0}
                    className={`inline-editable ${row.original.quantity > (row.original.part?.quantity || row.original.quantityAvailable || 0) ? "outofstock" : ""}`}
                  />
                }
              />
            </div>) };
        case 'quantityAvailable':
          return {
            ...def,
            Header: <Popup
                      mouseEnterDelay={PopupDelayMs}
                      content={<p>{t("popup.bom.inventoryQuantity", "The quantity of parts currently in inventory")}</p>}
                      trigger={<span>{t("label.inStock", "In Stock")}</span>}
                    />,
            Cell: ({ row }) => (<div>
              <Popup
                wide="very"
                mouseEnterDelay={PopupDelayMs}
                content={
                  <p>
                    <Trans i18nKey="popup.bom.quantityAvailable">
                      Edit the quantity available in your <Icon name="box" /> inventory
                    </Trans>
                  </p>
                }
                trigger={
                  <Form.Input
                    type="text"
                    transparent
                    name="quantityAvailable"
                    onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                    onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                    value={row.original.part?.quantity || row.original.quantityAvailable || 0}
                    className="inline-editable"
                  />
                }
              />
            </div>)
          };
        case 'partType':
            return {...def, Cell: ({row}) => (
              <div onClick={e => handleSelfLink(e, row.original.part, columnName)}>
                <div className="icon-container small">{getIconForPart(row.original.part)} 
                  <div>
                    <span>{row.original.part.partType}</span>
                  </div>
                </div>
              </div>
            )};
        case 'cost':
          return {
            ...def,
            Header: <span key={key}>{t("label.cost", "Cost")}</span>,
            Cell: ({ row }) => (<div>
              <Popup
                wide
                mouseEnterDelay={PopupDelayMs}
                content={
                  <p>
                    <Trans i18nKey="popup.bom.bomCost">Edit the part cost</Trans>
                  </p>
                }
                trigger={
                  <Form.Input
                    label={getCurrencySymbol(row.original.part?.currency || row.original.currency || "USD")}
                    type="text"
                    transparent
                    name="cost"
                    onBlur={(e, control) => handleSaveColumn(e, control, row.original)}
                    onChange={(e, control) => handlePartsInlineChange(e, control, row.original)}
                    value={row.original.part?.cost || row.original.cost || 0}
                    className="inline-editable"
                  />
                }
              />
            </div>)
          };
        case 'totalCost':
          return {
            ...def, Cell: ({ row }) => (
              <>{formatCurrency(row.original.quantity * (row.original.part.cost || row.original.cost || 0), row.original.currency || "USD")}</>
            )
          };
        case 'currency':
          return { ...def, Cell: ({ row }) => (<span>{row.original.part[columnName]}</span>) };
        case 'mountingType':
          return { ...def, Cell: ({ row }) => (<span>{row.original.part[columnName]}</span>) };
        case 'imageUrl':
          return {
            ...def, Header: <i key={key}></i>, columnDefType: 'display', Cell: ({ row }) => (
              <Popup
                content={<p><Image src={row.original.part.imageUrl} size="small"></Image></p>}
                trigger={columnsArray.includes('imageUrl') && columnsVisibleArray.includes('imageUrl') && row.original.part.imageUrl && <Image src={row.original.part.imageUrl} circular inline size="mini" style={{ float: 'right' }} />}
              />
            )
          };
        case 'actions': 
          return {
            ...def, Header: <i key={key}>Select <Link onClick={handleSelectAll}>all</Link> <Link onClick={handleSelectNone}>none</Link></i>, columnDefType: 'display', Cell: ({row}) => (
            <>
                {row.original.partId > 0 && 
                <Popup
                content={<p>{t('button.viewPart', "View Part")}</p>}
                  trigger={<Button circular size='mini' icon='microchip' onClick={e => handleViewPart(e, row.original.part)} />}
                />}
              {columnsArray.includes('datasheetUrl') && columnsVisibleArray.includes('datasheetUrl') && row.original.part.datasheetUrl?.length > 0 && 
                <Popup
                  content={<p>{t('button.viewDatasheet', "View Datasheet")}</p>}
                  trigger={<Button circular size='mini' icon='file pdf outline' onClick={e => handleVisitLink(e, row.original.part.datasheetUrl)} />}
                />}
                {columnsArray.includes('productUrl') && columnsVisibleArray.includes('productUrl') && row.original.part.productUrl &&
                <Popup
                  content={<p>{t('button.viewProduct', "View Product")}</p>}
                  trigger={<Button circular size='mini' icon='file outline' onClick={e => handleVisitLink(e, row.original.part.productUrl)} />}
                />}
              <div style={{ float: 'right', marginTop: '4px' }}>
                {columnsArray.includes('selected') && columnsVisibleArray.includes('selected') && 
                <Popup 
                  content={<p>{t('label.select', "Select")}</p>}
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
          <div style={{display: 'flex', alignItems: 'center'}}>
            <Dropdown selection options={itemsPerPageOptions} value={pageSize} className="labeled" onChange={handlePageSizeChange} style={{width: '75px', minWidth: '75px', marginRight: '10px'}} />
            <div>
              <span>{t("comp.partsGrid.recordsPerPage", "records per page")}</span>
            </div>
          </div>
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

BomPartsGrid.propTypes = {
  /** BOM Project */
  project: PropTypes.object.isRequired,
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
  onRowEditChange: PropTypes.func,
  onSaveInlineChange: PropTypes.func
};
