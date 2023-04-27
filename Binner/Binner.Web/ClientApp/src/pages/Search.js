import React, { useState, useEffect, useMemo } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import _ from "underscore";
import debounce from "lodash.debounce";
import { Input, Button, Icon, Form, Breadcrumb } from "semantic-ui-react";
import { getQueryVariable } from "../common/query";
import PartsGrid2 from "../components/PartsGrid2";
import { fetchApi } from "../common/fetchApi";
import { FormHeader } from "../components/FormHeader";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import customEvents from '../common/customEvents';

export function Search(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  Search.abortController = new AbortController();
  const [searchParams] = useSearchParams();

  const [parts, setParts] = useState([]);
  const [keyword, setKeyword] = useState(getQueryVariable(window.location.search, "keyword") || "");
  const [filterBy, setFilterBy] = useState(getQueryVariable(window.location.search, "by") || "");
  const [filterByValue, setFilterByValue] = useState(getQueryVariable(window.location.search, "value") || "");
  const [sortBy, setSortBy] = useState("DateCreatedUtc");
  const [sortDirection, setSortDirection] = useState("Descending");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    let partNumber = '';
    if (input.type === "datamatrix") {
      partNumber = input.value.mfgPartNumber;
    } else {
      partNumber = input.value;
    }
    setKeyword(partNumber);
    search(partNumber);
  };

  const enableKeyboardListening = () => {
    setIsKeyboardListening(true);
  };

  const disableKeyboardListening = () => {
    setIsKeyboardListening(false);
  };

  const loadParts = async (page, reset = false, by = filterBy, byValue = filterByValue, results = pageSize, orderBy = sortBy, orderDirection = sortDirection) => {
    const response = await fetchApi(
      `api/part/list?orderBy=${orderBy}&direction=${orderDirection}&results=${results}&page=${page}&by=${by}&value=${byValue}`
    );
    const { data } = response;
    const pageOfData = data.items;
    const totalPages = Math.ceil(data.totalItems / results);
    let newData = [];
    if (reset) newData = [...pageOfData];
    else newData = [...parts, ...pageOfData];
    setParts(newData);
    setPage(page);
    setTotalPages(totalPages);
    setLoading(false);
    return response;
  };

  const search = async (keyword) => {
    console.log('search', keyword);
    Search.abortController.abort(); // Cancel the previous request
    Search.abortController = new AbortController();

    // if there's a keyword we should clear binning (because they use different endpoints)
    setFilterBy("");
    setFilterByValue("");

    setLoading(true);

    try {
      await fetchApi(`api/part/search?keywords=${keyword}`, {
        signal: Search.abortController.signal,
      }).then((response) => {
        const { data } = response;
        if (response.responseObject.status === 200) {
          setParts(data || []);
          setLoading(false);
        } else {
          setParts([]);
          setLoading(false);
        }
      }).catch((response) => {
        if (response.status === 404){
          // part not found
          this.setState({ parts: [], loading: false, noRemainingData: true });
        }
      });
    } catch (ex) {
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const searchDebounced = useMemo(() => debounce(search, 400), []);

  useEffect(() => {
    customEvents.notifySubscribers("avatar", true);
    return () => {
      customEvents.notifySubscribers("avatar", false);
    };
  });

  useEffect(() => {
    const _keyword = searchParams.get("keyword");
    const _by = searchParams.get("by");
    const _byValue = searchParams.get("value");
    if (_keyword !== keyword) {
      if (_keyword && _keyword.length > 0) {
        // if there's a keyword we should clear binning (because they use different endpoints)
        setKeyword(_keyword);
        setFilterBy("");
        setFilterByValue("");
        setPage(1);
        search(_keyword);
      } else if (_by && _by.length > 0) {
        // likewise, clear keyword if we're in a bin search
        setFilterBy(_by);
        setFilterByValue(_byValue);
        setKeyword("");
        setPage(1);
        loadParts(page, true, _by, _byValue);
      } else {
        setPage(1);
        loadParts(page, true, "", "");
      }
    } else {
      if (keyword && keyword.length > 0) search(keyword);
      else loadParts(page);
    }

    return () => {
      Search.abortController.abort();
    };
  }, [searchParams]);

  const handlePartClick = (e, part) => {
    props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
  };

  const handleNextPage = (e, page) => {
    loadParts(page, true);
  };

  const handleSearch = (e, control) => {
    switch (control.name) {
      case "keyword":
        if (control.value && control.value.length > 0) {
          searchDebounced(control.value);
        } else {
          loadParts(page, true);
        }
        break;
      default:
        break;
    }
    setKeyword(control.value);
  };

  const removeFilter = (e) => {
    e.preventDefault();
    setFilterBy("");
    setFilterByValue("");
    props.history(`/inventory`);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadParts(page, true, filterBy, filterByValue, pageSize);
  };

  const handleSortChange = async (sortBy, sortDirection) => {
    const newSortBy = sortBy || "DateCreatedUtc";
    const newSortDirection = sortDirection || "Descending";
    setSortBy(newSortBy);
    setSortDirection(newSortDirection);
    return await loadParts(page, true, filterBy, filterByValue, pageSize, newSortBy, newSortDirection);
  };

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={3} />
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.search.title', "Inventory")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.search.title', "Inventory")} to={".."}>
			</FormHeader>
      <Form>
        <Form.Field width={5}>
          <Input            
            placeholder={t('page.search.search', "Search")}
            icon="search"
            focus
            value={keyword}
            onChange={handleSearch}
            name="keyword"
            onFocus={disableKeyboardListening}
            onBlur={enableKeyboardListening}
          />
        </Form.Field>
      </Form>
      <div style={{ paddingTop: "10px", marginBottom: "10px" }}>
        {filterBy && (
          <Button primary size="mini" onClick={removeFilter}>
            <Icon name="delete" />
            {filterBy}: {filterByValue}
          </Button>
        )}
      </div>
      <PartsGrid2
        parts={parts}
        page={page}
        totalPages={totalPages}
        loading={loading}
        loadPage={handleNextPage}
        onPartClick={handlePartClick}
        onPageSizeChange={handlePageSizeChange}
        onSortChange={handleSortChange}
        name="partsGrid"
      >{t('message.noMatchingResults', "No matching results.")}</PartsGrid2>
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Search {...props} params={useParams()} history={useNavigate()} location={window.location} />;
