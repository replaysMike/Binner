import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label, Button, Segment, Form, TextArea, Icon, Statistic } from 'semantic-ui-react';
import { encodeResistance, decodeResistance } from '../../common/Utils';

export class OhmsLawCalculator extends Component {
  static displayName = OhmsLawCalculator.name;

  constructor(props) {
    super(props);
    const ohmsLawPreferences = JSON.parse(localStorage.getItem('ohmsLawPreferences')) || {
      inputVoltage: '120',
      inputCurrent: '',
      inputResistance: '100k',
      inputPower: ''
    };
    this.state = {
      inputVoltage: ohmsLawPreferences.inputVoltage,
      inputCurrent: ohmsLawPreferences.inputCurrent,
      inputResistance: ohmsLawPreferences.inputResistance,
      inputPower: ohmsLawPreferences.inputPower,
      output: '',
      outputCalculation: '',
      outputUnits: '',
      output2: '',
      outputCalculation2: '',
      outputUnits2: '',
    };
    this.handleChangeValue = this.handleChangeValue.bind(this);
    this.calcOutput = this.calcOutput.bind(this);
  }

  componentDidMount() {
    const { inputVoltage, inputCurrent, inputResistance, inputPower } = this.state;
    this.calcOutput(inputVoltage, inputCurrent, inputResistance, inputPower);
  }

  handleChangeValue(e, control) {
    const { inputVoltage, inputCurrent, inputResistance, inputPower } = this.state;
    let newInputVoltage = inputVoltage;
    let newInputCurrent = inputCurrent;
    let newInputResistance = inputResistance;
    let newInputPower = inputPower;
    switch (control.name) {
      case 'inputVoltage':
        newInputVoltage = control.value;
        break;
      case 'inputCurrent':
        newInputCurrent = control.value;
        break;
      case 'inputResistance':
        newInputResistance = control.value;
        break;
      case 'inputPower':
        newInputPower = control.value;
        break;
    }
    this.setState({ inputVoltage: newInputVoltage, inputCurrent: newInputCurrent, inputResistance: newInputResistance, inputPower: newInputPower });
    localStorage.setItem('ohmsLawPreferences', JSON.stringify({ inputVoltage: newInputVoltage, inputCurrent: newInputCurrent, inputResistance: newInputResistance, inputPower: newInputPower }));
    this.calcOutput(newInputVoltage, newInputCurrent, newInputResistance, newInputPower);
  }

  calcOutput(inputVoltage, inputCurrent, inputResistance, inputPower) {
    let inputVoltageVal = Number.parseFloat(inputVoltage);
    let inputCurrentVal = Number.parseFloat(inputCurrent);
    let inputResistanceVal = decodeResistance(inputResistance);
    let inputPowerVal = Number.parseFloat(inputPower);
    let output = 0;
    let outputCalculation = '';
    let outputUnits = '';
    let output2 = 0;
    let outputCalculation2 = '';
    let outputUnits2 = '';
    if (inputVoltageVal > 0 && inputCurrentVal > 0) {
      outputCalculation = 'R = V / I';
      outputUnits = 'Resistance';
      outputCalculation2 = 'P = V * I';
      outputUnits2 = 'Watts';
      output = encodeResistance(inputVoltageVal / inputCurrentVal, 2);
      output2 = inputVoltageVal * inputCurrentVal;
    } else if (inputCurrentVal > 0 && inputResistanceVal > 0) {
      outputCalculation = 'V = I * R';
      outputUnits = 'Voltage';
      outputCalculation2 = 'P = R * I' + String.fromCharCode(178);
      outputUnits2 = 'Watts';
      output = inputCurrentVal * inputResistanceVal;
      output2 = inputCurrentVal * Math.pow(inputResistanceVal, 2);
    } else if (inputVoltageVal > 0 && inputResistanceVal > 0) {
      outputCalculation = 'I = V / R';
      outputUnits = 'Amps';
      outputCalculation2 = 'P = V' + String.fromCharCode(178) + ' / R';
      outputUnits2 = 'Watts';
      output = inputVoltageVal / inputResistanceVal;
      output2 = Math.pow(inputVoltageVal, 2) / inputResistanceVal;
    } else if (inputVoltageVal > 0 && inputPowerVal > 0) {
      outputCalculation = 'I = P / V';
      outputUnits = 'Amps';
      outputCalculation2 = 'R = V' + String.fromCharCode(178) + ' / P';
      outputUnits2 = 'Resistance';
      output = inputPowerVal / inputVoltageVal;
      output2 = Math.pow(inputVoltageVal, 2) / inputPowerVal;
    } else if (inputResistanceVal > 0 && inputPowerVal > 0) {
      outputCalculation = 'V = ' + String.fromCharCode(8730)+'(P * R)';
      outputUnits = 'Voltage';
      outputCalculation2 = 'I = ' + String.fromCharCode(8730)+'(P / R)';
      outputUnits2 = 'Amps';
      output = Math.sqrt(inputResistanceVal * inputPowerVal);
      output2 = Math.sqrt(inputPowerVal / inputResistanceVal);
    } else if (inputCurrentVal > 0 && inputPowerVal > 0) {
      outputCalculation = 'V = P / I';
      outputUnits = 'Voltage';
      outputCalculation2 = 'R = P / I' + String.fromCharCode(178);
      outputUnits2 = 'Amps';
      output = inputPowerVal / inputCurrentVal;
      output2 = inputPowerVal / Math.pow(inputCurrentVal, 2);
    }

    this.setState({ output, outputUnits, outputCalculation, output2, outputUnits2, outputCalculation2 });
  }

  render() {
    const { inputVoltage, inputCurrent, inputResistance, inputPower, output, outputCalculation, outputUnits, output2, outputCalculation2, outputUnits2 } = this.state;
    return (
      <div>
        <h1>Ohms Law Calculator</h1>
        <p>Ohms Law explains the relationship between voltage, current and resistance. Input any 2 values to calculate the other 2 values.</p>
        <code>
          V = I * R
        </code>
        <Form>
          <Segment>
            <Form.Field>
              <label>Voltage</label>
              <Input label='V' name='inputVoltage' value={inputVoltage} onChange={this.handleChangeValue} />
            </Form.Field>
            <br />
            <Form.Field>
              <label>Current</label>
              <Input label='A' name='inputCurrent' value={inputCurrent} onChange={this.handleChangeValue} />
            </Form.Field>
            <br />
            <Form.Field>
              <label>Resistance</label>
              <Input label='Ω' name='inputResistance' value={inputResistance} onChange={this.handleChangeValue} />
            </Form.Field>
            <br />
            <Form.Field>
              <label>Power</label>
              <Input label='W' name='inputPower' value={inputPower} onChange={this.handleChangeValue} />
            </Form.Field>
          </Segment>
          <Segment textAlign='center'>
            <Statistic.Group widths='two' textAlign='center'>
              <Statistic>
                <Statistic.Value>{output}</Statistic.Value>
                <Statistic.Label>{outputUnits}</Statistic.Label>
                <code>
                  {outputCalculation}
                </code>
              </Statistic>
              <Statistic>
                <Statistic.Value>{output2}</Statistic.Value>
                <Statistic.Label>{outputUnits2}</Statistic.Label>
                <code>
                  {outputCalculation2}
                </code>
              </Statistic>
            </Statistic.Group>
          </Segment>
        </Form>
      </div>
    );
  }
}
