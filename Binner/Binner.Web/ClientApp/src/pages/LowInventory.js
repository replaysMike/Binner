import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Breadcrumb, Button, Icon } from "semantic-ui-react";
import PartsGrid2Memoized from "../components/PartsGrid2Memoized";
import { FormHeader } from "../components/FormHeader";
import { fetchApi } from '../common/fetchApi';

export function LowInventory (props) {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  var byParam = searchParams.get("by");
  var valueParam = searchParams.get("value");
  const by = byParam?.split(',') || [];
  const byValue = valueParam?.split(',') || [];
  LowInventory.abortController = new AbortController();
  const [parts, setParts] = useState([]);
  const [filterBy, setFilterBy] = useState(by || []);
  const [filterByValue, setFilterByValue] = useState(byValue || []);
  const [sortBy, setSortBy] = useState("DateCreatedUtc");
  const [sortDirection, setSortDirection] = useState("Descending");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [totalRecords, setTotalRecords] = useState(0);
  const [renderIsDirty, setRenderIsDirty] = useState(true);
  const [initComplete, setInitComplete] = useState(false);

  const handleInit = (config) => {
    setPageSize(config.pageSize);
    setInitComplete(true);
  };

  const loadParts = useCallback(async (page, reset = false, by = filterBy, byValue = filterByValue, results = pageSize, orderBy = sortBy, orderDirection = sortDirection) => {
    const response = await fetchApi(
      `api/part/low?orderBy=${orderBy || ""}&direction=${orderDirection || ""}&results=${results}&page=${page}&by=${by?.join(',')}&value=${byValue?.join(',')}`
    );
    const { data } = response;
    const pageOfData = data.items;
    const totalPages = Math.ceil(data.totalItems / results);
    setTotalRecords(data.totalItems);
    let newData = [];
    if (pageOfData) {
      if (reset)
        newData = [...pageOfData];
      else
        newData = [...parts, ...pageOfData];
    }
    setParts(newData);
    setPage(page);
    setTotalPages(totalPages);
    setTotalRecords(data.totalItems);
    setLoading(false);
    setRenderIsDirty(!renderIsDirty);
  }, [filterBy, filterByValue, pageSize, sortBy, sortDirection]);

  /*useEffect(() => {
    console.log('useEffect 1', byParam, valueParam, initComplete);
    if (pageSize === -1) return;
    if (by && by.length > 0) {
      // likewise, clear keyword if we're in a bin search
      setFilterBy(by);
      setFilterByValue(byValue);
      setPage(1);
      loadParts(page, true, by, byValue, pageSize, sortBy, sortDirection);
    } else {
      setPage(1);
      setFilterBy([]);
      setFilterByValue([]);
      loadParts(page, true, [], [], pageSize, sortBy, sortDirection);
    }

    return () => {
      LowInventory.abortController.abort();
    };
  }, [byParam, valueParam]);*/

  useEffect(() => {
    loadParts(1, true, filterBy, filterByValue, pageSize, sortBy, sortDirection);
  }, [filterBy, filterByValue, pageSize, sortBy, sortDirection, loadParts]);

  const handleNextPage = async (e, page) => {
    await loadParts(page, true);
  };

  const handlePartClick = (e, part) => {
    if (part.partId)
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}:${part.partId}`);
    else
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
  };

  const removeFilter = (e, filterName, filterValue) => {
    e.preventDefault();
    let newFilterBy = [];
    let newFilterByValue = [];
    for(let i =0; i < filterBy.length; i++) {
      if (filterBy[i] === filterName && filterByValue[i] === filterValue) {
        // remove it
      }else{
        newFilterBy.push(filterBy[i]);
        newFilterByValue.push(filterByValue[i]);
      }
    }
    setFilterBy(newFilterBy);
    setFilterByValue(newFilterByValue);

    // replace the browser url
    let newBrowserUrl = '/lowstock';
    if (newFilterBy.length > 0 || newFilterByValue.length > 0) {
      newBrowserUrl += '?';
      if (newFilterBy.length > 0)
        newBrowserUrl += `by=${newFilterBy.join(',')}&value=${newFilterByValue.join(',')}`;
    }
    window.history.pushState(null, null, newBrowserUrl);
    setRenderIsDirty(!renderIsDirty);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadParts(page, true, by, byValue, pageSize);
  };

  const handleSortChange = async (sortBy, sortDirection) => {
    const newSortBy = sortBy || "DateCreatedUtc";
    const newSortDirection = sortDirection || "Descending";
    setSortBy(newSortBy);
    setSortDirection(newSortDirection);
    return await loadParts(page, true, filterBy, filterByValue, pageSize, newSortBy, newSortDirection);
  };

  const renderPartsTable = useMemo(() => {
    return (<PartsGrid2Memoized 
      parts={parts} 
      columns="partNumber,lowStockThreshold,quantity,manufacturerPartNumber,description,partType,packageType,mountingType,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,arrowPartNumber,datasheetUrl,print,delete"
      defaultVisibleColumns='partNumber,lowStockThreshold,quantity,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl,print,delete' 
      page={page} 
      totalPages={totalPages} 
      totalRecords={totalRecords}
      loading={loading} 
      loadPage={handleNextPage} 
      onPartClick={handlePartClick} 
      onPageSizeChange={handlePageSizeChange}
      onSortChange={handleSortChange}
      onInit={handleInit}
      by={filterBy}
      byValue={filterByValue}
      name='partsGrid' 
      visitUrl="/lowstock"
    >{t('message.noMatchingResults', "No matching results.")}</PartsGrid2Memoized>);
  }, [renderIsDirty, parts, page, totalPages, totalRecords, loading, filterBy, filterByValue]);
  
  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.lowInventory.title', "Low Inventory")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.lowInventory.title', "Low Inventory")} to="/">
        <Trans i18nKey="page.lowInventory.description">
          Use this page to reorder parts you are low on.<br/>
          You can define a custom <i>Low Stock</i> value per part in your inventory.
        </Trans>
			</FormHeader>
      <div style={{ paddingTop: "10px", marginBottom: "10px" }}>
        {filterBy && filterBy.map((filterName, index) => (
              <Button key={index} primary size="mini" onClick={e => removeFilter(e, filterName, filterByValue[index])}>
                <Icon name="delete" />
                {filterName}: {filterByValue[index]}
              </Button>       
            ))
          }
      </div>
      {renderPartsTable}
    </div>
  );
};

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <LowInventory {...props} params={useParams()} history={useNavigate()} location={window.location} />
);