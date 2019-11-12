import React, { Component } from 'react';
import _ from 'underscore';
import { Button, Icon, Form, Input, Checkbox, Table, Image, Dropdown, Segment } from 'semantic-ui-react';

export class PrintLabels extends Component {
  static displayName = PrintLabels.name;

  constructor(props) {
    super(props);
    this.state = {
      loading: false,
      lines: [],
      imgBase64: '',
      labelName: '30277',
      labelSource: 0,
      labelPositions: [
        {
          key: 1,
          value: 0,
          text: 'Left',
        },
        {
          key: 2,
          value: 1,
          text: 'Right',
        },
        {
          key: 3,
          value: 2,
          text: 'Center',
        },
      ],
      labelNames: [
        {
          key: 1,
          value: '30277',
          text: '30277 (Dual 9/16" x 3 7/16")',
        },
        {
          key: 2,
          value: '30346',
          text: '30346 (1/2" x 1 7/8")',
        },
      ],
      labelSources: [
        {
          key: 1,
          value: 0,
          text: 'Auto',
        },
        {
          key: 2,
          value: 1,
          text: 'Left',
        },
        {
          key: 3,
          value: 2,
          text: 'Right',
        },
      ]
    };
    this.onSubmit = this.onSubmit.bind(this);
    this.handlePreview = this.handlePreview.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLineChange = this.handleLineChange.bind(this);
    this.handleAdd = this.handleAdd.bind(this);
  }

  componentDidMount() {
  }

  async onSubmit(e) {
    const { lines, labelSource, labelName } = this.state;
    this.setState({ loading: true });

    const request = {
      showDiagnostic: false,
      labelName,
      labelSource,
      lines: lines.map(l => {
        return {
          label: Number.parseInt(l.label) || 1,
          content: l.content,
          fontSize: Number.parseInt(l.fontSize) || 16,
          position: Number.parseInt(l.position) || 2,
          margin: {
            top: Number.parseInt(l.topMargin) || 0,
            left: Number.parseInt(l.leftMargin) || 0
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: false
    };

    const response = await fetch('print/custom', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });

    this.setState({ loading: false });
  }

  async handlePreview(e) {
    e.preventDefault();
    e.stopPropagation();
    const { lines, labelSource, labelName } = this.state;

    const request = {
      showDiagnostic: true,
      labelName,
      labelSource,
      lines: lines.map(l => {
        return {
          label: Number.parseInt(l.label) || 1,
          content: l.content,
          fontSize: Number.parseInt(l.fontSize) || 16,
          position: Number.parseInt(l.position) || 2,
          margin: {
            top: Number.parseInt(l.topMargin) || 0,
            left: Number.parseInt(l.leftMargin) || 0
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: true
    };

    const response = await fetch('print/custom', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    const arrayBuffer = await response.arrayBuffer();
    const base64 = this.arrayBufferToBase64(arrayBuffer);
    this.setState({ imgBase64: base64 });
  }

  arrayBufferToBase64(buffer) {
    let binary = '';
    let bytes = [].slice.call(new Uint8Array(buffer));

    bytes.forEach((b) => binary += String.fromCharCode(b));

    return btoa(binary);
  }

  handleChange(e, control) {
    const { labelName, labelSource } = this.state;
    switch (control.name) {
      case 'labelName':
        labelName = control.value;
        break;
      case 'labelSource':
        labelSource = control.value;
        break;
    }
    this.setState({ labelName, labelSource });
  }

  handleLineChange(e, control, line) {
    const { lines } = this.state;
    line[control.name] = control.value;
    // const existingLine = _.find(lines, { lineId: line.lineId });
    this.setState({ lines });
  }

  handleAdd(e) {
    e.preventDefault();
    e.stopPropagation();
    const { lines } = this.state;
    const newLine = {
      id: 1,
      label: 1,
      content: '',
      fontSize: 16,
      position: 2,
      topMargin: 0,
      leftMargin: 0,
      barcode: false
    };
    lines.push(newLine);
    this.setState({ lines });
  }

  render() {
    const { labelName, labelNames, labelPositions, labelSource, labelSources, lines, loading, imgBase64 } = this.state;
    return (
      <div>
        <h1>Print Label</h1>
        <Form onSubmit={this.onSubmit} loading={loading}>
          <Form.Group>
            <Form.Dropdown label='Label Type' placeholder='30277' selection value={labelName} options={labelNames} onChange={this.handleChange} name='labelName' />
            <Form.Dropdown label='Paper Source' placeholder='Auto' selection value={labelSource} options={labelSources} onChange={this.handleChange} name='labelSource' />
          </Form.Group>
          <Button onClick={this.handleAdd}>Add line</Button>

          <Table compact celled size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell>Label #</Table.HeaderCell>
                <Table.HeaderCell>Text</Table.HeaderCell>
                <Table.HeaderCell>FontSize</Table.HeaderCell>
                <Table.HeaderCell>Alignment</Table.HeaderCell>
                <Table.HeaderCell>Top Margin</Table.HeaderCell>
                <Table.HeaderCell>Left Margin</Table.HeaderCell>
                <Table.HeaderCell>Is Barcode</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {lines.map((l, k) =>
                <Table.Row key={k}>
                  <Table.Cell><Input name='label' value={l.label} onChange={(e, c) => this.handleLineChange(e, c, l)} /></Table.Cell>
                  <Table.Cell><Input name='content' value={l.content} onChange={(e, c) => this.handleLineChange(e, c, l)} /></Table.Cell>
                  <Table.Cell><Input name='fontSize' value={l.fontSize} onChange={(e, c) => this.handleLineChange(e, c, l)} /></Table.Cell>
                  <Table.Cell>
                    <Dropdown name='position' placeholder='Center' selection value={l.position} options={labelPositions} onChange={(e, c) => this.handleLineChange(e, c, l)} />
                  </Table.Cell>
                  <Table.Cell><Input name='topMargin' value={l.topMargin} onChange={(e, c) => this.handleLineChange(e, c, l)} /></Table.Cell>
                  <Table.Cell><Input name='leftMargin' value={l.leftMargin} onChange={(e, c) => this.handleLineChange(e, c, l)} /></Table.Cell>
                  <Table.Cell><Checkbox toggle name='barcode' checked={l.barcode} onChange={e => this.handleLineChange(e, l)} /></Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
          <Button onClick={this.handlePreview}><Icon name='eye' /> Preview</Button>
          <Button primary><Icon name='print' /> Print</Button>
          <div>
            {imgBase64 && <Image label={{
              color: 'grey',
              content: 'Preview',
              icon: 'eye',
              ribbon: true,
              size: 'tiny'
            }} name='previewImage' src={`data:image/png;base64,${imgBase64}`} size='medium' style={{ marginTop: '20px' }} />}
          </div>
        </Form>
      </div>
    );
  }
}
