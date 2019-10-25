import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Statistic, Segment } from 'semantic-ui-react';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = {
      summary: {},
      loading: true
    };
  }

  async componentDidMount() {
    await this.loadDashboard();
  }

  async loadDashboard() {
    this.setState({ loading: true });
    const response = await fetch(`part/summary`);

    if (response.status == 200) {
      const data = await response.json();
      this.setState({ summary: data || {}, loading: false });
    }
    else
      this.setState({ summary: {}, loading: false });
  }

  render() {
    const { summary, loading } = this.state;
    return (
      <div>
        <h1>Welcome to Binner</h1>
        <p>Binner is an inventory management app for makers, hobbyists and professionals.</p>
        <p>Choose an action:</p>
        <ul>
          <li><Link to="/inventory/add">Add Inventory</Link></li>
          <li><Link to="/search">Search inventory</Link></li>
          <li><Link to="/datasheets">Datasheet Search</Link></li>
          <li><Link to="/order">Order Low Stock</Link></li>
          <li><Link to="/categories">Manage Categories</Link></li>
          <li><Link to="/projects">Manage Projects</Link></li>
        </ul>

        <h2>Dashboard</h2>
        <div className="divTable">
          <div className="divTableBody">
            <div className="divTableRow">
              <div className="divTableCell header">Dashboard</div>
            </div>
            <div className="divTableRow">
              <div className="divTableCell">
                <Segment loading={loading}>
                  <Statistic>
                    <Statistic.Value>{summary.lowStockCount}</Statistic.Value>
                    <Statistic.Label>Low Stock</Statistic.Label>
                  </Statistic>
                  <Statistic>
                    <Statistic.Value>{summary.partsCount}</Statistic.Value>
                    <Statistic.Label>Parts</Statistic.Label>
                  </Statistic>
                  <Statistic>
                    <Statistic.Value>{summary.projectsCount}</Statistic.Value>
                    <Statistic.Label>Projects</Statistic.Label>
                  </Statistic>
                </Segment>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
