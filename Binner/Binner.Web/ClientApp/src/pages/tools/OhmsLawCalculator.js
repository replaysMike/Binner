import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { Input, Segment, Form, Statistic, Breadcrumb } from 'semantic-ui-react';
import { encodeResistance, decodeResistance } from '../../common/Utils';

export function OhmsLawCalculator (props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const ohmsLawPreferences = JSON.parse(localStorage.getItem('ohmsLawPreferences')) || {
    inputVoltage: '120',
    inputCurrent: '',
    inputResistance: '100k',
    inputPower: ''
  };
  const [inputVoltage,setInputVoltage] = useState(ohmsLawPreferences.inputVoltage);
  const [inputCurrent,setInputCurrent] = useState(ohmsLawPreferences.inputCurrent);
  const [inputResistance,setInputResistance] = useState(ohmsLawPreferences.inputResistance);
  const [inputPower,setInputPower] = useState(ohmsLawPreferences.inputPower);
  const [output,setOutput] = useState();
  const [outputCalculation,setOutputCalculation] = useState();
  const [outputUnits,setOutputUnits] = useState();
  const [output2,setOutput2] = useState();
  const [outputCalculation2,setOutputCalculation2] = useState();
  const [outputUnits2,setOutputUnits2] = useState();

  useEffect(() => {
    calcOutput(inputVoltage, inputCurrent, inputResistance, inputPower);
  }, []);

  const handleChangeValue = (e, control) => {
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
      default:
        break;
    }
    setInputVoltage(newInputVoltage);
    setInputCurrent(newInputCurrent);
    setInputResistance(newInputResistance);
    setInputPower(newInputPower);

    localStorage.setItem('ohmsLawPreferences', JSON.stringify({ inputVoltage: newInputVoltage, inputCurrent: newInputCurrent, inputResistance: newInputResistance, inputPower: newInputPower }));
    calcOutput(newInputVoltage, newInputCurrent, newInputResistance, newInputPower);
  }

  const calcOutput = (inputVoltage, inputCurrent, inputResistance, inputPower) => {
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

    setOutput(output);
    setOutputUnits(outputUnits);
    setOutputCalculation(outputCalculation);
    setOutput2(output2);
    setOutputUnits2(outputUnits2);
    setOutputCalculation2(outputCalculation2);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.ohmsLaw', "Ohms Law Calculator")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.tool.ohmsLaw.title', "Ohms Law Calculator")}</h1>
      <p>{t('page.tool.ohmsLaw.description', "Ohms Law explains the relationship between voltage, current and resistance. Input any 2 values to calculate the other 2 values.")}</p>
      <code>
        V = I * R
      </code>
      <Form>
        <Segment>
          <Form.Field>
            <label>{t('page.tool.ohmsLaw.voltage', "Voltage")}</label>
            <Input label='V' name='inputVoltage' value={inputVoltage} onChange={handleChangeValue} />
          </Form.Field>
          <br />
          <Form.Field>
            <label>{t('page.tool.ohmsLaw.current', "Current")}</label>
            <Input label='A' name='inputCurrent' value={inputCurrent} onChange={handleChangeValue} />
          </Form.Field>
          <br />
          <Form.Field>
            <label>{t('page.tool.ohmsLaw.resistance', "Resistance")}</label>
            <Input label='Ω' name='inputResistance' value={inputResistance} onChange={handleChangeValue} />
          </Form.Field>
          <br />
          <Form.Field>
            <label>{t('page.tool.ohmsLaw.power', "Power")}</label>
            <Input label='W' name='inputPower' value={inputPower} onChange={handleChangeValue} />
          </Form.Field>
        </Segment>
        <Segment style={{textAlign: 'center'}}>
          <Statistic.Group widths='two' style={{textAlign: 'center'}}>
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
