import React, { Component } from "react";
import _ from "underscore";
import { Layout } from './Layout';
import {ErrorContext} from '../common/ErrorContext';
import { DEFAULT_FONT } from '../common/Types';

import { HandleBinaryResponse } from "../common/handleResponse.js";
import { Button, Icon, Form, Input, Checkbox, Table, Image, Dropdown } from "semantic-ui-react";

export class PrintLabels extends Component {
  static displayName = PrintLabels.name;

  constructor(props) {
    super(props);
    const printPreferences = JSON.parse(
      localStorage.getItem("printPreferences")
    ) || {
      lastLabelName: "30277",
      lastLabelSource: 0,
      lastFont: DEFAULT_FONT
    };

    const printLabelHistory =
      JSON.parse(localStorage.getItem("printLabelHistory")) || [];
    var quantities = [];
    for (var i = 1; i <= 10; i++)
      quantities.push({ key: i, value: i, text: i.toString() });

    this.state = {
      loading: false,
      printPreferences,
      printLabelHistory,
      lines: [],
      quantity: 1,
      imgBase64: "",
      labelName: printPreferences.lastLabelName || "30277",
      labelSource: printPreferences.lastLabelSource || 0,
      font: printPreferences.lastFont || DEFAULT_FONT,
      quantities,
      labelPositions: [
        {
          key: 1,
          value: 0,
          text: "Left"
        },
        {
          key: 2,
          value: 1,
          text: "Right"
        },
        {
          key: 3,
          value: 2,
          text: "Center"
        }
      ],
      labelNames: [
        {
          key: 1,
          value: "30277",
          text: '30277 (Dual 9/16" x 3 7/16")',
          defaults: {
            label: 1, // use label 1,
            fontSizes: [16, 7, 16],
            topMargins: [0, 10, 0],
            isBarcodes: [false, false, true],
            fonts: [DEFAULT_FONT]
          }
        },
        {
          key: 2,
          value: "30346",
          text: '30346 (1/2" x 1 7/8")',
          defaults: {
            label: 2, // use label 2, there is no label 1,
            fontSizes: [16, 6, 16],
            topMargins: [0, 10, 0],
            isBarcodes: [false, false, true],
            fonts: [DEFAULT_FONT]
          }
        }
      ],
      labelSources: [
        {
          key: 1,
          value: 0,
          text: "Auto"
        },
        {
          key: 2,
          value: 1,
          text: "Left"
        },
        {
          key: 3,
          value: 2,
          text: "Right"
        }
      ],
      fonts: [
        {
          key: 1,
          value: 0,
          text: "Loading"
        }
      ]
    };
    this.onSubmit = this.onSubmit.bind(this);
    this.handlePreview = this.handlePreview.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleLineChange = this.handleLineChange.bind(this);
    this.handleAdd = this.handleAdd.bind(this);
    this.handleReset = this.handleReset.bind(this);
    this.handleLoad = this.handleLoad.bind(this);
    this.loadFonts = this.loadFonts.bind(this);
  }

  componentDidMount() {
    this.loadFonts();
  }

  async loadFonts() {
    const response = await fetch("print/fonts", {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });
    const data = await response.json();
    const newFonts = data.map((l, k) => {
      return {
        key: k,
        value: l,
        text: l
      };
    });
    const selectedFont = _.find(newFonts, (x) => x && x.text === DEFAULT_FONT);
    this.setState({
      loading: false,
      fonts: newFonts,
      font: selectedFont.value
    });
  }

  async onSubmit(e) {
    const { lines, labelSource, labelName, quantity, printLabelHistory } =
      this.state;
    this.setState({ loading: true });
    const maxLineHistory = 20;

    const entry = {
      date: new Date(),
      // dereference and copy
      lines: JSON.parse(JSON.stringify(lines))
    };
    var newPrintLabelHistory = printLabelHistory;
    if (
      _.filter(printLabelHistory, (f) => _.isEqual(f.lines, entry.lines))
        .length === 0
    ) {
      printLabelHistory.push(entry);
      newPrintLabelHistory = printLabelHistory.slice(
        Math.max(printLabelHistory.length - maxLineHistory, 0)
      );
      localStorage.setItem(
        "printLabelHistory",
        JSON.stringify(newPrintLabelHistory)
      );
    }

    const request = {
      showDiagnostic: false,
      labelName,
      labelSource,
      lines: lines.map((l) => {
        return {
          label: Number.parseInt(l.label),
          content: l.content,
          fontSize: Number.parseInt(l.fontSize),
          position: Number.parseInt(l.position),
          font: l.font,
          margin: {
            top: Number.parseInt(l.topMargin),
            left: Number.parseInt(l.leftMargin)
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: false
    };

    for (var i = 0; i < quantity; i++) {
      await fetch("print/custom", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
      });
    }

    this.setState({
      loading: false,
      printLabelHistory: newPrintLabelHistory,
      quantity: 1
    });
  }

  async handlePreview(e) {
    e.preventDefault();
    e.stopPropagation();
    const { lines, labelSource, labelName } = this.state;

    const request = {
      showDiagnostic: true,
      labelName,
      labelSource,
      lines: lines.map((l) => {
        return {
          label: Number.parseInt(l.label),
          content: l.content,
          fontSize: Number.parseInt(l.fontSize),
          position: Number.parseInt(l.position),
          font: l.font,
          margin: {
            top: Number.parseInt(l.topMargin),
            left: Number.parseInt(l.leftMargin)
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: true
    };

    await fetch("print/custom", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    })
      .then(HandleBinaryResponse)
      .catch(HandleBinaryResponse)
      .then((response) => {
        const base64 = this.arrayBufferToBase64(response);
        this.setState({ imgBase64: base64 }); 
      });
  }

  arrayBufferToBase64(buffer) {
    let binary = "";
    let bytes = [].slice.call(new Uint8Array(buffer));

    bytes.forEach((b) => (binary += String.fromCharCode(b)));

    return btoa(binary);
  }

  handleChange(e, control) {
    const { labelName, labelSource, font, printPreferences, quantity } =
      this.state;
    let newLabelName = labelName;
    let newLabelSource = labelSource;
    let newFont = font;
    let newQuantity = quantity;
    switch (control.name) {
      case "labelName":
        newLabelName = control.value;
        localStorage.setItem(
          "printPreferences",
          JSON.stringify({ ...printPreferences, lastLabelName: control.value })
        );
        break;
      case "labelSource":
        newLabelSource = control.value;
        localStorage.setItem(
          "printPreferences",
          JSON.stringify({
            ...printPreferences,
            lastLabelSource: control.value
          })
        );
        break;
      case "quantity":
        newQuantity = control.value;
        break;
      case "font":
        newFont = control.value;
        localStorage.setItem(
          "printPreferences",
          JSON.stringify({ ...printPreferences, font: control.value })
        );
        break;
      default:
        break;
    }
    this.setState({
      labelName: newLabelName,
      labelSource: newLabelSource,
      font: newFont,
      printPreferences,
      quantity: newQuantity
    });
  }

  handleLineChange(e, control, line) {
    const { lines } = this.state;
    switch (control.name) {
      case "barcode":
        line[control.name] = control.checked || false;
        break;
      default:
        line[control.name] = control.value;
        break;
    }
    this.setState({ lines });
  }

  handleLoad(e, entry) {
    e.preventDefault();
    e.stopPropagation();
    // dereference and copy
    const newLines = JSON.parse(JSON.stringify(entry.lines));
    // ensure all fields are populated, even for older history that is missing fields
    const formatLines = newLines.map((x) => {
      return {
        id: x.id,
        label: x.label,
        content: x.content || "",
        fontSize: x.fontSize || 16,
        position: x.position || 2,
        topMargin: x.topMargin || 0,
        leftMargin: x.leftMargin || 0,
        font: x.font || DEFAULT_FONT,
        barcode: x.barcode || false
      };
    });
    this.setState({ lines: formatLines });
  }

  handleAdd(e) {
    e.preventDefault();
    e.stopPropagation();
    const { lines, labelNames, labelName } = this.state;
    const label = _.find(labelNames, (x) => x && x.value === labelName);
    const lastLine = _.last(lines);
    const newLine = {
      id: lines.length + 1,
      label: (lastLine && lastLine.label) || (label && label.defaults.label),
      content: "",
      fontSize:
        label &&
        label.defaults.fontSizes.length > lines.length &&
        (label.defaults.fontSizes[lines.length] || 16),
      position: 2,
      topMargin:
        label &&
        label.defaults.topMargins.length >= lines.length &&
        (label.defaults.topMargins[lines.length] || 0),
      leftMargin: 0,
      font:
        label &&
        label.defaults.fonts.length > lines.length &&
        (label.defaults.fonts[lines.length] || DEFAULT_FONT),
      barcode:
        label &&
        label.defaults.isBarcodes.length >= lines.length &&
        (label.defaults.isBarcodes[lines.length] || false)
    };
    lines.push(newLine);
    this.setState({ lines });
  }

  handleReset(e) {
    e.preventDefault();
    e.stopPropagation();
    this.setState({ lines: [] });
  }

  removeLine(e, line, index) {
    e.preventDefault();
    e.stopPropagation();
    const { lines } = this.state;
    lines.splice(lines.indexOf(line), 1);
    this.setState({ lines });
  }

  render() {
    const {
      labelName,
      labelNames,
      labelPositions,
      labelSource,
      labelSources,
      fonts,
      quantity,
      quantities,
      lines,
      loading,
      imgBase64,
      printLabelHistory
    } = this.state;
    return (
      <div>
        <h1>Print Label</h1>
        <p>
          Print custom multi-line labels for your storage bins.
          <br />
          Print history is kept so you can reuse templates for your labels.
        </p>
        <Form onSubmit={this.onSubmit} loading={loading}>
          <Form.Group>
            <Form.Dropdown
              label="Label Type"
              placeholder="30277"
              selection
              value={labelName}
              options={labelNames}
              onChange={this.handleChange}
              name="labelName"
            />
            <Form.Dropdown
              label="Paper Source"
              placeholder="Auto"
              selection
              value={labelSource}
              options={labelSources}
              onChange={this.handleChange}
              name="labelSource"
            />
          </Form.Group>
          <Button onClick={this.handleAdd}>Add line</Button>
          <Button onClick={this.handleReset}>Reset</Button>

          <Table compact celled size="small">
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell>Label #</Table.HeaderCell>
                <Table.HeaderCell>Text</Table.HeaderCell>
                <Table.HeaderCell>FontSize</Table.HeaderCell>
                <Table.HeaderCell>Alignment</Table.HeaderCell>
                <Table.HeaderCell>Top Margin</Table.HeaderCell>
                <Table.HeaderCell>Left Margin</Table.HeaderCell>
                <Table.HeaderCell>Font</Table.HeaderCell>
                <Table.HeaderCell>Is Barcode</Table.HeaderCell>
                <Table.HeaderCell></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {lines.map((l, k) => (
                <Table.Row key={k}>
                  <Table.Cell>
                    <Input
                      name="label"
                      value={l.label}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Input
                      name="content"
                      value={l.content}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Input
                      name="fontSize"
                      value={l.fontSize}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Dropdown
                      name="position"
                      placeholder="Center"
                      selection
                      value={l.position}
                      options={labelPositions}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Input
                      name="topMargin"
                      value={l.topMargin}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Input
                      name="leftMargin"
                      value={l.leftMargin}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Dropdown
                      name="font"
                      selection
                      value={l.font}
                      options={fonts}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Checkbox
                      toggle
                      name="barcode"
                      checked={l.barcode}
                      onChange={(e, c) => this.handleLineChange(e, c, l)}
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Button
                      circular
                      size="mini"
                      icon="delete"
                      title="Delete"
                      onClick={(e) => this.removeLine(e, l, k)}
                    />
                  </Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table>
          <Form.Group>
            <Form.Dropdown
              label="Quantity"
              placeholder="1"
              selection
              value={quantity}
              options={quantities}
              onChange={this.handleChange}
              name="quantity"
            />
          </Form.Group>
          <Button onClick={this.handlePreview}>
            <Icon name="eye" /> Preview
          </Button>
          <Button primary>
            <Icon name="print" /> Print
          </Button>
          <div>
            {imgBase64 && (
              <Image
                label={{
                  color: "grey",
                  className: "transparent",
                  content: "Preview",
                  icon: "eye",
                  ribbon: true,
                  size: "tiny"
                }}
                name="previewImage"
                src={`data:image/png;base64,${imgBase64}`}
                size="medium"
                style={{ marginTop: "20px" }}
              />
            )}
          </div>
          <div style={{ marginTop: "20px" }}>
            <h2>Print History</h2>
            <Table compact celled size="small">
              <Table.Body>
                {_.sortBy(
                  printLabelHistory.map((l, k) => (
                    <Table.Row key={k}>
                      <Table.Cell>
                        {new Date(l.date).toLocaleString("en-US")}
                      </Table.Cell>
                      <Table.Cell>
                        {l.lines.length > 0 ? l.lines[0].content : ""}
                      </Table.Cell>
                      <Table.Cell>
                        {l.lines.length > 1 ? l.lines[1].content : ""}
                      </Table.Cell>
                      <Table.Cell>
                        {l.lines.length > 2 ? l.lines[2].content : ""}
                      </Table.Cell>
                      <Table.Cell>
                        <Button onClick={(e) => this.handleLoad(e, l)}>
                          <Icon name="folder open" /> Load
                        </Button>
                      </Table.Cell>
                    </Table.Row>
                  )),
                  "date"
                ).reverse()}
              </Table.Body>
            </Table>
          </div>
        </Form>
      </div>
    );
  }
}
