import React, { useState, useEffect } from "react";
import { Route, Routes } from "react-router";
import { useSearchParams } from "react-router-dom";
import { toast } from "react-toastify";

// routing
import PageWrapper from "./routes/PageWrapper";
import AdminWrapper from "./routes/AdminWrapper";

// layouts
import { Layout } from "./layouts/Layout";

// components
import ErrorModal from "./components/ErrorModal";
import LicenseErrorModal from "./components/LicenseErrorModal";

// styles
import "./custom.css";
import "./bootstrap.css"; /* needed for the carousel control */

// pages
import { AccessDenied } from "./pages/AccessDenied";
import { Account } from "./pages/Account";
import { Home } from "./pages/Home";
import { Login } from "./pages/Login";
import Inventory from "./pages/Inventory";
import Search from "./pages/Search";
import Boms from "./pages/Boms";
import Bom from "./pages/Bom";
import Project from "./pages/Project";
import { Datasheets } from "./pages/Datasheets";
import LowInventory from "./pages/LowInventory";
import { OrderImport } from "./pages/OrderImport";
import { PartTypes } from "./pages/PartTypes";
import { ExportData } from "./pages/ExportData";
import { Printing } from "./pages/printing/Home";
import { PrintLabels } from "./pages/printing/PrintLabels";
import { PrintLabels2 } from "./pages/printing/PrintLabels2";
import { BulkPrint } from "./pages/printing/BulkPrint";
import { Tools } from "./pages/Tools";
import { Settings } from "./pages/Settings";
import { OhmsLawCalculator } from "./pages/tools/OhmsLawCalculator";
import { ResistorColorCodeCalculator } from "./pages/tools/ResistorColorCodeCalculator";
import { SmdResistorCodeCalculator } from "./pages/tools/SmdResistorCodeCalculator";
import { VoltageDividerCalculator } from "./pages/tools/VoltageDividerCalculator";
import { BarcodeScanner } from "./pages/tools/BarcodeScanner";
import { Help } from "./pages/help/Home";
import { Scanning } from "./pages/help/Scanning";
import { ApiIntegrations } from "./pages/help/ApiIntegrations";
import { BOM } from "./pages/help/BOM";

// admin
import { Users } from "./pages/admin/users/Users";
import { User } from "./pages/admin/users/User";
import { Admin } from "./pages/admin/Home";
import { Backup } from "./pages/admin/Backup";
import { UpdateParts } from "./pages/admin/UpdateParts";
import { ActivateLicense } from "./pages/admin/ActivateLicense";
import { SystemInformation } from "./pages/admin/SystemInformation";

function withSearchParams(Component) {
  return (props) => <Component {...props} searchParams={useSearchParams()} />;
}

export const App = (props) => {
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
    //console.log('window size', window.innerWidth, window.innerHeight);
    //console.log('document size', document.documentElement.scrollWidth, document.documentElement.scrollHeight);
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
    const handleWindowResize = () => {
      updateView();
    };
    window.addEventListener('resize', handleWindowResize);

    window.showErrorWindow = showErrorWindow;
    window.showLicenseErrorWindow = showLicenseErrorWindow;

    // provide a UI toast when we have authenticated with DigiKey
    if (props.searchParams) {
      const [searchParams] = props.searchParams;
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
      }
    }
    return () => {
      window.removeEventListener('resize', handleWindowResize);
    };
  }, []);

  const showErrorWindow = (errorObject) => {
    if (errorObject && Object.prototype.toString.call(errorObject) === "[object String]") {
      setError({ modalTitle: "Error", url: "", header: "", errorMessage: errorObject, stackTrace: "" });
    } else if (errorObject)
      setError({ modalTitle: "API Error", url: errorObject.url, header: errorObject.exceptionType, errorMessage: errorObject.message, stackTrace: errorObject.stackTrace });
    else setError({ modalTitle: "API Error", url: "", header: "", errorMessage: "", stackTrace: "" });
  };

  const showLicenseErrorWindow = (errorObject) => {
    if (errorObject && Object.prototype.toString.call(errorObject) === "[object String]") {
      setLicenseError({ modalTitle: "License Limitation", url: "", header: "", errorMessage: errorObject });
    } else if (errorObject) this.setState({ licenseError: { modalTitle: "License Limitation", url: errorObject.url, header: errorObject.exceptionType, errorMessage: errorObject.message } });
    else setLicenseError({ modalTitle: "License Limitation", url: "", header: "", errorMessage: "" });
  };

  return (
    <div>
      <Layout>
        <Routes>
          <Route exact path="/" element={<PageWrapper><Home /></PageWrapper>} />
          <Route exact path="/accessdenied" element={<PageWrapper><AccessDenied /></PageWrapper>} />
          <Route exact path="/login" element={<PageWrapper><Login /></PageWrapper>} />
          <Route exact path="/account" element={<PageWrapper><Account /></PageWrapper>} />
          <Route exact path="/inventory/add/:partNumberToAdd" element={<PageWrapper><Inventory /></PageWrapper>} />
          <Route exact path="/inventory/add" element={<PageWrapper><Inventory /></PageWrapper>} />
          <Route exact path="/inventory/:partNumber" element={<PageWrapper><Inventory /></PageWrapper>} />
          <Route exact path="/inventory" element={<PageWrapper><Search /></PageWrapper>} />
          <Route exact path="/project/:project" element={<PageWrapper><Project /></PageWrapper>} />
          <Route exact path="/bom/:project" element={<PageWrapper><Bom /></PageWrapper>} />
          <Route exact path="/bom" element={<PageWrapper><Boms /></PageWrapper>} />
          <Route path="/datasheets" element={<PageWrapper><Datasheets /></PageWrapper>} />
          <Route path="/lowstock" element={<PageWrapper><LowInventory /></PageWrapper>} />
          <Route path="/import" element={<PageWrapper><OrderImport /></PageWrapper>} />
          <Route path="/partTypes" element={<PageWrapper><PartTypes /></PageWrapper>} />
          <Route path="/projects" element={<PageWrapper><Bom /></PageWrapper>} />
          <Route path="/exportData" element={<PageWrapper><ExportData /></PageWrapper>} />
          <Route path="/printing" element={<PageWrapper><Printing /></PageWrapper>} />
          <Route path="/printing/printLabels" element={<PageWrapper><PrintLabels /></PageWrapper>} />
          <Route path="/printing/labelTemplates" element={<PageWrapper><PrintLabels2 /></PageWrapper>} />
          <Route path="/printing/bulkPrint" element={<PageWrapper><BulkPrint /></PageWrapper>} />
          <Route exact path="/tools" element={<PageWrapper><Tools /></PageWrapper>} />
          <Route path="/settings" element={<PageWrapper><Settings /></PageWrapper>} />
          <Route path="/tools/ohmslaw" element={<PageWrapper><OhmsLawCalculator /></PageWrapper>} />
          <Route path="/tools/resistor" element={<PageWrapper><ResistorColorCodeCalculator /></PageWrapper>} />
          <Route path="/tools/smdresistor" element={<PageWrapper><SmdResistorCodeCalculator /></PageWrapper>} />          
          <Route path="/tools/voltagedivider" element={<PageWrapper><VoltageDividerCalculator /></PageWrapper>} />
          <Route path="/tools/barcodescanner" element={<PageWrapper><BarcodeScanner /></PageWrapper>} />
          <Route exact path="/help" element={<PageWrapper><Help /></PageWrapper>} />
          <Route path="/help/scanning" element={<PageWrapper><Scanning /></PageWrapper>} />
          <Route path="/help/api-integrations" element={<PageWrapper><ApiIntegrations /></PageWrapper>} />
          <Route path="/help/bom" element={<PageWrapper><BOM /></PageWrapper>} />

          {/* admin */}

          <Route path="/admin" element={<AdminWrapper><Admin /></AdminWrapper>} />
          <Route path="/admin/users" element={<AdminWrapper><Users /></AdminWrapper>} />
          <Route exact path="/admin/users/:userId" element={<AdminWrapper><User /></AdminWrapper>} />
          <Route path="/admin/backup" element={<AdminWrapper><Backup /></AdminWrapper>} />
          <Route path="/admin/info" element={<AdminWrapper><SystemInformation /></AdminWrapper>} />
          <Route path="/admin/updateParts" element={<AdminWrapper><UpdateParts /></AdminWrapper>} />
          <Route path="/admin/activateLicense" element={<AdminWrapper><ActivateLicense /></AdminWrapper>} />
        </Routes>
      </Layout>
      <ErrorModal context={error} />
      <LicenseErrorModal context={licenseError} />
    </div>
  );
}

export default withSearchParams(App);
