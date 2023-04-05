import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Statistic, Segment, Icon } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { VersionBanner } from "../components/VersionBanner";
import semver from "semver";
import customEvents from '../common/customEvents';

export function Home(props) {
  const { t } = useTranslation();
  const [summary, setSummary] = useState({});
  const [loading, setLoading] = useState(true);
  const [versionData, setVersionData] = useState({});
  const [newVersionBannerIsOpen, setNewVersionBannerIsOpen] = useState(false);
  const [subscription, setSubscription] = useState(null);
  const [version, setVersion] = useState(null);

  const getLatestVersion = useCallback(async (installedVersionData) => {
    const response = await fetch("system/version");
    if (response.ok) {
      const latestVersionData = await response.json();
      setVersionData(latestVersionData);
      const skipVersion = localStorage.getItem("skipVersion") || "1.0.0";
      if (semver.gt(latestVersionData.version, installedVersionData.version) && semver.gt(latestVersionData.version, skipVersion)) {
        // new version is available, and hasn't been skipped
        setNewVersionBannerIsOpen(true);
      }
    }
  }, []);

  const updateVersion = useCallback((installedVersionData, subscriptionId) => {
    setVersion(installedVersionData.version);
    
    getLatestVersion(installedVersionData);

  }, [getLatestVersion]);

  useEffect(() => {
    setSubscription(customEvents.subscribe("version", (data, subscriptionId) => updateVersion(data, subscriptionId)));
  }, [updateVersion, setSubscription]);

  let navigate = useNavigate();

  useEffect(() => {
    async function load() {
      Home.abortController = new AbortController();
      if (Home.abortController) {
        Home.abortController.abort(); // Cancel the previous request
      }
      Home.abortController = new AbortController();
      setLoading(true);
      await fetchApi(`part/summary`, {
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
      <h1>{t('page.home.title', "Dashboard")}</h1>
      <p>{t('page.home.description', "Binner is an inventory management app for makers, hobbyists and professionals.")}</p>
      <Segment>
        <Statistic.Group widths="four">
          <Statistic onClick={(e) => route(e, "/inventory/add")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="plus" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.addInventory', "Add Inventory")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/inventory")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="search" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.searchInventory', "Search Inventory")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/bom")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="list alternate outline" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.bom', "BOM")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/datasheets")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="file alternate" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.datasheets', "Datasheets")}</Statistic.Label>
          </Statistic>
        </Statistic.Group>
        <Statistic.Group widths="three" size="tiny" style={{ marginTop: "50px" }}>
          <Statistic onClick={(e) => route(e, "/lowstock")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="battery low" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.viewLowStock', "View Low Stock")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/partTypes")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="sitemap" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.partTypes', "Part Types")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/exportData")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="cloud download" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.importExport', "Import/Export")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/print")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="print" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.printLabels', "Print Labels")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/tools")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="wrench" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.tools', "Tools")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/settings")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="cog" />
            </Statistic.Value>
            <Statistic.Label>{t('page.home.settings', "Settings")}</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>

      <h2>{t('page.home.yourOverview', "Your Overview")}</h2>
      <Segment inverted loading={loading} textAlign="center">
        <Statistic.Group widths="five">
          <Statistic color="red" inverted>
            <Statistic.Value>
              <Icon name="battery low" />
              {summary.lowStockCount}
            </Statistic.Value>
            <Statistic.Label>{t('page.home.lowStock', "Low Stock")}</Statistic.Label>
          </Statistic>
          <Statistic color="orange" inverted>
            <Statistic.Value>
              <Icon name="microchip" />
              {summary.partsCount}
            </Statistic.Value>
            <Statistic.Label>{t('page.home.parts', "Parts")}</Statistic.Label>
          </Statistic>
          <Statistic color="orange" inverted>
            <Statistic.Value>
              <Icon name="microchip" />
              {summary.uniquePartsCount}
            </Statistic.Value>
            <Statistic.Label>{t('page.home.uniqueParts', "Unique Parts")}</Statistic.Label>
          </Statistic>
          <Statistic color="green" inverted>
            <Statistic.Value>
              <Icon name="dollar" />
              {(summary.partsCost || 0).toFixed(2)}
            </Statistic.Value>
            <Statistic.Label>{t('page.home.value', "Value")}</Statistic.Label>
          </Statistic>
          <Statistic color="blue" inverted>
            <Statistic.Value>
              <Icon name="folder open" />
              {summary.projectsCount}
            </Statistic.Value>
            <Statistic.Label>{t('page.home.projects', "Projects")}</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>
      <VersionBanner isOpen={newVersionBannerIsOpen} version={versionData}></VersionBanner>
    </div>
  );
}
Home.abortController = new AbortController();
