import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { AddInventory } from './components/AddInventory';
import { Search } from './components/Search';
import { Datasheets } from './components/Datasheets';
import { Order } from './components/Order';
import { OrderImport } from './components/OrderImport';
import { Categories } from './components/Categories';
import { Projects } from './components/Projects';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/inventory/add' component={AddInventory} />
        <Route path='/search' component={Search} />
        <Route path='/datasheets' component={Datasheets} />
        <Route path='/order' component={Order} />
        <Route path='/order/import' component={OrderImport} />
        <Route path='/categories' component={Categories} />
        <Route path='/projects' component={Projects} />
      </Layout>
    );
  }
}
