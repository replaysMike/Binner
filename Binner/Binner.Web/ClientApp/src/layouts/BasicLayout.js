import { useCallback, useEffect, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import { Outlet } from "react-router-dom";
import { toast } from "react-toastify";
import customEvents from '../common/customEvents';
import { setLocalData } from "../common/storage";
// components
import { Sidebar } from '../components/Sidebar';
import { SidebarContent } from '../components/SidebarContent';
import ErrorModal from "../components/modals/ErrorModal";
import LicenseErrorModal from "../components/modals/LicenseErrorModal";

export function BasicLayout(props) {
  const { t } = useTranslation('en');
  const noop = t('noop', "-do-not-translate-");

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'sidebar', ...options });
  };

  const [version, setVersion] = useState(null);
  const [latestVersion, setLatestVersion] = useState('');
  const [latestVersionUrl, setLatestVersionUrl] = useState('');
  const [sidebarVisible, setSidebarVisible] = useState(false);

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

    /**
     * When we receive the latest version, update it.
     */
    const updateLatestVersion = useCallback((latestVersionData) => {
      setLatestVersion(latestVersionData.version);
      setLatestVersionUrl(latestVersionData.url);
    }, []);
  
    useEffect(() => {
      const latestVersionEvent = customEvents.subscribe("latestVersion", (data) => updateLatestVersion(data));
      return () => {
        customEvents.unsubscribe(latestVersionEvent);
      };
    }, [updateLatestVersion]);

  const openSidebar = () => {
    setViewPreference('lastOpenTime', new Date());
    setSidebarVisible(true);
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
          <SidebarContent
            openSidebar={openSidebar}
            latestVersion={latestVersion}
            latestVersionUrl={latestVersionUrl}
            version={version} />
        </Sidebar>
        <div id="banner" />
        <div className="layout-container">
          <Header />
          <Outlet />
          {props.children}
          <Footer />
        </div>
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
