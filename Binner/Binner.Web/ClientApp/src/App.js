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
import ErrorModal from "./components/modals/ErrorModal";
import LicenseErrorModal from "./components/modals/LicenseErrorModal";

// styles
import "./custom.css";
import "./bootstrap.css"; /* needed for the carousel control */

// pages
import { NotFound } from "./pages/NotFound";
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
    const handleWindowResize = () => {
      updateView();
    };
    window.addEventListener('resize', handleWindowResize);

    window.showErrorWindow = showErrorWindow;
    window.showLicenseErrorWindow = showLicenseErrorWindow;

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
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Home />} />
            <Route path="accessdenied" element={<AccessDenied />} />
            <Route path="login" element={<Login />} />
            <Route path="account" element={<Account />} />
            <Route path="inventory/:partNumber" element={<Inventory />} />
            <Route path="inventory/add/:partNumberToAdd?" element={<Inventory />} />
            <Route path="inventory" element={<Search />} />
            <Route path="project/:project" element={<Project />} />
            <Route path="bom" element={<Boms />} />
            <Route path="bom/:project" element={<Bom />} />
            <Route path="datasheets" element={<Datasheets />} />
            <Route path="lowstock" element={<LowInventory />} />
            <Route path="import" element={<OrderImport />} />
            <Route path="partTypes" element={<PartTypes />} />
            <Route path="projects" element={<Bom />} />
            <Route path="exportData" element={<ExportData />} />
            <Route path="printing" element={<Printing />} />
            <Route path="printing/printLabels" element={<PrintLabels />} />
            <Route path="printing/labelTemplates" element={<PrintLabels2 />} />
            <Route path="printing/bulkPrint" element={<BulkPrint />} />

            <Route path="tools" element={<Tools />} />
            <Route path="tools/ohmslaw" element={<OhmsLawCalculator />} />
            <Route path="tools/resistor" element={<ResistorColorCodeCalculator />} />
            <Route path="tools/smdresistor" element={<SmdResistorCodeCalculator />} />          
            <Route path="tools/voltagedivider" element={<VoltageDividerCalculator />} />
            <Route path="tools/barcodescanner" element={<BarcodeScanner />} />

            <Route path="settings" element={<Settings />} />
            <Route path="help" element={<Help />} />
            <Route path="help/scanning" element={<Scanning />} />
            <Route path="help/api-integrations" element={<ApiIntegrations />} />
            <Route path="help/bom" element={<BOM />} />

            {/* admin */}

            <Route path="admin" element={<AdminWrapper><Admin /></AdminWrapper>} />
            <Route path="admin/users" element={<AdminWrapper><Users /></AdminWrapper>} />
            <Route path="admin/users/:userId" element={<AdminWrapper><User /></AdminWrapper>} />
            <Route path="admin/backup" element={<AdminWrapper><Backup /></AdminWrapper>} />
            <Route path="admin/info" element={<AdminWrapper><SystemInformation /></AdminWrapper>} />
            <Route path="admin/updateParts" element={<AdminWrapper><UpdateParts /></AdminWrapper>} />
            <Route path="admin/activateLicense" element={<AdminWrapper><ActivateLicense /></AdminWrapper>} />

            <Route path="*" element={<NotFound />} />
          </Route>
        </Routes>
      <ErrorModal context={error} />
      <LicenseErrorModal context={licenseError} />
    </div>
  );
}

export default withSearchParams(App);
