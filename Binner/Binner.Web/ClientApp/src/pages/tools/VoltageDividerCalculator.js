import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { Input, Segment, Form, Statistic, Breadcrumb } from 'semantic-ui-react';
import { decodeResistance } from '../../common/Utils';

export function VoltageDividerCalculator (props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const voltageDividerPreferences = JSON.parse(localStorage.getItem('voltageDividerPreferences')) || {
    inputVoltage: 120,
    r1: '10k',
    r2: '10k',
  };
  const [inputVoltage, setInputVoltage] = useState(voltageDividerPreferences.inputVoltage);
  const [r1, setR1] = useState(voltageDividerPreferences.r1);
  const [r2, setR2] = useState(voltageDividerPreferences.r2);
  const [outputVoltage, setOutputVoltage] = useState(0);

  useEffect(() => {
    calcOutputVoltage(inputVoltage, r1, r2);
  }, []);
  

  const handleChangeValue = (e, control) => {
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
      default:
        break;
    }
    setInputVoltage(newInputVoltage);
    setR1(newR1);
    setR2(newR2);

    localStorage.setItem('voltageDividerPreferences', JSON.stringify({ inputVoltage: newInputVoltage, r1: newR1, r2: newR2 }));
    calcOutputVoltage(newInputVoltage, newR1, newR2);
  };

  const calcOutputVoltage = (inputVoltage, r1, r2) => {
    let r1Val = decodeResistance(r1);
    let r2Val = decodeResistance(r2);
    let outputVoltage = inputVoltage * (r2Val / (r1Val + r2Val));
    setOutputVoltage(outputVoltage);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.voltDividerCalc', "Voltage Divider Calculator")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.tool.voltDividerCalc.title', 'Voltage Divider Calculator')}</h1>
      <p>{t('page.tool.voltDividerCalc.description', 'A voltage divider uses 2 resistors to reduce a voltage to a fraction of its input voltage.')}</p>
      <Form>
        <Segment>
          <Form.Field>
            <label>{t('page.tool.voltDividerCalc.voltageInput', 'Voltage Input')}</label>
            <Input label='V' name='inputVoltage' value={inputVoltage} onChange={handleChangeValue} />
          </Form.Field>
          <br />
          <Form.Field>
            <label>{t('page.tool.voltDividerCalc.resistor', 'Resistor')} 1</label>
            <Input label='Ω' name='r1' value={r1} onChange={handleChangeValue} />
          </Form.Field>
          <br />
          <Form.Field>
            <label>{t('page.tool.voltDividerCalc.resistor', 'Resistor')} 2</label>
            <Input label='Ω' name='r2' value={r2} onChange={handleChangeValue} />
          </Form.Field>
        </Segment>
        <Segment textAlign='center'>
          <Statistic>
            <Statistic.Value>{outputVoltage.toFixed(2)}</Statistic.Value>
            <Statistic.Label>{t('page.tool.voltDividerCalc.outputVoltage', 'Output Voltage')}</Statistic.Label>
          </Statistic>
          <code>
            Vout = Vin * (R2 / R1 + R2)
          </code>
        </Segment>
      </Form>
    </div>
  );
}
