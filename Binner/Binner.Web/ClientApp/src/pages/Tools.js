import React, { Component } from 'react';
import _ from 'underscore';
import { Segment, Statistic, Icon } from 'semantic-ui-react';

export class Tools extends Component {
  static displayName = Tools.name;

  constructor(props) {
    super(props);
    this.route = this.route.bind(this);
  }

  route(e, url) {
    e.preventDefault();
    e.stopPropagation();
    this.props.history.push(url);
  }

  render() {
    return (
      <div>
        <h1>Tools</h1>
        <p>Binner includes a suite of free utilities common to daily life in electrical engineering.</p>
        <Segment>
          <Statistic.Group widths='three'>
            <Statistic onClick={e => this.route(e, '/tools/resistor')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='wrench' /></Statistic.Value>
              <Statistic.Label>Resistor Color Code Calculator</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/tools/ohmslaw')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='wrench' /></Statistic.Value>
              <Statistic.Label>Ohms Law Calculator</Statistic.Label>
            </Statistic>
            <Statistic onClick={e => this.route(e, '/tools/voltagedivider')} style={{ cursor: 'pointer' }}>
              <Statistic.Value><Icon name='wrench' /></Statistic.Value>
              <Statistic.Label>Voltage Divider Calculator</Statistic.Label>
            </Statistic>
          </Statistic.Group>
        </Segment>
      </div>
    );
  }
}
