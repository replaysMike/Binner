import React, { Component } from 'react';
import _ from 'underscore';
import { Label, Segment, Form, Dropdown, Statistic } from 'semantic-ui-react';
import { encodeResistance } from '../../common/Utils';

export class ResistorColorCodeCalculator extends Component {
  static displayName = ResistorColorCodeCalculator.name;

  constructor(props) {
    super(props);
    const resistorColorCodePreferences = JSON.parse(localStorage.getItem('resistorColorCodePreferences')) || {
      lastResistorBands: 4,
      lastBands: [0, 0, 0, 0.1, null, null]
    };
    this.colors = [
      { key: 0, value: 0, order: 2, text: 'Black', color: 'black', label: { color: 'black', empty: true, circular: true }, tolerance: 0, ppm: 0 },
      { key: 1, value: 1, order: 3, text: 'Brown', color: '#654b2f', label: { color: 'brown', empty: true, circular: true }, tolerance: 0.01, ppm: 100 },
      { key: 2, value: 2, order: 4, text: 'Red', color: 'red', label: { color: 'red', empty: true, circular: true }, tolerance: 0.02, ppm: 50 },
      { key: 3, value: 3, order: 5, text: 'Orange', color: 'orange', label: { color: 'orange', empty: true, circular: true }, tolerance: 0, ppm: 15 },
      { key: 4, value: 4, order: 6, text: 'Yellow', color: 'yellow', label: { color: 'yellow', empty: true, circular: true }, tolerance: 0, ppm: 25 },
      { key: 5, value: 5, order: 7, text: 'Green', color: 'green', label: { color: 'green', empty: true, circular: true }, tolerance: 0.005, ppm: 0 },
      { key: 6, value: 6, order: 8, text: 'Blue', color: '#2d3daf', label: { color: 'blue', empty: true, circular: true }, tolerance: 0.0025, ppm: 10 },
      { key: 7, value: 7, order: 9, text: 'Violet', color: '#9032c6', label: { color: 'violet', empty: true, circular: true }, tolerance: 0.001, ppm: 5 },
      { key: 8, value: 8, order: 10, text: 'Grey', color: '#999', label: { color: 'grey', empty: true, circular: true }, tolerance: 0.0005, ppm: 0 },
      { key: 9, value: 9, order: 11, text: 'White', color: '#fafafa', label: { empty: true, circular: true }, tolerance: 0, ppm: 0 },
      { key: 10, value: 0.1, order: 0, text: 'Gold', color: 'gold', label: { color: 'yellow', empty: true, circular: true }, tolerance: 0.05, ppm: 0 },
      { key: 11, value: 0.01, order: 1, text: 'Silver', color: 'silver', label: { color: 'grey', empty: true, circular: true }, tolerance: 0.1, ppm: 0 },
    ];
    const resistorBands = resistorColorCodePreferences.lastResistorBands || 4;
    const tolerance = _.find(this.colors, x => x.value === resistorColorCodePreferences.lastBands[resistorBands - 1]).tolerance;
    const ppm = 0;
    this.state = {
      resistorColorCodePreferences,
      bands: [],
      resistorBands,
      tolerance,
      ppm
    };
    this.bandOptions = [
      { key: 1, text: '4 Band', value: 4 },
      { key: 2, text: '5 Band', value: 5 },
      { key: 3, text: '6 Band', value: 6 },
    ];
    this.handleChangeBands = this.handleChangeBands.bind(this);
    this.handleChangeBandValue = this.handleChangeBandValue.bind(this);
    this.calculateResistorValue = this.calculateResistorValue.bind(this);
    this.setBands = this.setBands.bind(this);
    this.renderCanvas = this.renderCanvas.bind(this);
  }

  componentDidMount() {
    this.setBands(this.state.resistorBands);
  }

  componentDidUpdate() {
    this.renderCanvas();
  }

  handleChangeBands(e, control) {
    const { resistorColorCodePreferences } = this.state;
    switch (control.name) {
      case 'resistorBands':
        this.setState({ resistorBands: control.value });
        this.setBands(control.value);
        resistorColorCodePreferences.lastResistorBands = control.value;
        localStorage.setItem('resistorColorCodePreferences', JSON.stringify(resistorColorCodePreferences));
        break;
      default:
        break;
    }
  }

  handleChangeBandValue(e, control, band, index) {
    const { bands, resistorBands, tolerance, ppm, resistorColorCodePreferences } = this.state;
    let toleranceValue = tolerance;
    let ppmValue = ppm;
    switch (control.name) {
      case 'bandValue':
        band.value = control.value;
        resistorColorCodePreferences.lastBands[index] = control.value;
        if ((band.key === resistorBands && resistorBands < 6) || (band.key === resistorBands - 1 && resistorBands === 6))
          toleranceValue = Number.parseFloat(_.find(this.colors, x => x.value === band.value).tolerance);
        if (band.key === resistorBands && resistorBands === 6)
          ppmValue = Number.parseInt(_.find(this.colors, x => x.value === band.value).ppm);
        localStorage.setItem('resistorColorCodePreferences', JSON.stringify(resistorColorCodePreferences));
        this.setState({ bands, tolerance: toleranceValue, ppm: ppmValue });
        break;
      default:
        break;
    }
  }

  setBands(resistorBands) {
    const { resistorColorCodePreferences } = this.state;
    let bands = [];
    const bandOptions = _.filter(this.colors, i => i.key < 10).map(i => {
      return {
        key: i.key,
        value: i.value,
        order: i.order,
        text: i.text,
        description: i.value,
        label: i.label,
        tolerance: i.tolerance,
        ppm: i.ppm
      }
    });
    const multiplierOptions = _.filter(this.colors, i => i.key < 12).map(i => {
      return {
        key: i.key,
        value: i.value,
        order: i.order,
        text: i.text,
        description: (i.value >= 1 || i.value === 0) ? `x${encodeResistance(Math.pow(10, i.value))}` : `x${encodeResistance(i.value, 2)}`,
        label: i.label,
        tolerance: i.tolerance,
        ppm: i.ppm
      }
    });
    const toleranceOptions = _.sortBy(_.filter(this.colors, i => i.tolerance !== 0).map(i => {
      return {
        key: i.key,
        value: i.value,
        order: i.order,
        text: i.text,
        description: `±${(i.tolerance * 100.0) < 1 ? (i.tolerance * 100.0).toFixed(2) : (i.tolerance * 100.0)}%`,
        label: i.label,
        tolerance: i.tolerance,
        ppm: i.ppm
      }
    }), x => x.order);
    const ppmOptions = _.filter(this.colors, i => i.ppm > 0).map(i => {
      return {
        key: i.key,
        value: i.value,
        order: i.order,
        text: i.text,
        description: `${i.ppm} ppm`,
        label: i.label,
        tolerance: i.tolerance,
        ppm: i.ppm
      }
    });
    for (let i = 1; i <= resistorBands; i++) {
      if ((resistorBands < 6 && i === resistorBands - 1) || (resistorBands === 6 && i === resistorBands - 2))
        bands.push({ key: i, text: '', value: resistorColorCodePreferences.lastBands[i - 1] || 0, placeholder: 'Select a Multiplier', label: 'Multiplier', options: multiplierOptions });
      else if ((resistorBands < 6 && i === resistorBands) || (resistorBands === 6 && i === resistorBands - 1))
        bands.push({ key: i, text: '', value: resistorColorCodePreferences.lastBands[i - 1] || 0.1, placeholder: 'Select a Tolerance', label: 'Tolerance', options: toleranceOptions });
      else if ((resistorBands === 6 && i === resistorBands))
        bands.push({ key: i, text: '', value: resistorColorCodePreferences.lastBands[i - 1] || 7, placeholder: 'Select a Color', label: 'PPM', options: ppmOptions });
      else
        bands.push({ key: i, text: '', value: resistorColorCodePreferences.lastBands[i - 1] || 0, placeholder: 'Select a Color', label: `Band ${i}`, options: bandOptions });
    }
    this.setState({ bands });
  }

  calculateResistorValue() {
    const { bands, resistorBands } = this.state;
    if (bands && bands.length > 0) {
      switch (resistorBands) {
        case 4:
          return Number.parseInt(bands[0].value.toString() + bands[1].value.toString()) * Math.pow(10, bands[2].value);
        case 5:
          return Number.parseInt(bands[0].value.toString() + bands[1].value.toString() + bands[2].value.toString()) * Math.pow(10, bands[3].value);
        case 6:
          return Number.parseInt(bands[0].value.toString() + bands[1].value.toString() + bands[2].value.toString()) * Math.pow(10, bands[3].value);
        default:
          return 0;
      }
    }
    return 0;
  }

  renderCanvas() {
    const { bands, resistorBands, tolerance } = this.state;
    // draw a resistor with color bands
    const canvas = this.refs.canvas;
    const g = canvas.getContext('2d');
    g.strokeStyle = '#999';
    g.lineWidth = 5;
    g.beginPath();
    g.moveTo(0, 25);
    g.lineTo(300, 25);
    g.closePath();
    g.stroke();
    if (tolerance === 0.01)
      g.fillStyle = '#5e7dce';
    else
      g.fillStyle = '#b0a180';
    g.fillRect(50, 0, 200, 50);
    let spacing = 40;
    if (resistorBands === 5)
      spacing = 33;
    if (resistorBands === 6)
      spacing = 28;
    const x = 50;
    for (let i = 1; i <= resistorBands; i++) {
      g.lineWidth = 10;
      const color = _.find(this.colors, x => x.value === bands[i - 1].value).color;
      if (color === 'gold') {
        const grd = g.createRadialGradient(45.100, 10.900, 5.000, 147.000, 85.000, 150.000);
        grd.addColorStop(0.000, 'rgba(255, 255, 255, 1.000)');
        grd.addColorStop(0.134, 'rgba(255, 255, 172, 1.000)');
        grd.addColorStop(0.234, 'rgba(209, 180, 100, 1.000)');
        grd.addColorStop(0.309, 'rgba(165, 124, 41, 1.000)');
        grd.addColorStop(0.422, 'rgba(253, 185, 49, 1.000)');
        grd.addColorStop(0.615, 'rgba(178, 134, 41, 1.000)');
        grd.addColorStop(0.786, 'rgba(232, 183, 92, 1.000)');
        grd.addColorStop(0.906, 'rgba(253, 185, 49, 1.000)');
        grd.addColorStop(1.000, 'rgba(254, 219, 55, 1.000)');
        g.strokeStyle = grd;
      } else if (color === 'silver') {
        const grd = g.createRadialGradient(35.100, 0.000, 2.500, 137.000, 80.000, 150.000);
        grd.addColorStop(0.000, 'rgba(255, 255, 255, 1.000)');
        grd.addColorStop(0.409, 'rgba(80, 80, 80, 1.000)');
        grd.addColorStop(0.506, 'rgba(40, 40, 40, 1.000)');
        grd.addColorStop(0.706, 'rgba(140, 140, 140, 1.000)');
        grd.addColorStop(1.000, 'rgba(229, 229, 229, 1.000)');
        g.strokeStyle = grd;
      } else {
        g.strokeStyle = color;
      }
      g.beginPath();
      g.moveTo(x + (i * spacing), 0);
      g.lineTo(x + (i * spacing), 50);
      g.closePath();
      g.stroke();
      g.lineWidth = 1;
      g.strokeStyle = '#6c5a34';
      g.beginPath();
      g.moveTo(x + (i * spacing) + 5, 0);
      g.lineTo(x + (i * spacing) + 5, 50);
      g.closePath();
      g.stroke();
    }
  }

  render() {
    const { resistorBands, bands, tolerance, ppm } = this.state;
    const resistanceValue = this.calculateResistorValue();
    const resistance = encodeResistance(resistanceValue, 1);
    let resistanceRangeTop = 0;
    let resistanceRangeBottom = 0;
    let resistanceRange = '';
    if (resistanceValue > 0) {
      const tolerancePerc = tolerance * 100.0;
      const toleranceDecimals = tolerancePerc < 1 ? 2 : 0;
      const valueDecimals = tolerancePerc < 1 ? 2 : 1;
      const ppmStr = ppm > 0 ? `${ppm} ppm` : '';
      resistanceRangeTop = resistanceValue + (resistanceValue * tolerance);
      resistanceRangeBottom = resistanceValue - (resistanceValue * tolerance);
      resistanceRange = `${encodeResistance(resistanceRangeBottom, valueDecimals)} - ${encodeResistance(resistanceRangeTop, valueDecimals)} (${tolerancePerc.toFixed(toleranceDecimals)}%) ${ppmStr}`;
    }
    return (
      <div>
        <h1>Resistor Color Code Calculator</h1>
        <Form>
          <Segment>
            <Form.Field>
              <label>Choose the number of bands:</label>
              <Dropdown options={this.bandOptions} value={resistorBands} placeholder='Choose bands...' onChange={this.handleChangeBands} name='resistorBands'></Dropdown>
            </Form.Field>
            <br />
            {bands.map((b, k) =>
              <Form.Field key={k}>
                <label>{b.label}</label>
                <Label empty circular color={_.find(this.colors, { value: b.value }).label.color} style={{ marginRight: '10px' }} />
                <Dropdown placeholder={b.placeholder} text={b.text} value={b.value} options={b.options} onChange={(e, control) => this.handleChangeBandValue(e, control, b, k)} name='bandValue'></Dropdown>
                <br />
              </Form.Field>
            )}
          </Segment>
          <Segment textAlign='center'>
            <Statistic>
              <Statistic.Value>{resistance}</Statistic.Value>
              <Statistic.Label>Resistance</Statistic.Label>
            </Statistic>
            <div>
              {resistanceRange}
            </div>
            <div>
              <canvas ref='canvas' width={300} height={50} />
            </div>
          </Segment>
        </Form>
      </div>
    );
  }
}
