import React, { useState, useEffect, useMemo, useCallback } from "react";
import { useParams, useNavigate, useSearchParams, Link } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import debounce from "lodash.debounce";
import { Button, Icon, Form, Breadcrumb } from "semantic-ui-react";
import { toast } from "react-toastify";
import ProtectedInput from "../components/ProtectedInput";
import PartsGrid2Memoized from "../components/PartsGrid2Memoized";
import { fetchApi } from "../common/fetchApi";
import { FormHeader } from "../components/FormHeader";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";

export function Search(props) {
  const DebounceTimeMs = 400;
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  var byParam = searchParams.get("by");
  var valueParam = searchParams.get("value");
  var routeCacheKill = searchParams.get("_");
  const by = byParam?.split(',') || [];
  const byValue = valueParam?.split(',') || [];
  const keywordParam = searchParams.get("keyword");
  Search.abortController = new AbortController();

  const [parts, setParts] = useState([]);
  const [keyword, setKeyword] = useState(keywordParam || "");
  const [filterBy, setFilterBy] = useState(by || []);
  const [filterByValue, setFilterByValue] = useState(byValue || []);
  const [sortBy, setSortBy] = useState("DateCreatedUtc");
  const [sortDirection, setSortDirection] = useState("Descending");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(-1);
  const [loading, setLoading] = useState(true);
  const [totalRecords, setTotalRecords] = useState(0);
  const [renderIsDirty, setRenderIsDirty] = useState(true);
  const [initComplete, setInitComplete] = useState(false);
  const [showPartNotFound, setShowPartNotFound] = useState(false);

  const handleInit = (config) => {
    setPageSize(config.pageSize);
    setInitComplete(true);
  };

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = async (e, input) => {
    let cleanPartNumber = "";
    if (input.type === "datamatrix") {
      if (input.value.mfgPartNumber && input.value.mfgPartNumber.length > 0) cleanPartNumber = input.value.mfgPartNumber;
      else if (input.value.description && input.value.description.length > 0) cleanPartNumber = input.value.description;
    } else if (input.type === "code128") {
      cleanPartNumber = input.value;
    }
    setKeyword(cleanPartNumber);

    const response = await fetchApi(`api/part/barcode/info?barcode=${encodeURIComponent(cleanPartNumber.trim())}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      if (data.requiresAuthentication) {
        // redirect for authentication
        window.open(data.redirectUrl, "_blank");
        return;
      }
      if (data.errors && data.errors.length > 0) {
        const errorMessage = data.errors.join("\n");
        if(data.errors[0] === "Api is not enabled."){
          // supress warning for barcode scans
        }
        else
          toast.error(errorMessage);
      } else if (data.response.parts.length > 0) {
        // show the metadata in the UI
        var partInfo =  data.response.parts[0];
        setKeyword(partInfo.basePartNumber);
        await search(partInfo.basePartNumber);
      } else {
        // no barcode found
        setKeyword('');
      }
    } else {
      // error received
      toast.error('Barcode search returned an error!');
    }
  };

  const loadParts = async (page, reset = false, by = filterBy, byValue = filterByValue, results = pageSize, orderBy = sortBy, orderDirection = sortDirection, keyword = null) => {
    const response = await fetchApi(
      `api/part/list?orderBy=${orderBy || ""}&direction=${orderDirection || ""}&results=${results}&page=${page}&keyword=${keyword || ""}&by=${by?.join(',')}&value=${byValue?.join(',')}`
    );
    const { data } = response;
    const pageOfData = data.items;
    const totalPages = Math.ceil(data.totalItems / results);
    let newData = [];
    if (pageOfData) {
      if (reset) 
        newData = [...pageOfData];
      else 
        newData = [...parts, ...pageOfData];
    }
    if (newData.length > 0)
      setShowPartNotFound(false);
    else if(keyword.length > 0)
      setShowPartNotFound(true);
    setParts(newData);
    setPage(page);
    setTotalPages(totalPages);
    setTotalRecords(data.totalItems);
    setLoading(false);
    setRenderIsDirty(!renderIsDirty);
    return response;
  };

  const search = useCallback(async (keyword, by = filterBy, byValue = filterByValue) => {
    Search.abortController.abort(); // Cancel the previous request
    Search.abortController = new AbortController();
    // if there's a keyword we should clear binning (because they use different endpoints)
    setFilterBy([]);
    setFilterByValue([]);

    setLoading(true);
    setShowPartNotFound(false);

    try {
      await fetchApi(`api/part/search?keywords=${encodeURIComponent((keyword || "").trim())}`, {
        signal: Search.abortController.signal,
      }).then((response) => {
        const { data } = response;
        if (response.responseObject.status === 200) {
          setParts(data || []);
          setLoading(false);
        } else {
          setShowPartNotFound(true);
          setParts([]);
          setLoading(false);
        }
        setRenderIsDirty(!renderIsDirty);
      }).catch((response) => {
        if (response.responseObject.status === 404){
          // part not found
          setShowPartNotFound(true);
          setParts([]);
          setLoading(false);
          setRenderIsDirty(!renderIsDirty);
        }
      });
    } catch (ex) {
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  }, [renderIsDirty]);

  const searchDebounced = useMemo(() => debounce(search, DebounceTimeMs), []);
  const loadPartsDebounced = useMemo(() => debounce(loadParts, DebounceTimeMs), []);

  useEffect(() => {
    if (pageSize === -1) return;
    setPage(1);
    setKeyword(keywordParam || "");
    setFilterBy(byParam?.split(',') || []);
    setFilterByValue(valueParam?.split(',') || []);
    loadPartsDebounced(1, true, by, byValue, pageSize, sortBy, sortDirection, keywordParam || "");
    return () => {
      Search.abortController.abort();
    };
  }, [byParam, valueParam, keywordParam, routeCacheKill, initComplete]);

  useEffect(() => {
    loadPartsDebounced(1, true, filterBy, filterByValue, pageSize, sortBy, sortDirection, keyword || "");
  }, [filterBy, filterByValue, keyword]);

  const handlePartClick = (e, part) => {
    if (part.partId)
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}:${part.partId}`);
    else
      props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
  };

  const handleNextPage = async (e, page) => {
    await loadParts(page, true, filterBy, filterByValue, pageSize, sortBy, sortDirection, keyword || "");
  };

  const handleSearch = (e, control) => {
    setKeyword(control.value);
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
    // go

    // replace the browser url
    let newBrowserUrl = '/inventory';
    if (newFilterBy.length > 0 || newFilterByValue.length > 0 || keyword.length > 0) {
      newBrowserUrl += '?';
      if (newFilterBy.length > 0)
        newBrowserUrl += `by=${newFilterBy.join(',')}&value=${newFilterByValue.join(',')}`;
      if (newFilterBy.length > 0 && keyword.length > 0)
        newBrowserUrl += `&`;
      if (keyword.length > 0)
        newBrowserUrl += `keyword=${keyword}`;
    }
    window.history.pushState(null, null, newBrowserUrl);
    setRenderIsDirty(!renderIsDirty);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadParts(page, true, filterBy, filterByValue, pageSize, sortBy, sortDirection, keyword || "");
  };

  const handleSortChange = async (sortBy, sortDirection) => {
    const newSortBy = sortBy || "DateCreatedUtc";
    const newSortDirection = sortDirection || "Descending";
    setSortBy(newSortBy);
    setSortDirection(newSortDirection);
    return await loadParts(page, true, filterBy, filterByValue, pageSize, newSortBy, newSortDirection, keyword || "");
  };

  const renderPartsTable = useMemo(() => {
    return (<PartsGrid2Memoized
        parts={parts}
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
        keyword={keyword}
        name="partsGrid"
      >{t('message.noMatchingResults', "No matching results.")}</PartsGrid2Memoized>);
  }, [renderIsDirty, parts, page, totalPages, totalRecords, loading, filterBy, filterByValue]);


  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} minInputLength={4} swallowKeyEvent={false} />
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.search.title', "Inventory")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.search.title', "Inventory")} to="/" />
      <Form>
        <Form.Field width={5} style={{margin: 0}}>
          <ProtectedInput            
            focus
            placeholder={t('page.search.search', "Search")}
            icon="search"
            value={keyword || ""}
            onChange={handleSearch}
            id="keyword"
            name="keyword"
            clearOnScan={false}
            onBarcodeReadStarted={(e) => { searchDebounced.cancel();  }}
            onBarcodeReadCancelled={(e) => { searchDebounced.cancel();  }}
            onBarcodeReadReceived={(e) => { searchDebounced.cancel();  }}
          />
        </Form.Field>
        <div className="suggested-part">
            {showPartNotFound && 
              <span><Icon name="warning sign" color="yellow" />
              <Trans i18nKey="page.search.searchNotFound">
                No resuts found in inventory. Do you want to <Link to={`/inventory/add/${encodeURIComponent(keyword)}`}>Add</Link> it?
              </Trans>
              </span>}
          </div>
      </Form>
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
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Search {...props} params={useParams()} history={useNavigate()} location={window.location} />;
