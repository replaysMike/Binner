import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Statistic, Segment, Icon } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";

export function Home(props) {
  const [summary, setSummary] = useState({});
  const [loading, setLoading] = useState(true);
  let navigate = useNavigate();

  useEffect(() => {
    async function load() {
      Home.abortController = new AbortController();
      if (Home.abortController) {
        Home.abortController.abort(); // Cancel the previous request
      }
      Home.abortController = new AbortController();
      setLoading(true);
      await fetchApi(`api/part/summary`, {
        signal: Home.abortController.signal,
      }).then((response) => {
        const { data } = response;
        setSummary(data || {});
        setLoading(false);
      });

      return async function cleanup() {
        await Home.abortController.abort();
      };
    }

    load();
  }, [setLoading, setSummary]);

  const route = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(url);
  };

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Binner is an inventory management app for makers, hobbyists and professionals.</p>
      <Segment>
        <Statistic.Group widths="three">
          <Statistic onClick={(e) => route(e, "/inventory/add")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="plus" />
            </Statistic.Value>
            <Statistic.Label>Add Inventory</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/inventory")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="search" />
            </Statistic.Value>
            <Statistic.Label>Search Inventory</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/datasheets")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="file alternate" />
            </Statistic.Value>
            <Statistic.Label>Datasheets</Statistic.Label>
          </Statistic>
        </Statistic.Group>
        <Statistic.Group widths="four" size="tiny" style={{ marginTop: "50px" }}>
          <Statistic onClick={(e) => route(e, "/lowstock")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="battery low" />
            </Statistic.Value>
            <Statistic.Label>View Low Stock</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/partTypes")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="sitemap" />
            </Statistic.Value>
            <Statistic.Label>Part Types</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/projects")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="folder open" />
            </Statistic.Value>
            <Statistic.Label>Projects</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/exportData")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="cloud download" />
            </Statistic.Value>
            <Statistic.Label>Import/Export</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/print")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="print" />
            </Statistic.Value>
            <Statistic.Label>Print Labels</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/tools")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="wrench" />
            </Statistic.Value>
            <Statistic.Label>Tools</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/settings")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="cog" />
            </Statistic.Value>
            <Statistic.Label>Settings</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>

      <h2>Your Overview</h2>
      <Segment inverted loading={loading} textAlign="center">
        <Statistic.Group widths="five">
          <Statistic color="red" inverted>
            <Statistic.Value>
              <Icon name="battery low" />
              {summary.lowStockCount}
            </Statistic.Value>
            <Statistic.Label>Low Stock</Statistic.Label>
          </Statistic>
          <Statistic color="orange" inverted>
            <Statistic.Value>
              <Icon name="microchip" />
              {summary.partsCount}
            </Statistic.Value>
            <Statistic.Label>Parts</Statistic.Label>
          </Statistic>
          <Statistic color="orange" inverted>
            <Statistic.Value>
              <Icon name="microchip" />
              {summary.uniquePartsCount}
            </Statistic.Value>
            <Statistic.Label>Unique Parts</Statistic.Label>
          </Statistic>
          <Statistic color="green" inverted>
            <Statistic.Value>
              <Icon name="dollar" />
              {(summary.partsCost || 0).toFixed(2)}
            </Statistic.Value>
            <Statistic.Label>Value</Statistic.Label>
          </Statistic>
          <Statistic color="blue" inverted>
            <Statistic.Value>
              <Icon name="folder open" />
              {summary.projectsCount}
            </Statistic.Value>
            <Statistic.Label>Projects</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>
    </div>
  );
}
Home.abortController = new AbortController();
