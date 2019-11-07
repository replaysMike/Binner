import React, { Component } from 'react';
import { Route, Switch } from 'react-router';
import { Layout } from './pages/Layout';
import { Home } from './pages/Home';
import { Inventory } from './pages/Inventory';
import { Search } from './pages/Search';
import { Datasheets } from './pages/Datasheets';
import { LowInventory } from './pages/LowInventory';
import { OrderImport } from './pages/OrderImport';
import { PartTypes } from './pages/PartTypes';
import { Projects } from './pages/Projects';
import { ExportData } from './pages/ExportData';
import { createBrowserHistory as createHistory } from "history";

import './custom.css'

export default class App extends Component {
  static displayName = App.name;
  history = createHistory(this.props);

  constructor(props) {
    super(props);
  }

  render() {
    return (
      <Layout history={this.history}>
        <Switch>
          <Route exact path='/' component={Home} />
          <Route exact path='/inventory/add' component={Inventory} />
          <Route exact path='/inventory/:partNumber' component={Inventory} />
          <Route exact path='/inventory' component={Search} />
          <Route path='/datasheets' component={Datasheets} />
          <Route path='/lowstock' component={LowInventory} />
          <Route path='/import' component={OrderImport} />
          <Route path='/partTypes' component={PartTypes} />
          <Route path='/projects' component={Projects} />
          <Route path='/exportData' component={ExportData} />
        </Switch>
      </Layout>
    );
  }
}
