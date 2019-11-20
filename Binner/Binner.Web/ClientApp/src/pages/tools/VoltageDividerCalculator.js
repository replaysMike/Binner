import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label, Button, Segment, Form, TextArea, Icon, Statistic } from 'semantic-ui-react';
import { decodeResistance } from '../../common/Utils';

export class VoltageDividerCalculator extends Component {
  static displayName = VoltageDividerCalculator.name;

  constructor(props) {
    super(props);
    const voltageDividerPreferences = JSON.parse(localStorage.getItem('voltageDividerPreferences')) || {
      inputVoltage: 120,
      r1: '10k',
      r2: '10k',
    };
    this.state = {
      inputVoltage: voltageDividerPreferences.inputVoltage,
      r1: voltageDividerPreferences.r1,
      r2: voltageDividerPreferences.r2,
      outputVoltage: 0.0
    };
    this.handleChangeValue = this.handleChangeValue.bind(this);
    this.calcOutputVoltage = this.calcOutputVoltage.bind(this);
  }

  componentDidMount() {
    const { inputVoltage, r1, r2 } = this.state;
    this.calcOutputVoltage(inputVoltage, r1, r2);
  }

  handleChangeValue(e, control) {
    const { inputVoltage, r1, r2 } = this.state;
    let newInputVoltage = inputVoltage;
    let newR1 = r1;
    let newR2 = r2;
    switch (control.name) {
      case 'inputVoltage':
        newInputVoltage = Number.parseFloat(control.value);
        break;
      case 'r1':
        newR1 = control.value;
        break;
      case 'r2':
        newR2 = control.value;
        break;
    }
    this.setState({ inputVoltage: newInputVoltage, r1: newR1, r2: newR2 });
    localStorage.setItem('voltageDividerPreferences', JSON.stringify({ inputVoltage: newInputVoltage, r1: newR1, r2: newR2 }));
    this.calcOutputVoltage(newInputVoltage, newR1, newR2);
  }

  calcOutputVoltage(inputVoltage, r1, r2) {
    let r1Val = decodeResistance(r1);
    let r2Val = decodeResistance(r2);
    let outputVoltage = inputVoltage * (r2Val / (r1Val + r2Val));
    this.setState({ outputVoltage });
  }

  render() {
    const { inputVoltage, r1, r2, outputVoltage} = this.state;
    return (
      <div>
        <h1>Voltage Divider Calculator</h1>
        <p>A voltage divider uses 2 resistors to reduce a voltage to a fraction of its input voltage.</p>
        <Form>
          <Segment>
            <Form.Field>
              <label>Voltage Input</label>
              <Input label='V' name='inputVoltage' value={inputVoltage} onChange={this.handleChangeValue} />
            </Form.Field>
            <br />
            <Form.Field>
              <label>Resistor 1</label>
              <Input label='Ω' name='r1' value={r1} onChange={this.handleChangeValue} />
            </Form.Field>
            <br />
            <Form.Field>
              <label>Resistor 2</label>
              <Input label='Ω' name='r2' value={r2} onChange={this.handleChangeValue} />
            </Form.Field>
          </Segment>
          <Segment textAlign='center'>
            <Statistic>
              <Statistic.Value>{outputVoltage.toFixed(2)}</Statistic.Value>
              <Statistic.Label>Output Voltage</Statistic.Label>
            </Statistic>
            <code>
              Vout = Vin * (R2 / R1 + R2)
            </code>
          </Segment>
        </Form>
      </div>
    );
  }
}
