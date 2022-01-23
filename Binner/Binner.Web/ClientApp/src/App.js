import React, { Component } from "react";
import { Route, Switch } from "react-router";
import { Layout } from "./pages/Layout";
import { Home } from "./pages/Home";
import { Inventory } from "./pages/Inventory";
import { Search } from "./pages/Search";
import { Datasheets } from "./pages/Datasheets";
import { LowInventory } from "./pages/LowInventory";
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
import { createBrowserHistory as createHistory } from "history";
import "./custom.css";
import ErrorModal from "./components/ErrorModal";
import { ErrorContext } from "./common/ErrorContext";

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
          <Switch>
            <Route exact path="/" component={Home} />
            <Route exact path="/inventory/add" component={Inventory} />
            <Route exact path="/inventory/:partNumber" component={Inventory} />
            <Route exact path="/inventory" component={Search} />
            <Route path="/datasheets" component={Datasheets} />
            <Route path="/lowstock" component={LowInventory} />
            <Route path="/import" component={OrderImport} />
            <Route path="/partTypes" component={PartTypes} />
            <Route path="/projects" component={Projects} />
            <Route path="/exportData" component={ExportData} />
            <Route path="/print" component={PrintLabels} />
            <Route exact path="/tools" component={Tools} />
            <Route path="/settings" component={Settings} />
            <Route path="/tools/ohmslaw" component={OhmsLawCalculator} />
            <Route
              path="/tools/resistor"
              component={ResistorColorCodeCalculator}
            />
            <Route
              path="/tools/voltagedivider"
              component={VoltageDividerCalculator}
            />
          </Switch>
        </Layout>
        <ErrorContext.Provider value={this.state}>
          <ErrorModal />
        </ErrorContext.Provider>
      </div>
    );
  }
}
