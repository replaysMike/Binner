import React, { Component } from "react";
import { Route, Routes } from "react-router";
import { createBrowserHistory as createHistory } from "history";
import { ErrorContext } from "./common/ErrorContext";

// layouts
import { Layout } from "./layouts/Layout";

// components
import ErrorModal from "./components/ErrorModal";

// styles
import "./custom.css";
import './bootstrap.css'; /* needed for the carousel control */

// pages
import { Home } from "./pages/Home";
import Inventory from "./pages/Inventory";
import Search from "./pages/Search";
import { Datasheets } from "./pages/Datasheets";
import LowInventory from "./pages/LowInventory";
import { OrderImport } from "./pages/OrderImport";
import { PartTypes } from "./pages/PartTypes";
import { Projects } from "./pages/Projects";
import { ExportData } from "./pages/ExportData";
import { PrintLabels } from "./pages/PrintLabels";
import { Tools } from "./pages/Tools";
import { Settings } from "./pages/Settings";
import { OhmsLawCalculator } from "./pages/tools/OhmsLawCalculator";
import { ResistorColorCodeCalculator } from "./pages/tools/ResistorColorCodeCalculator";
import { VoltageDividerCalculator } from "./pages/tools/VoltageDividerCalculator";
import { BarcodeScanner } from "./pages/tools/BarcodeScanner";
import { Help } from './pages/help/Home';
import { Scanning } from './pages/help/Scanning';
import { ApiIntegrations } from './pages/help/ApiIntegrations';

export default class App extends Component {
  static displayName = App.name;
  history = createHistory(this.props);

  constructor(){
    super();
    this.state = {
      modalTitle: "",
      url: "",
      header: "",
      errorMessage: "",
      stackTrace: ""
    };
    window.showErrorWindow = this.showErrorWindow;
  }

  showErrorWindow = errorObject => {
    if (errorObject && Object.prototype.toString.call(errorObject) === "[object String]"){
      this.setState({ modalTitle: "Error", url: "", header: "", errorMessage: errorObject, stackTrace: "" });
    }else if (errorObject)
      this.setState({ modalTitle: "API Error", url: errorObject.url, header: errorObject.exceptionType, errorMessage: errorObject.message, stackTrace: errorObject.stackTrace });
    else
      this.setState({ modalTitle: "API Error", url: "", header: "", errorMessage: "", stackTrace: "" });
  };

  render() {
    return (
      <div>
        <Layout history={this.history}>
          <Routes>
            <Route exact path="/" element={<Home />} />
            <Route exact path="/inventory/add" element={<Inventory />} />
            <Route exact path="/inventory/:partNumber" element={<Inventory />} />
            <Route exact path="/inventory" element={<Search />} />
            <Route path="/datasheets" element={<Datasheets />} />
            <Route path="/lowstock" element={<LowInventory />} />
            <Route path="/import" element={<OrderImport />} />
            <Route path="/partTypes" element={<PartTypes />} />
            <Route path="/projects" element={<Projects />} />
            <Route path="/exportData" element={<ExportData />} />
            <Route path="/print" element={<PrintLabels />} />
            <Route exact path="/tools" element={<Tools />} />
            <Route path="/settings" element={<Settings />} />
            <Route path="/tools/ohmslaw" element={<OhmsLawCalculator />} />
            <Route
              path="/tools/resistor"
              element={<ResistorColorCodeCalculator />}
            />
            <Route
              path="/tools/voltagedivider"
              element={<VoltageDividerCalculator />}
            />
            <Route
              path="/tools/barcodescanner"
              element={<BarcodeScanner />}
            />
            <Route exact path="/help" element={<Help />} />
            <Route path="/help/scanning" element={<Scanning />} />
            <Route path="/help/api-integrations" element={<ApiIntegrations />} />
          </Routes>
        </Layout>
        <ErrorContext.Provider value={this.state}>
          <ErrorModal />
        </ErrorContext.Provider>
      </div>
    );
  }
}
