import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import { Segment, Form, Input, Breadcrumb, Tab, Grid, Button, Table, Icon } from 'semantic-ui-react';
import { encodeCapacitance } from '../../common/Utils';
import "./SmdCapacitorCodeCalculator.css";

export function SmdCapacitorCodeCalculator() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(0);
  const [capacitance, setCapacitance] = useState("0µF");
  const [selectedValues, setSelectedValues] = useState([null, null, null, null]);
  const canvasRef = useRef();
  const eia198_lookup = [
    { key: 'A', value: 1 },
    { key: 'B', value: 1.1 },
    { key: 'C', value: 1.2 },
    { key: 'D', value: 1.3 },
    { key: 'E', value: 1.5 },
    { key: 'F', value: 1.6 },
    { key: 'G', value: 1.8 },
    { key: 'H', value: 2 },
    { key: 'J', value: 2.2 },
    { key: 'K', value: 2.4 },
    { key: 'a', value: 2.6 },
    { key: 'L', value: 2.7 },
    { key: 'M', value: 3 },
    { key: 'N', value: 3.3 },
    { key: 'b', value: 3.5 },
    { key: 'P', value: 3.6 },
    { key: 'Q', value: 3.9 },
    { key: 'd', value: 4 },
    { key: 'R', value: 4.3 },
    { key: 'e', value: 4.5 },
    { key: 'S', value: 4.7 },
    { key: 'f', value: 5 },
    { key: 'T', value: 5.1 },
    { key: 'U', value: 5.6 },
    { key: 'm', value: 6 },
    { key: 'V', value: 6.2 },
    { key: 'W', value: 6.8 },
    { key: 'n', value: 7 },
    { key: 'X', value: 7.5 },
    { key: 't', value: 8 },
    { key: 'Y', value: 8.2 },
    { key: 'y', value: 9 },
    { key: 'Z', value: 9.1 },
  ];

  useEffect(() => {
    renderCanvas("");
  }, []);

  useEffect(() => {
    renderCanvas("");
  }, [activeTab]);

  const handleTabChange = (e, data) => {
    setActiveTab(data.activeIndex);
    setSelectedValues([null, null, null, null]);
    setCapacitance("0µF");
  };

  const handleSelectValue = (e, control, row, cell) => {
    selectedValues[cell] = row;
    setSelectedValues([...selectedValues]);
    renderCanvas(selectedValues.join('') || '0');

    let value = '';
    switch (activeTab) {
      case 0:
        // 3 digit EIA code
        value = encodeCapacitance(encode3DigitEIACapacitance(selectedValues), 3);
        break;
      case 1:
        // 4 digit EIA code
        value = encodeCapacitance(encode4DigitEIACapacitance(selectedValues), 3);
        break;
      case 2:
        // EIA-198 code
        value = encodeCapacitance(encodeEIA198Capacitance(selectedValues), 3);
        break;
    }
    setCapacitance(value);
  };

  const encode3DigitEIACapacitance = (values) => {
    if (values[1] === 'R') {
      return values[0] + (values[2] * 0.1);
    }
    return ((values[0] * 10) + (values[1])) * (Math.pow(10, values[2]));
  };

  const encode4DigitEIACapacitance = (values) => {
    if (values[1] === 'R') {
      return (values[0]) + (values[2] * 0.1);
    }
    return (((values[0] * 100) + (values[1] * 10)) * Math.pow(10, values[2])) / 10;
  };

  const encodeEIA198Capacitance = (values) => {
    const key = values[0];
    const firstValue = _.find(eia198_lookup, { key: key });
    // the 9th multiplier code is a divide code
    if (values[1] === 9)
      return (firstValue?.value || 0) * 0.1;
    return (firstValue?.value || 0) * Math.pow(10, values[1]);
  };

  const renderCanvas = (text) => {
    // draw a capacitor with color bands
    const canvas = canvasRef.current;
    if (!canvas) return;
    const g = canvas.getContext('2d');
    const gwidth = g.canvas.width;
    const gheight = g.canvas.height;
    // fill rect
    const bgGradient = g.createLinearGradient(0, 0, gwidth + 40, gheight + 40);
    bgGradient.addColorStop(0, '#d1bb91');
    bgGradient.addColorStop(1, '#ad8f4d');
    g.fillStyle = bgGradient;
    g.fillRect(0, 0, gwidth, gheight);
    // draw solder pads
    const solderGradient = g.createLinearGradient(0, 0, 60, gheight + 40);
    solderGradient.addColorStop(0, '#eee');
    solderGradient.addColorStop(1, '#777');
    g.fillStyle = solderGradient;
    g.fillRect(0, 0, 50, gheight);
    const solderGradient2 = g.createLinearGradient(gwidth - 50, 0, gwidth + 10, gheight + 40);
    solderGradient2.addColorStop(0, '#eee');
    solderGradient2.addColorStop(1, '#777');
    g.fillStyle = solderGradient2;
    g.fillRect(gwidth - 50, 0, gwidth, gheight);

    g.strokeStyle = '#333';
    const lineWidth = 2;
    g.lineWidth = lineWidth;
    g.beginPath();
    g.moveTo(0, 0);
    g.lineTo(gwidth, 0);
    g.lineTo(gwidth, gheight);
    g.lineTo(0, gheight);
    g.lineTo(0, 0);
    g.closePath();
    g.stroke();

    g.font = "36px Arial";
    g.fillStyle = '#000';
    text = text || '0';
    const textSize = g.measureText(text);
    const textWidth = textSize.width;
    const textHeight = textSize.actualBoundingBoxAscent + textSize.actualBoundingBoxDescent;
    g.fillText(text, (g.canvas.width / 2) - (textWidth / 2), (g.canvas.height / 2) + (textHeight / 2));
  };

  const getUnits = (capacitance) => {
    let units = 'pF';
    const capacitanceLc = capacitance.toLowerCase();
    if (capacitanceLc.includes('nf'))
      units = 'nF';
    else if (capacitanceLc.includes('µf') || capacitanceLc.includes('uf'))
      units = 'µF';
    else if (capacitanceLc.includes('mf'))
      units = 'mF';
    return units;
  }

  const decodeCapacitanceToPf = (capacitance) => {
    const capacitanceLc = capacitance.toLowerCase();
    const units = getUnits(capacitance);
    const value = parseFloat(capacitanceLc.replace('µf', '').replace('uf', '').replace('nf', '').replace('pf', '').replace('mf', ''));
    switch (units) {
      case 'mF': // millifarad
        return value * 1000 * 1000 * 1000;
      case 'µF': // microfarad
        return value * 1000 * 1000;
      case 'nF': // nanofarad
        return value * 1000;
      default:
      case 'pF': // picofarad
        return value;
    }
  };

  const normalizeUnits = (e) => {
    if (!e.target) return;
    const units = getUnits(e.target.value);
    setCapacitance(e.target.value?.toLowerCase().replace('µf', '').replace('uf', '').replace('nf', '').replace('pf', '').replace('mf', '') + units || '');
  };

  const set3DigitEIAFromCapacitance = (capacitancePf) => {
    const capStr = capacitancePf.toString();
    let numZeros = capStr.length - 2;
    if (numZeros > 9) numZeros = 9;
    const isPrecision = capStr[1] === '.' || capStr[1] === ',';
    const newSelectedValues = [parseInt(capStr[0]), isPrecision ? 'R' : parseInt(capStr[1]), isPrecision ? parseInt(capStr[2]) : numZeros, null];
    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const set4DigitEIAFromCapacitance = (capacitancePf) => {
    const capStr = capacitancePf.toString();
    let numZeros = capStr.length - 2;
    if (numZeros > 9) numZeros = 9;
    const isPrecision = capStr[1] === '.' || capStr[1] === ',';
    const newSelectedValues = [parseInt(capStr[0]), isPrecision ? 'R' : parseInt(capStr[1]), isPrecision ? parseInt(capStr[2]) : numZeros, null];
    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const setEIA198FromCapacitance = (capacitancePf) => {
    const capStr = capacitancePf.toString();

    let numZeros = capStr.length - 2;
    if (numZeros > 9) numZeros = 9;

    let multiplierIndex = capStr.length - 1;
    if (capacitancePf < 1 && (capStr[1] === '.' || capStr[1] === ',')) {
      multiplierIndex = 9;
    }
    const multiplier = Math.pow(10, multiplierIndex);
    let value = capacitancePf / multiplier;
    if (multiplierIndex === 9) {
      value = capacitancePf * 10;
    }
    const eia198Entry = _.find(eia198_lookup, { value: value });
    const newSelectedValues = [eia198Entry?.key || null, multiplierIndex, null, null];
    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const handleSetCapacitance = (e, control) => {
    setCapacitance(control.value);

    // try to decode the capacitance value and update the code if possible
    const capacitanceValuePf = decodeCapacitanceToPf(control.value);
    let newSelectedValues = [null, null, null, null];
    switch (activeTab) {
      case 0:
        // 3 digit EIA code
        newSelectedValues = set3DigitEIAFromCapacitance(capacitanceValuePf);
        break;
      case 1:
        // 4 digit EIA code
        newSelectedValues = set4DigitEIAFromCapacitance(capacitanceValuePf);
        break;
      case 2:
        // EIA-198 code
        newSelectedValues = setEIA198FromCapacitance(capacitanceValuePf);
        break;
    }
    renderCanvas(newSelectedValues.join('') || '0');
  };

  const handleClear = () => {
    setSelectedValues([null, null, null, null]);
    setCapacitance("0µF");
    renderCanvas('');
  };

  const render3DigitEIA = () => {
    const tableRows = [];
    let selected = false;
    for (let row = 0; row < 10; row++) {
      const cells = [];
      for (let cell = 0; cell < 3; cell++) {
        selected = selectedValues[cell] === row;
        if (row === 0 && cell === 0)
          cells.push(<></>);
        else if (cell === 2) {
          cells.push(<Button title={`Multiplier: x${Math.pow(10, row).toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
        } else
          cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
      }
      tableRows.push(cells);
    }
    const cells = [];
    cells.push(<></>);
    selected = selectedValues[1] === 'R';
    cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, 'R', 1)}>R</Button>);
    cells.push(<></>);
    tableRows.push(cells);

    const capacitorCode = selectedValues.join('') || '0';
    return (<Grid divided>
      <Grid.Row>
        <Grid.Column width={4}>
          <Table compact>
            <Table.Body>
              {tableRows.map((row, rowKey) => (
                <Table.Row key={rowKey}>
                  {row.map((cell, cellKey) => (
                    <Table.Cell key={cellKey} textAlign="center">
                      {cell}
                    </Table.Cell>
                  ))}
                </Table.Row>))}
            </Table.Body>
          </Table>
        </Grid.Column>
        <Grid.Column width={12}>
          <div className="results">
            <div className="field">
              <label>Selected Code</label>
              <span>{capacitorCode}</span>
              {capacitorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Capacitance</label>
              <Input name="capacitance" value={capacitance} onChange={handleSetCapacitance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 3 digit EIA code is a 3 digit code where the first 2 digits are the significant figures and the 3rd digit is the multiplier. For example, a code of 102 would be (10 * 10&#178;) = 1nF, while a code of 475 would be (47 * 10&#8309;) = 4.7µF.
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const render4DigitEIA = () => {
    const tableRows = [];
    const multipliers = [
      { key: 'B', tolerance: 0.1.toLocaleString() + 'pF' },
      { key: 'C', tolerance: 0.25.toLocaleString() + 'pF' },
      { key: 'D', tolerance: 0.5.toLocaleString() + 'pF' },
      { key: 'F', tolerance: '1%' },
      { key: 'G', tolerance: '2%' },
      { key: 'J', tolerance: '5%' },
      { key: 'K', tolerance: '10%' },
      { key: 'M', tolerance: '20%' },
      { key: 'Z', tolerance: '+80%/-20%' },
    ];
    let selected = false;
    for (let row = 0; row < 10; row++) {
      const cells = [];
      for (let cell = 0; cell < 4; cell++) {
        selected = selectedValues[cell] === row;
        if ((row === 0 && cell === 0) || (row === 0 && cell === 3))
          cells.push(<></>);
        else {
          if (cell === 3) {
            if (row > multipliers.length) {
              cells.push(<></>);
            } else {
              selected = selectedValues[cell] === multipliers[row - 1].key;
              cells.push(<Button title={`Tolerance: ${multipliers[row - 1].tolerance}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, multipliers[row - 1].key, cell)}>{multipliers[row - 1].key}</Button>);
            }
          } else if (cell === 2) {
            cells.push(<Button title={`Multiplier: x ${Math.pow(10, row).toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
          } else {
            cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
          }
        }
      }
      tableRows.push(cells);
    }
    const cells = [];
    cells.push(<></>);
    selected = selectedValues[1] === 'R';
    cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} disabled={selectedValues[2] === 'R'} onClick={(e, control) => handleSelectValue(e, control, 'R', 1)}>R</Button>);
    cells.push(<></>);
    cells.push(<></>);
    tableRows.push(cells);

    const capacitorCode = selectedValues.join('') || '0';
    const capacitanceValue = encode4DigitEIACapacitance(selectedValues);
    const tolerance = _.find(multipliers, i => i.key === selectedValues[3])?.tolerance || '0%';

    let low = '';
    let high = '';
    if (tolerance.includes('%')) {
      const toleranceValue = parseFloat(tolerance.replace('%', ''));
      low = encodeCapacitance(capacitanceValue - (capacitanceValue * (toleranceValue / 100)), 3);
      high = encodeCapacitance(capacitanceValue + (capacitanceValue * (toleranceValue / 100)), 3);
    } else if (tolerance.includes('pF')) {
      const toleranceValue = parseFloat(tolerance.replace('pF', ''));
      low = encodeCapacitance(capacitanceValue - toleranceValue, 5);
      high = encodeCapacitance(capacitanceValue + toleranceValue, 5);
    }

    return (<Grid divided>
      <Grid.Row>
        <Grid.Column width={6}>
          <Table compact>
            <Table.Body>
              {tableRows.map((row, rowKey) => (
                <Table.Row key={rowKey}>
                  {row.map((cell, cellKey) => (
                    <Table.Cell key={cellKey} textAlign="center">
                      {cell}
                    </Table.Cell>
                  ))}
                </Table.Row>))}
            </Table.Body>
          </Table>
        </Grid.Column>
        <Grid.Column width={10}>
          <div className="results">
            <div className="field">
              <label>Selected Code</label>
              <span>{capacitorCode}</span>
              {capacitorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Capacitance</label>
              <Input name="capacitance" value={capacitance} onChange={handleSetCapacitance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="field">
              <label>Tolerance</label>
              <span>±{tolerance}{low && <span>({low} to {high})</span>}</span>
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 4 digit EIA code is the same as the 3 digit code, but with an additional digit to specify tolerance. For example, a code of 100B would be (100 * 10) = 10nF, while a code of 475F would be (47 * 10&#8309;) = 4.7µF with a tolerance of 1% (4.653µF to 4.747µF).
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const renderEIA198 = () => {
    const tableRows = [];
    let selected = false;

    for (let row = 0; row < eia198_lookup.length; row++) {
      const cells = [];
      for (let cell = 0; cell < 2; cell++) {
        selected = selectedValues[cell] === row;
        if (cell == 0) {
          selected = selectedValues[cell] === eia198_lookup[row].key;
          cells.push(<Button title={`${eia198_lookup[row].value}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, eia198_lookup[row].key, cell)}>{eia198_lookup[row].key}</Button>);
        }
        else {
          if (row < 10)
            cells.push(<Button title={`x ${row === 9 ? 0.1.toLocaleString() : Math.pow(10, row).toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
          else
            cells.push(<></>);
        }
      }
      tableRows.push(cells);
    }

    const capacitorCode = selectedValues.join('') || '0';
    return (<Grid divided>
      <Grid.Row>
        <Grid.Column width={4}>
          <Table compact>
            <Table.Body>
              {tableRows.map((row, rowKey) => (
                <Table.Row key={rowKey}>
                  {row.map((cell, cellKey) => (
                    <Table.Cell key={cellKey} textAlign="center">
                      {cell}
                    </Table.Cell>
                  ))}
                </Table.Row>))}
            </Table.Body>
          </Table>
        </Grid.Column>
        <Grid.Column width={12}>
          <div className="results">
            <div className="field">
              <label>Selected Code</label>
              <span>{capacitorCode}</span>
              {capacitorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Capacitance</label>
              <Input name="capacitance" value={capacitance} onChange={handleSetCapacitance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 2 digit EIA-198 code uses a short case sensitive value identifier (determined using a lookup table) for the 1st digit and the 2nd digit is the multiplier. For example, a code of F6 would be (lookup[F]=1.6 * 10&#8310;) = 1.6µF, while a code of a2 would be (lookup[a]=2.6 * 10&#178;) = 260pF. Note that the last multiplier value of 9 is actually a divider and indicates very small values.
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const tabs = [
    { menuItem: '3 Digit EIA', render: () => <Tab.Pane>{render3DigitEIA()}</Tab.Pane> },
    { menuItem: '4 Digit EIA', render: () => <Tab.Pane>{render4DigitEIA()}</Tab.Pane> },
    { menuItem: 'EIA-198', render: () => <Tab.Pane>{renderEIA198()}</Tab.Pane> },
  ]

  return (
    <div className="smd-capacitor-code-calculator">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.smdCapacitanceCalc', "SMD Capacitor Code Calculator")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.tool.smdCapacitanceCalc.title', 'SMD Capacitor Code Calculator')}</h1>
      <Form>
        <Segment>
          <Tab panes={tabs} onTabChange={handleTabChange} activeIndex={activeTab}></Tab>
        </Segment>
      </Form>
    </div>
  );
}
