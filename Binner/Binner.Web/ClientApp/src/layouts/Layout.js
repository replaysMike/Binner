import React, { useCallback, useEffect, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import customEvents from '../common/customEvents';
import { Outlet } from "react-router-dom";
// components
import ErrorModal from "../components/modals/ErrorModal";
import LicenseErrorModal from "../components/modals/LicenseErrorModal";
import { getUserAccount } from "../common/authentication";

export function Layout(props) {
  const { t } = useTranslation('en');
  const noop = t('noop', "-do-not-translate-");
  const [version, setVersion] = useState(null);

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

  // Get the max-width from local storage
  const userAccount = getUserAccount();
  const maxWidth = (userAccount && userAccount.maxWidth) || '50%';

  return (
    <div className="centered" style={{marginBottom: '50px', position: 'relative', zIndex: '50', textAlign: 'left'}}>
      <div id="banner" />
      <Container style={{ maxWidth: maxWidth }}>
        <Header />
        <Outlet />
        <Footer />
      </Container>
      <div className={`promo ${window.avatar ? 'masked' : ''}`}>
        <div style={{marginBottom: '10px'}}>{version ? <span>{t('footer.version', "Version")}: {version}</span> : ""}</div>
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
