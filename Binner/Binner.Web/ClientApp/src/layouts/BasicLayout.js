import { useCallback, useEffect, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router-dom";
import { Icon, Grid, Button, Popup } from "semantic-ui-react";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import { Outlet } from "react-router-dom";
import { toast } from "react-toastify";
import { parseJSON } from "date-fns";
import { getFriendlyElapsedTime, getTimeDifference, getDifferenceInMinutes } from "../common/datetime";
import { config } from "../common/config";
import customEvents from '../common/customEvents';
import { fetchApi } from "../common/fetchApi";
import { isAdmin } from "../common/authentication";
import { getLocalData, setLocalData } from "../common/storage";
import { ThemeChangeToggle } from "../components/ThemeChangeToggle";

import _ from "underscore";
// components
import { Sidebar } from '../components/Sidebar';
import ErrorModal from "../components/modals/ErrorModal";
import LicenseErrorModal from "../components/modals/LicenseErrorModal";
import Logo from "./logo-light.svg?react";
import { ReleaseNotesModal } from "../components/modals/ReleaseNotesModal";

export function BasicLayout(props) {
  const MAX_MESSAGE_LEN = 165;
  const { t } = useTranslation('en');
  let navigate = useNavigate();
  const noop = t('noop', "-do-not-translate-");
  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'sidebar' })
  };

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'sidebar', ...options });
  };


  const [version, setVersion] = useState(null);
  const [latestVersion, setLatestVersion] = useState('');
  const [sidebarVisible, setSidebarVisible] = useState(false);
  const [url, setUrl] = useState('');
  const [systemMessages, setSystemMessages] = useState([]);
  const [isViewMessageModalOpen, setIsViewMessageModalOpen] = useState(false);
  const [viewMessage, setViewMessage] = useState(null);
  const [viewMessageTitle, setViewMessageTitle] = useState(null);
  const [viewMessageDescription, setViewMessageDescription] = useState(null);

  const [error, setError] = useState({
    modalTitle: "",
    url: "",
    header: "",
    errorMessage: "",
    stackTrace: ""
  });
  const [licenseError, setLicenseError] = useState({
    modalTitle: "",
    url: "",
    header: "",
    errorMessage: ""
  });
  const [windowSize, setWindowSize] = useState([
    window.innerWidth,
    window.innerHeight,
  ]);
  const [documentSize, setDocumentSize] = useState([
    document.documentElement.scrollWidth,
    document.documentElement.scrollHeight,
  ]);

  const updateView = () => {
    // used for activating avatar images on largely vertical pages
    setWindowSize([window.innerWidth, window.innerHeight]);
    setDocumentSize([document.documentElement.scrollWidth, document.documentElement.scrollHeight]);
    //console.debug('window size', window.innerWidth, window.innerHeight);
    //console.debug('document size', document.documentElement.scrollWidth, document.documentElement.scrollHeight);
    if (document.documentElement.scrollHeight > window.innerHeight) {
      // enable avatar
      window.avatar = true;
    } else {
      window.avatar = false;
    }
    setSidebarVisible(false);
  };

  useEffect(() => {
    updateView();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [window.location.href]);

  useEffect(() => {
    window.showErrorWindow = showErrorWindow;
    window.showLicenseErrorWindow = showLicenseErrorWindow;

    const handleWindowResize = () => {
      updateView();
    };
    window.addEventListener('resize', handleWindowResize);

    // provide a UI toast when we have authenticated with DigiKey
    if (props.searchParams) {
      const [searchParams, setSearchParams] = props.searchParams;
      const apiAuthSuccess = searchParams.get("api-authenticate") || "";
      if (apiAuthSuccess !== "") {
        let apiName = searchParams.get("api") || "External Api";
        // validate the name
        switch (apiName.toLowerCase()) {
          case "digikey":
          case "mouser":
          case "swarm":
          case "octopart":
          case "arrow":
            break;
          default:
            apiName = "External Api";
            break;
        }
        toast.dismiss();
        if (apiAuthSuccess) toast.success(`Successfully authenticated with ${apiName}!`);
        else toast.error(`Failed to authenticate with ${apiName}!`);

        // remove search params
        searchParams.delete('api-authenticate');
        searchParams.delete('api');
        setSearchParams(searchParams);
      }
    }

    return () => {
      window.removeEventListener('resize', handleWindowResize);
    };
  }, []);

  /**
   * When we receive a version header from an api request, update it.
   */
  const updateVersion = useCallback((data) => {
    setVersion(data.version);
  }, [setVersion]);

  useEffect(() => {
    customEvents.subscribe("version", (data) => updateVersion(data));
  }, [updateVersion]);

  /**
   * When we receive the latest version, update it.
   */
  const updateLatestVersion = useCallback((latestVersionData) => {
    setLatestVersion(latestVersionData.version);
    setUrl(latestVersionData.url);
  }, []);

  useEffect(() => {
    const latestVersionEvent = customEvents.subscribe("latestVersion", (data) => updateLatestVersion(data));
    return () => {
      customEvents.unsubscribe(latestVersionEvent);
    };
  }, [updateLatestVersion]);

  /**
   * When we receive system messages, update it.
   */
  const updateSystemMessages = useCallback((systemMessagesData) => {
    setSystemMessages(systemMessagesData);
    
    if (_.find(systemMessagesData, x => x.readDateUtc === null)) {
      // open sidebar, but lets track how often it opens so we don't annoy the user
      const MaxAutoOpenMinutes = 15; // only open if more than 15 min has elapsed
      const lastOpenTime = getViewPreference('lastOpenTime');
      if (!lastOpenTime || getDifferenceInMinutes(parseJSON(lastOpenTime)) > MaxAutoOpenMinutes)
        openSidebar();

      // mark messages as read
      setTimeout(async () => {
        const unreadMessageIds = _.filter(systemMessagesData, i => i.readDateUtc === null).map(x => x.messageId);
        if (unreadMessageIds.length > 0) {
          await fetchApi("/api/system/messages/read", {
            method: "PUT",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              messageIds: unreadMessageIds
            }),
          });

          // mark messages as read in UI after some time
          setTimeout(() => {
            setSystemMessages(systemMessagesData.map(x => { x.readDateUtc = new Date(); return x; }));
            setTimeout(() => {
              setSystemMessages(systemMessagesData.map(x => { x.readDateUtc = ''; return x; }));
            }, 5000);
          }, 2000);
        }
      }, 2000);
    }
  }, []);

  useEffect(() => {
    const systemMessagesEvent = customEvents.subscribe("systemMessages", (data) => updateSystemMessages(data));
    return () => {
      customEvents.unsubscribe(systemMessagesEvent);
    };
  }, [updateSystemMessages]);

  const showErrorWindow = (errorObject) => {
    if (errorObject && Object.prototype.toString.call(errorObject) === "[object String]") {
      setError({ modalTitle: "Error", url: "", header: "", errorMessage: errorObject, stackTrace: "" });
    } else if (errorObject) {
      setError({ modalTitle: "API Error", url: errorObject.url, header: errorObject.exceptionType, errorMessage: errorObject.message, stackTrace: errorObject.stackTrace });
    } else {
      setError({ modalTitle: "API Error", url: "", header: "", errorMessage: "", stackTrace: "" });
    }
  };

  const showLicenseErrorWindow = (errorObject) => {
    if (errorObject && Object.prototype.toString.call(errorObject) === "[object String]") {
      setLicenseError({ modalTitle: "License Limitation", url: "", header: "", errorMessage: errorObject });
    } else if (errorObject) {
      setLicenseError({ modalTitle: "License Limitation", url: errorObject.url, header: errorObject.exceptionType, errorMessage: errorObject.message });
    } else {
      setLicenseError({ modalTitle: "License Limitation", url: "", header: "", errorMessage: "" });
    }
  };

  const toggleSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };

  const openSidebar = () => {
    setViewPreference('lastOpenTime', new Date());
    setSidebarVisible(true);
  };

  const handleView = (e) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const getReadIcon = (readDate) => {
    if (readDate === null) return <Icon className="message-read" name="circle" color="blue" size="mini" style={{ verticalAlign: 'middle' }} />;
    else if (readDate instanceof Date) return <Icon className="message-read" name="circle" color="grey" size="mini" style={{ verticalAlign: 'middle' }} />;
    return "";
  };

  const route = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(url);
  };

  const handleReadMessage = (e, control, msg) => {
    setIsViewMessageModalOpen(true);
    setViewMessage(msg.message);
    setViewMessageTitle(msg.title);
    setViewMessageDescription(msg?.dateCreatedUtc !== null ? getFriendlyElapsedTime(getTimeDifference(Date.now(), parseJSON(msg.dateCreatedUtc)), true) : '');
  };

  const handleViewMessageModalClose = (e) => {
    setIsViewMessageModalOpen(false);
  };

  return (
    <div className="sticky-container">
      <div className={`sticky ${(window.avatar) ? 'active' : ''} centered`} />
      <div className="centered" id="global" style={{position: 'relative', zIndex: '50', textAlign: 'left'}}>
        {/** Resource Sidebar */}
        <Sidebar
          // this will close the sidebar on click outside
          onHide={() => setSidebarVisible(false)}
          onShow={() => setSidebarVisible(true)}
          visible={sidebarVisible}
        >
          <div className="content-container">
            <div className="content-header">
              <div className="top-controls"><ThemeChangeToggle dark /></div>
              <h1><Logo width="18" height="18" /> <span>Binner</span></h1>
              <header>
                <div className="version">{version ? <span>{t('footer.version', "Version")}: {version} ({latestVersion?.replace('v', '') === version || config.BINNERIO === "true" ? 'latest' : <Link onClick={handleView}>{t('label.updateTo', "update to")} {latestVersion}</Link>})</span> : ""}</div>
              </header>
            </div>
            { /** TOP */}
            <div className="content-top">
              <div className="messages">
                <h2>
                  <Popup
                    content={<p>{t('label.refresh', "Refresh")}</p>}
                    trigger={<Link size="medium" onClick={() => { }} style={{ marginLeft: '5px' }}>
                      <Icon name="refresh" />
                    </Link>}
                  />
                  {t('comp.sidebar.systemMessages', "System Messages")}</h2>
                <ul className="messagelist">
                  {systemMessages.length > 0 
                    ? systemMessages.map((msg, key) => (
                      <li key={key} className="message">
                        <div>
                          <header>{getReadIcon(msg.readDateUtc)}{msg.title}</header>
                          <span className="date">{msg?.dateCreatedUtc !== null ? getFriendlyElapsedTime(getTimeDifference(Date.now(), parseJSON(msg.dateCreatedUtc)), true) : ''}</span>
                          
                          {msg.message.length > MAX_MESSAGE_LEN ? (<><p>{msg.message.substring(0, MAX_MESSAGE_LEN)}...</p><Link className="small" onClick={(e, control) => handleReadMessage(e, control, msg)}>Read more...</Link></>) : <p>{msg.message}</p>}
                        </div>
                      </li>
                  ))
                    : <li><div>{t('comp.sidebar.noMessages', "No messages available")}</div></li>}
                </ul>
              </div>
            </div>
            { /** BOTTOM */}
            <div className="content-bottom">
              <div className="quicklinks centered">
                <h3>{t('comp.sidebar.quickLinks', "Quick Links")}</h3>
                <Grid columns={5} centered>
                  <Grid.Row textAlign="center">
                    <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.searchInventory', "Search Inventory")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.searchInventory', "Search Inventory")} icon="search" color="blue" onClick={e => route(e, '/inventory')} /></div>} /></Grid.Column>
                    <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.tools', "Tools")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.tools', "Tools")} icon="wrench" color="green" onClick={e => route(e, '/tools')} /></div>} /></Grid.Column>
                    <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.bom', "BOM")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.bom', "BOM")} icon="list alternate outline" color="orange" onClick={e => route(e, '/bom')} /></div>} /></Grid.Column>
                    <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.settings', "Settings")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.settings', "Settings")} icon="cog" color="violet" onClick={e => route(e, '/settings')} /></div>} /></Grid.Column>
                    {isAdmin() ? <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.admin', "Admin")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.admin', "Admin")} icon="cog" color="red" onClick={e => route(e, '/admin')} /></div>} /></Grid.Column> : "" }
                  </Grid.Row>
                </Grid>
              </div>
            </div>
          </div>
        </Sidebar>
        <ReleaseNotesModal 
          header={t('comp.sidebar.systemMessages', "System Messages")}
          description={viewMessageDescription}
          text={viewMessage}
          isOpen={isViewMessageModalOpen || false}
          onClose={handleViewMessageModalClose}
        />

        <div id="banner" />
        <Container>
          <Header />
          <Outlet />
          {props.children}
          <Footer />
        </Container>
        <div className={`promo ${window.avatar ? 'masked' : ''}`}>
          <div style={{ marginBottom: '10px' }}>{version ? <span>{t('footer.version', "Version")}: {version}</span> : ""}</div>
          <span>
            <Trans i18nKey="footer.promo">
              Try <a href="https://binner.io">Binner Cloud Free</a>
            </Trans>
          </span>
        </div>
        
        <ErrorModal context={error} />
        <LicenseErrorModal context={licenseError} />
      </div>
    </div>

  );
}
