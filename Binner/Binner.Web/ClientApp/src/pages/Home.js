import React, { Component } from 'react';
import { Statistic, Segment, Icon } from 'semantic-ui-react';

export class Home extends Component {
  static displayName = Home.name;
  static abortController = new AbortController();

  constructor(props) {
    super(props);
    this.state = {
      summary: {},
      loading: true
    };
    this.route = this.route.bind(this);
  }

  async componentDidMount() {
    await this.loadDashboard();
  }

  componentWillUnmount() {
    Home.abortController.abort();
  }

  async loadDashboard() {
    Home.abortController.abort(); // Cancel the previous request
    Home.abortController = new AbortController();
    this.setState({ loading: true });
    const response = await fetch(`part/summary`, {
      signal: Home.abortController.signal
    });

    if (response.status === 200) {
      const data = await response.json();
      this.setState({ summary: data || {}, loading: false });
    }
    else
      this.setState({ summary: {}, loading: false });
  }

  route(e, url) {
    e.preventDefault();
    e.stopPropagation();
    this.props.history.push(url);
  }

  render() {
    const { summary, loading } = this.state;
    return (
      <div>
        <h1>Welcome to Binner</h1>
        <p>Binner is an inventory management app for makers, hobbyists and professionals.</p>
        <Segment>
          <Statistic.Group widths='three'>
            <Statistic onClick={e => this.route(e, '/inventory/add')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='plus' /></Statistic.Value>
              <Statistic.Label>Add Inventory</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/inventory')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='search' /></Statistic.Value>
              <Statistic.Label>Search Inventory</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/datasheets')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='file alternate' /></Statistic.Value>
              <Statistic.Label>Datasheets</Statistic.Label>
            </Statistic>
          </Statistic.Group>
          <Statistic.Group widths='four' size='tiny' style={{ marginTop: '50px' }}>
            <Statistic onClick={e => this.route(e, '/lowstock')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='battery low' /></Statistic.Value>
              <Statistic.Label>View Low Stock</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/partTypes')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='sitemap' /></Statistic.Value>
              <Statistic.Label>Part Types</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/projects')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='folder open' /></Statistic.Value>
              <Statistic.Label>Projects</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/exportData')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='cloud download' /></Statistic.Value>
              <Statistic.Label>Import/Export</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/print')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='print' /></Statistic.Value>
              <Statistic.Label>Print Labels</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/tools')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='wrench' /></Statistic.Value>
              <Statistic.Label>Tools</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/settings')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='cog' /></Statistic.Value>
              <Statistic.Label>Settings</Statistic.Label>
            </Statistic>
          </Statistic.Group>
        </Segment>

        <h2>Dashboard</h2>
        <Segment inverted loading={loading} textAlign='center'>
          <Statistic.Group widths='five'>
            <Statistic color='red' inverted>
              <Statistic.Value><Icon name='battery low' />{summary.lowStockCount}</Statistic.Value>
              <Statistic.Label>Low Stock</Statistic.Label>
            </Statistic>
            <Statistic color='orange' inverted>
              <Statistic.Value><Icon name='microchip' />{summary.partsCount}</Statistic.Value>
              <Statistic.Label>Parts</Statistic.Label>
            </Statistic>
            <Statistic color='orange' inverted>
              <Statistic.Value><Icon name='microchip' />{summary.uniquePartsCount}</Statistic.Value>
              <Statistic.Label>Unique Parts</Statistic.Label>
            </Statistic>
            <Statistic color='green' inverted>
              <Statistic.Value><Icon name='dollar' />{(summary.partsCost || 0).toFixed(2)}</Statistic.Value>
              <Statistic.Label>Value</Statistic.Label>
            </Statistic>
            <Statistic color='blue' inverted>
              <Statistic.Value><Icon name='folder open' />{summary.projectsCount}</Statistic.Value>
              <Statistic.Label>Projects</Statistic.Label>
            </Statistic>
          </Statistic.Group>
        </Segment>
      </div>
    );
  }
}
