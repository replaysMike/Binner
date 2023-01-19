import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import PartsGrid from "../components/PartsGrid";
import { fetchApi } from '../common/fetchApi';

export function LowInventory (props) {
  const [parts, setParts] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);

  const loadParts = async (page, reset = false) => {
    const response = await fetchApi(`part/low?orderBy=DateCreatedUtc&direction=Descending&results=${pageSize}&page=${page}`);
    const pageOfData = response.data.items;
    let newData = [];
    if (reset)
      newData = [...pageOfData];
    else
      newData = [...parts, ...pageOfData];
    setParts(newData);
    setPage(page);
    setLoading(false);
  };

  useEffect(() => {
    loadParts(page);
}, [page]);

  const handleNextPage = () => {
    const { page, noRemainingData } = this.state;
    if (noRemainingData) return;

    const nextPage = page + 1;
    loadParts(nextPage);
  };

  const handlePartClick = (e, part) => {
    props.history(`/inventory/${encodeURIComponent(part.partNumber)}`);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadParts(page, true);
  };
  
  const columns =  'partNumber,quantity,lowStockThreshold,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl,delete';
  return (
    <div>
      <h1>Low Inventory</h1>
      <p>
        Use this page to reorder parts you are low on.<br/>
        You can define a custom <i>Low Stock</i> value per part in your inventory.
      </p>
      <PartsGrid parts={parts} columns={columns} page={page} totalPages={totalPages} loading={loading} loadPage={handleNextPage} onPartClick={handlePartClick} onPageSizeChange={handlePageSizeChange} name='partsGrid' />
    </div>
  );
};

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <LowInventory {...props} params={useParams()} history={useNavigate()} location={window.location} />
);