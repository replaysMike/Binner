import { useCallback, useEffect, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { Icon } from "semantic-ui-react";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import customEvents from '../common/customEvents';
import { Sidebar } from '../components/Sidebar';
import { fetchApi } from "../common/fetchApi";
import { Outlet } from "react-router-dom";
import { toast } from "react-toastify";
import { getFriendlyElapsedTime, getTimeDifference } from "../common/datetime";
import { parseJSON } from "date-fns";
import _ from "underscore";
// components
import ErrorModal from "../components/modals/ErrorModal";
import LicenseErrorModal from "../components/modals/LicenseErrorModal";

export function BasicLayout(props) {
  const { t } = useTranslation('en');
  const noop = t('noop', "-do-not-translate-");
  const [version, setVersion] = useState(null);
  const [latestVersion, setLatestVersion] = useState('');
  const [sidebarVisible, setSidebarVisible] = useState(false);
  const [url, setUrl] = useState('');
  const [systemMessages, setSystemMessages] = useState([]);

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
    console.debug('rx latest version', latestVersionData);
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
    console.debug('rx latest messages', systemMessagesData, _.find(systemMessagesData, x => x.readDateUtc === null));
    setSystemMessages(systemMessagesData);
    
    if (_.find(systemMessagesData, x => x.readDateUtc === null)) {
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
          setTimeout(() => {
            setSystemMessages(systemMessagesData.map(x => { x.readDateUtc = new Date(); return x; }));
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

  const toggleOpenSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };

  const openSidebar = () => {
    setSidebarVisible(true);
  };

  const handleView = (e) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const getReadIcon = (readDate) => {
    if (readDate === null) return <Icon className="message-read" name="circle" color="blue" size="mini" style={{ verticalAlign: 'middle' }} />;
    else if (readDate instanceof Date) return <Icon className="message-read" name="circle" color="white" size="mini" style={{ verticalAlign: 'middle' }} />;
    return "";
  };

  return (
    <div className="centered" id="global" style={{marginBottom: '50px', position: 'relative', zIndex: '50', textAlign: 'left'}}>
      {/** Resource Sidebar */}
      <Sidebar
        // this will close the sidebar on click outside
        onHide={() => setSidebarVisible(false)}
        onShow={() => setSidebarVisible(true)}
        visible={sidebarVisible}
      >
        <div>
          <h1>Binner</h1>
          <header>
            <div style={{ marginBottom: '10px' }}>{version ? <span>{t('footer.version', "Version")}: {version} ({latestVersion?.replace('v', '') === version ? 'latest' : <Link onClick={handleView}>update to {latestVersion}</Link>})</span> : ""}</div>
            <Link size="medium" onClick={() => { }} style={{marginLeft: '5px'}}>
              <Icon name="refresh" />
            </Link>
          </header>

          <div className="messages">
            <h2>System Messages</h2>
            <ul className="">
              {systemMessages.map((msg, key) => (
                <li key={key}>
                  <div>
                    <header>{getReadIcon(msg.readDateUtc)}{msg.title}</header>
                    <span className="date">{msg?.dateCreatedUtc !== null ? getFriendlyElapsedTime(getTimeDifference(Date.now(), parseJSON(msg.dateCreatedUtc)), true) : ''}</span>
                    <p>{msg.message}</p>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </Sidebar>

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
      
  );
}
