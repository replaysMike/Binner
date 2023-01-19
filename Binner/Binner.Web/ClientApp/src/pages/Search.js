import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Input, Button, Icon } from 'semantic-ui-react';
import { getQueryVariable } from '../common/query';
import PartsGrid from '../components/PartsGrid';
import { fetchApi } from '../common/fetchApi';

export function Search (props) {
  Search.abortController = new AbortController();
  const [searchParams] = useSearchParams();

  const [parts, setParts] = useState([]);
  const [keyword, setKeyword] = useState(getQueryVariable(window.location.search, "keyword") || "");
  const [by, setBy] = useState(getQueryVariable(window.location.search, "by") || "");
  const [byValue, setByValue] = useState(getQueryVariable(window.location.search, "value") || "");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);

  const loadParts = async (page, reset = false, _by = null, _byValue = null, _pageSize = null) => {
    let byParameter = _by;
    let byValueParameter = _byValue;
    let pageSizeParameter = _pageSize;
    if (byParameter === null)
      byParameter = by;
    if (byValueParameter === null)
      byValueParameter = byValue;
    if (pageSizeParameter === null)
      pageSizeParameter = pageSize;

    const response = await fetchApi(`part/list?orderBy=DateCreatedUtc&direction=Descending&results=${pageSizeParameter}&page=${page}&by=${byParameter}&value=${byValueParameter}`);
    const { data } = response;
    const pageOfData = data.items;
    const totalPages = data.totalItems / pageSizeParameter;
    let newData = [];
    if (reset)
      newData = [...pageOfData];
    else
      newData = [...parts, ...pageOfData];
      setParts(newData);
      setPage(page);
      setTotalPages(totalPages);
      setLoading(false);
  };

  const search = async (keyword) => {
    Search.abortController.abort(); // Cancel the previous request
    Search.abortController = new AbortController();
    
    // if there's a keyword we should clear binning (because they use different endpoints)
    setBy('');
    setByValue('');

    setLoading(true);

    try {
      const response = await fetch(`part/search?keywords=${keyword}`, {
        signal: Search.abortController.signal
      });

      if (response.status === 200) {
        const data = await response.json();
        setParts(data || []);
        setLoading(false);
      }
      else {
        setParts([]);
        setLoading(false);
      }
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const searchDebounced = AwesomeDebouncePromise(search, 400);

  useEffect(() => {
    const _keyword = searchParams.get("keyword");
    const _by = searchParams.get("by");
    const _byValue = searchParams.get("value");
    if (_keyword !== keyword) {
      if (_keyword && _keyword.length > 0) {
        // if there's a keyword we should clear binning (because they use different endpoints)
        setKeyword(_keyword);
        setBy("");
        setByValue("");
        setPage(1);
        search(_keyword);
      } else if (_by && _by.length > 0) {
        // likewise, clear keyword if we're in a bin search
        setBy(_by);
        setByValue(_byValue);
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

  const handleNextPage = () => {
    const nextPage = page + 1;
    loadParts(nextPage);
  };

  const handleSearch = (e, control) => {
    switch (control.name) {
      case 'keyword':
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
    setBy('');
    setByValue('');
    props.history(`/inventory`);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadParts(page, true, by, byValue, pageSize);
  };

  return (
    <div>
      <h1>Inventory</h1>
      <Input placeholder='Search' icon='search' focus value={keyword} onChange={handleSearch} name='keyword' />
      <div style={{ paddingTop: '10px', marginBottom: '10px' }}>
        {by && <Button primary size='mini' onClick={removeFilter}><Icon name='delete' />{by}: {byValue}</Button>}
      </div>
      <PartsGrid parts={parts} page={page} totalPages={totalPages} loading={loading} loadPage={handleNextPage} onPartClick={handlePartClick} onPageSizeChange={handlePageSizeChange} name='partsGrid' />
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <Search {...props} params={useParams()} history={useNavigate()} location={window.location} />
);