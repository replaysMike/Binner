import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import { Segment, Form, Input, Breadcrumb, Tab, Grid, Button, Table, Icon } from 'semantic-ui-react';
import { encodeResistance } from '../../common/Utils';
import "./SmdResistorCodeCalculator.css";

export function SmdResistorCodeCalculator() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(0);
  const [selectedValues, setSelectedValues] = useState([null, null, null, null]);
  const [resistance, setResistance] = useState("0Ω");
  const canvasRef = useRef();
  const eia96_lookup = [
    {
      key: 1,
      value: 100
    },
    {
      key: 2,
      value: 102
    },
    {
      key: 3,
      value: 105
    },
    {
      key: 4,
      value: 107
    },
    {
      key: 5,
      value: 110
    },
    {
      key: 6,
      value: 113
    },
    {
      key: 7,
      value: 115
    },
    {
      key: 8,
      value: 118
    },
    {
      key: 9,
      value: 121
    },
    {
      key: 10,
      value: 124
    },
    {
      key: 11,
      value: 127
    },
    {
      key: 12,
      value: 130
    },
    {
      key: 13,
      value: 133
    },
    {
      key: 14,
      value: 137
    },
    {
      key: 15,
      value: 140
    },
    {
      key: 16,
      value: 143
    },
    {
      key: 17,
      value: 147
    },
    {
      key: 18,
      value: 150
    },
    {
      key: 19,
      value: 154
    },
    {
      key: 20,
      value: 158
    },
    {
      key: 21,
      value: 162
    },
    {
      key: 22,
      value: 165
    },
    {
      key: 23,
      value: 169
    },
    {
      key: 24,
      value: 174
    },
    {
      key: 25,
      value: 178
    },
    {
      key: 26,
      value: 182
    },
    {
      key: 27,
      value: 187
    },
    {
      key: 28,
      value: 191
    },
    {
      key: 29,
      value: 196
    },
    {
      key: 30,
      value: 200
    },
    {
      key: 31,
      value: 205
    },
    {
      key: 32,
      value: 210
    },
    {
      key: 33,
      value: 215
    },
    {
      key: 34,
      value: 221
    },
    {
      key: 35,
      value: 226
    },
    {
      key: 36,
      value: 232
    },
    {
      key: 37,
      value: 237
    },
    {
      key: 38,
      value: 243
    },
    {
      key: 39,
      value: 249
    },
    {
      key: 40,
      value: 255
    },
    {
      key: 41,
      value: 261
    },
    {
      key: 42,
      value: 267
    },
    {
      key: 43,
      value: 274
    },
    {
      key: 44,
      value: 280
    },
    {
      key: 45,
      value: 287
    },
    {
      key: 46,
      value: 294
    },
    {
      key: 47,
      value: 301
    },
    {
      key: 48,
      value: 309
    },
    {
      key: 49,
      value: 316
    },
    {
      key: 50,
      value: 324
    },
    {
      key: 51,
      value: 332
    },
    {
      key: 52,
      value: 340
    },
    {
      key: 53,
      value: 348
    },
    {
      key: 54,
      value: 357
    },
    {
      key: 55,
      value: 365
    },
    {
      key: 56,
      value: 374
    },
    {
      key: 57,
      value: 383
    },
    {
      key: 58,
      value: 392
    },
    {
      key: 59,
      value: 402
    },
    {
      key: 60,
      value: 412
    },
    {
      key: 61,
      value: 422
    },
    {
      key: 62,
      value: 432
    },
    {
      key: 63,
      value: 442
    },
    {
      key: 64,
      value: 453
    },
    {
      key: 65,
      value: 464
    },
    {
      key: 66,
      value: 475
    },
    {
      key: 67,
      value: 487
    },
    {
      key: 68,
      value: 499
    },
    {
      key: 69,
      value: 511
    },
    {
      key: 70,
      value: 523
    },
    {
      key: 71,
      value: 536
    },
    {
      key: 72,
      value: 549
    },
    {
      key: 73,
      value: 562
    },
    {
      key: 74,
      value: 576
    },
    {
      key: 75,
      value: 590
    },
    {
      key: 76,
      value: 604
    },
    {
      key: 77,
      value: 619
    },
    {
      key: 78,
      value: 634
    },
    {
      key: 79,
      value: 649
    },
    {
      key: 80,
      value: 665
    },
    {
      key: 81,
      value: 681
    },
    {
      key: 82,
      value: 698
    },
    {
      key: 83,
      value: 715
    },
    {
      key: 84,
      value: 732
    },
    {
      key: 85,
      value: 750
    },
    {
      key: 86,
      value: 768
    },
    {
      key: 87,
      value: 787
    },
    {
      key: 88,
      value: 806
    },
    {
      key: 89,
      value: 825
    },
    {
      key: 90,
      value: 845
    },
    {
      key: 91,
      value: 866
    },
    {
      key: 92,
      value: 887
    },
    {
      key: 93,
      value: 909
    },
    {
      key: 94,
      value: 931
    },
    {
      key: 95,
      value: 953
    },
    {
      key: 96,
      value: 976
    }
  ];
  const eia96_multipliers = [
    { key: 'A', value: 1 },
    { key: 'B', value: 10 },
    { key: 'C', value: 100 },
    { key: 'D', value: 1000 },
    { key: 'E', value: 10000 },
    { key: 'F', value: 100000 },
    { key: 'H', value: 10 },
    { key: 'R', value: 0.01 },
    { key: 'S', value: 0.1 },
    { key: 'X', value: 0.1 },
    { key: 'Y', value: 0.01 },
    { key: 'Z', value: 0.001 },
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
    setResistance("0Ω");
  };

  const handleSelectValue = (e, control, row, cell) => {
    selectedValues[cell] = row;
    setSelectedValues([...selectedValues]);
    renderCanvas(selectedValues.join('') || '0');

    let value = '';
    switch (activeTab) {
      case 0:
        // 3 digit EIA code
        value = encodeResistance(encode3DigitEIAResistance(selectedValues), 3);
        break;
      case 1:
        // 4 digit EIA code
        value = encodeResistance(encode4DigitEIAResistance(selectedValues), 3);
        break;
      case 2:
        // EIA-96 code
        value = encodeResistance(encodeEIA96Resistance(selectedValues), 3);
        break;
    }
    setResistance(value);
  };

  const encode3DigitEIAResistance = (values) => {
    if (values[1] === 'R') {
      return values[0] + (values[2] * 0.1);
    }
    return ((values[0] * 10) + (values[1])) * (Math.pow(10, values[2]));
  };

  const encode4DigitEIAResistance = (values) => {
    if (values[1] === 'R') {
      return values[0] + (values[2] * 0.1) + (values[3] * 0.01);
    } else if (values[2] === 'R') {
      return (values[0] * 10) + (values[1]) + (values[3] * 0.1);
    }
    return ((values[0] * 100) + (values[1] * 10) + (values[2])) * Math.pow(10, values[3]);
  };

  const encodeEIA96Resistance = (values) => {
    if (values[1] === 'R') {
      return values[0] + (values[2] * 0.1);
    }
    const key = (values[0] * 10) + values[1];
    const firstValue = _.find(eia96_lookup, { key: key });
    const multiplier = _.find(eia96_multipliers, { key: values[2]});
    return (firstValue?.value || 0) * (multiplier.value);
  };

  const renderCanvas = (text) => {
    // draw a resistor with color bands
    const canvas = canvasRef.current;
    if (!canvas) return;
    const g = canvas.getContext('2d');
    const gwidth = g.canvas.width;
    const gheight = g.canvas.height;
    // fill rect
    const bgGradient = g.createLinearGradient(0, 0, gwidth + 40, gheight + 40);
    bgGradient.addColorStop(0, '#000');
    bgGradient.addColorStop(1, '#666');
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
    g.fillStyle = '#fff';
    text = text || '0';
    const textSize = g.measureText(text);
    const textWidth = textSize.width;
    const textHeight = textSize.actualBoundingBoxAscent + textSize.actualBoundingBoxDescent;
    g.fillText(text, (g.canvas.width / 2) - (textWidth / 2), (g.canvas.height / 2) + (textHeight / 2));
  };

  const getUnits = (resistance) => {
    let units = 'Ω';
    const resistanceLc = resistance.toLowerCase();
    // ordering matters here due to single character matches
    if (resistanceLc.includes('mega'))
      units = 'MΩ';
    else if (resistanceLc.includes('giga'))
      units = 'GΩ';
    else if (resistanceLc.includes('kilo'))
      units = 'kΩ';
    else if (resistanceLc.includes('ohms') || resistanceLc.includes('ohm'))
      return units;

    // continue with single character matches
    else if (resistanceLc.includes('mΩ') || resistanceLc.includes('m'))
      units = 'MΩ';
    else if (resistanceLc.includes('gΩ') || resistanceLc.includes('g'))
      units = 'GΩ';
    else if (resistanceLc.includes('kΩ') || resistanceLc.includes('k'))
      units = 'kΩ';
    return units;
  }

  const removeUnits = (resistance) => {
    // ordering matters here due to single character matches
    return resistance?.toLowerCase()
      .replace('gΩ', '')
      .replace('mΩ', '')
      .replace('kΩ', '')
      .replace('Ω', '')
      .replace('megaohm', '')
      .replace('mega', '')
      .replace('gigaohm', '')
      .replace('giga', '')
      .replace('ohms', '')
      .replace('ohm', '')
      .replace('o', '')
      .replace('m', '')
      .replace('g', '')
      .replace('k', '');
  };

  const decodeResistanceToOhms = (resistance) => {
    const units = getUnits(resistance);
    const value = parseFloat(removeUnits(resistance));
    switch (units) {
      case 'GΩ': // gigaohms
        return value * 1000 * 1000 * 1000;
      case 'MΩ': // megaohms
        return value * 1000 * 1000;
      case 'kΩ': // kiloohms
        return value * 1000;
      default:
      case 'Ω': // ohms
        return value;
    }
  };

  const normalizeUnits = (e) => {
    if (!e.target) return;
    const units = getUnits(e.target.value);
    setResistance(removeUnits(e.target.value) + units || '');
  };

  const set3DigitEIAFromResistance = (resistanceOhms) => {
    const resStr = resistanceOhms.toString();
    let numZeros = resStr.length - 2;
    if (numZeros > 9) numZeros = 9;
    const isPrecision = resStr[1] === '.' || resStr[1] === ',';
    const newSelectedValues = [parseInt(resStr[0]), isPrecision ? 'R' : parseInt(resStr[1]), isPrecision ? parseInt(resStr[2]) : numZeros, null];
    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const set4DigitEIAFromResistance = (resistanceOhms) => {
    const resStr = resistanceOhms.toString();
    let numZeros = resStr.length - 3;
    if (numZeros > 9) numZeros = 9;
    const isPrecision1 = resStr[1] === '.' || resStr[1] === ',';
    const isPrecision2 = resStr[2] === '.' || resStr[2] === ',';
    const isPrecision = isPrecision1 || isPrecision2;
    const newSelectedValues = [parseInt(resStr[0]), isPrecision1 ? 'R' : parseInt(resStr[1]), isPrecision2 ? 'R' : parseInt(resStr[2]) , isPrecision ? parseInt(resStr[3]) : numZeros];
    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const setEIA96FromResistance = (resistanceOhms) => {
    const resStr = resistanceOhms.toString();
    // determine the multiplier
    const mostSigDigits = parseInt(parseInt(resStr.replace('.', '').replace(',', '')).toString().substring(0, 3));
    let numZeros = resStr.length - 2;
    if (numZeros > 9) numZeros = 9;
    let multiplier = numZeros > 0 ? Math.pow(10, numZeros - 1) : 1;
    if (resistanceOhms < 100) {
      // multiplier is a decimal (R, S, X, Y, Z)
      const digits = resStr.split('.')[1]?.length || 0;
      multiplier = Math.trunc(Math.pow(0.1, digits) * 1000) / 1000;
    }
    const multiplierKey = _.find(eia96_multipliers, (m) => m.value === multiplier)?.key;
    // determine the value
    const value = mostSigDigits;
    const eia96Entry = _.find(eia96_lookup, { value: value });
    const newSelectedValues = [parseInt(eia96Entry?.key.toString()[0]), parseInt(eia96Entry?.key.toString()[1]), multiplierKey, null];

    setSelectedValues(newSelectedValues);
    return newSelectedValues;
  };

  const handleSetResistance = (e, control) => {
    setResistance(control.value);

    // try to decode the capacitance value and update the code if possible
    const resistanceValueOhms = decodeResistanceToOhms(control.value);
    let newSelectedValues = [null, null, null, null];
    switch (activeTab) {
      case 0:
        // 3 digit EIA code
        newSelectedValues = set3DigitEIAFromResistance(resistanceValueOhms);
        break;
      case 1:
        // 4 digit EIA code
        newSelectedValues = set4DigitEIAFromResistance(resistanceValueOhms);
        break;
      case 2:
        // EIA-96 code
        newSelectedValues = setEIA96FromResistance(resistanceValueOhms);
        break;
    }
    renderCanvas(newSelectedValues.join('') || '0');
  };

  const handleClear = () => {
    setSelectedValues([null, null, null, null]);
    setResistance("0Ω");
    renderCanvas('');
  };

  const render3DigitEIA = () => {
    const tableRows = [];
    let selected = false;
    for (let row = 0; row < 10; row++) {
      const cells = [];
      for (let cell = 0; cell < 3; cell++) {
        selected = selectedValues[cell] === row;
        if (cell === 2) {
          cells.push(<Button title={`Multiplier x ${Math.pow(10, row).toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
        } else {
          cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
        }
      }
      tableRows.push(cells);
    }
    const cells = [];
    cells.push(<></>);
    selected = selectedValues[1] === 'R';
    cells.push(<Button title="Indicates decimal place" type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, 'R', 1)}>R</Button>);
    cells.push(<></>);
    tableRows.push(cells);

    const resistorCode = selectedValues.join('') || '0';
    //const resistance = encodeResistance(encode3DigitEIAResistance(selectedValues), 1);
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
              <span>{resistorCode}</span>
              {resistorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Resistance</label>
              <Input name="resistance" value={resistance} onChange={handleSetResistance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 3 digit EIA code is a 3 digit code where the first 2 digits are the significant figures and the 3rd digit is the multiplier. For example, a code of 102 would be (10 * 10&#178;) = 1kΩ, while a code of 473 would be (47 * 10&#179;) = 47kΩ.
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const render4DigitEIA = () => {
    const tableRows = [];
    let selected = false;
    for (let row = 0; row < 10; row++) {
      const cells = [];
      for (let cell = 0; cell < 4; cell++) {
        selected = selectedValues[cell] === row;
        if (cell === 3) {
          cells.push(<Button title={`Multiplier x ${Math.pow(10, row).toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
        } else {
          cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
        }
      }
      tableRows.push(cells);
    }
    const cells = [];
    cells.push(<></>);
    selected = selectedValues[1] === 'R';
    cells.push(<Button title="Indicates decimal place" type="button" size="mini" className={`${selected ? 'selected' : ''}`} disabled={selectedValues[2] === 'R'} onClick={(e, control) => handleSelectValue(e, control, 'R', 1)}>R</Button>);
    selected = selectedValues[2] === 'R';
    cells.push(<Button title="Indicates decimal place" type="button" size="mini" className={`${selected ? 'selected' : ''}`} disabled={selectedValues[1] === 'R'} onClick={(e, control) => handleSelectValue(e, control, 'R', 2)}>R</Button>);
    cells.push(<></>);
    tableRows.push(cells);

    const resistorCode = selectedValues.join('') || '0';
    //const resistance = encodeResistance(encode4DigitEIAResistance(selectedValues), 2);
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
              <span>{resistorCode}</span>
              {resistorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Resistance</label>
              <Input name="resistance" value={resistance} onChange={handleSetResistance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 4 digit EIA code is the same as the 3 digit code, but with an additional digit for more precision. The first 3 digits are the same as the 3 digit code, and the 4th digit is the multiplier. For example, a code of 1001 would be (100 * 10) = 1kΩ, while a code of 1002 would be (100 * 10&#178;) = 10kΩ.
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const renderEIA96 = () => {
    const tableRows = [];
    let selected = false;
    
    for (let row = 0; row < 10; row++) {
      const cells = [];
      for (let cell = 0; cell < 3; cell++) {
        selected = selectedValues[cell] === row;
        if (cell == 2) {
          selected = selectedValues[cell] === eia96_multipliers[row].key;
          cells.push(<Button title={`x ${eia96_multipliers[row].value.toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, eia96_multipliers[row].key, cell)}>{eia96_multipliers[row].key}</Button>);
        }
        else
          cells.push(<Button type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, row, cell)}>{row}</Button>);
      }
      tableRows.push(cells);
    }
    let cells = [];
    cells.push(<></>);
    cells.push(<></>);

    selected = selectedValues[2] === 'Y';
    cells.push(<Button title={`x ${eia96_multipliers[10].value.toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, 'Y', 2)}>Y</Button>);
    tableRows.push(cells);
    cells = [];
    cells.push(<></>);
    cells.push(<></>);
    selected = selectedValues[2] === 'Z';
    cells.push(<Button title={`x ${eia96_multipliers[11].value.toLocaleString()}`} type="button" size="mini" className={`${selected ? 'selected' : ''}`} onClick={(e, control) => handleSelectValue(e, control, 'Z', 2)}>Z</Button>);
    tableRows.push(cells);

    const resistorCode = selectedValues.join('') || '0';
    //const resistance = encodeResistance(encodeEIA96Resistance(selectedValues), 3);
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
              <span>{resistorCode}</span>
              {resistorCode !== '0' && <Icon name="times circle" onClick={handleClear} style={{ cursor: 'pointer', marginLeft: '5px' }} color='grey' />}
            </div>
            <div className="field">
              <label>Resistance</label>
              <Input name="resistance" value={resistance} onChange={handleSetResistance} onBlur={normalizeUnits} autoComplete="off" />
            </div>
            <div className="canvas">
              <canvas ref={canvasRef} width={250} height={100} />
            </div>
            <div className="description">
              The 3 digit EIA-96 code is a 3 digit code where the first 2 digits are the significant figures (determined using a lookup table) and the 3rd digit is the multiplier. For example, a code of 12B would be (lookup[12]=13 * 10&#178;) = 1.3kΩ, while a code of 54R would be (lookup[54]=357 * 0.01) = 3.57Ω.
            </div>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>);
  }

  const tabs = [
    { menuItem: '3 Digit EIA', render: () => <Tab.Pane>{render3DigitEIA()}</Tab.Pane> },
    { menuItem: '4 Digit EIA', render: () => <Tab.Pane>{render4DigitEIA()}</Tab.Pane> },
    { menuItem: 'EIA-96', render: () => <Tab.Pane>{renderEIA96()}</Tab.Pane> },
  ]

  return (
    <div className="smd-resistor-code-calculator">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.smdResistanceCalc', "SMD Resistor Code Calculator")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.tool.smdResistanceCalc.title', 'SMD Resistor Code Calculator')}</h1>
      <Form>
        <Segment>
          <Tab panes={tabs} onTabChange={handleTabChange} activeIndex={activeTab}></Tab>
        </Segment>
      </Form>
    </div>
  );
}
