import React, { Component } from "react";
import { Input, Checkbox, Form, Segment, Header, Dropdown } from "semantic-ui-react";

export default class LineTemplate extends Component {
  constructor(props) {
    super(props);
    this.state = {
      positionOptions: [
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
      ]
    };
  }

  render() {
    const { line, fonts, title, color, defaultFont } = this.props;
    const { positionOptions } = this.state;

    return (
      <Segment color={color} {...this.props}>
        <Header dividing as="h3">
          {title}
        </Header>
        <Form.Group>
          <Form.Field width={4}>
            <label>Content</label>
            <Input
              name={this.props.name + "Content"}
              className="labeled"
              placeholder="{partNumber}"
              value={line.content || ""}
              onChange={this.props.onChange}
            ></Input>
          </Form.Field>
          <Form.Field width={4}>
            <label>Font</label>
            <Dropdown
              name={this.props.name + "FontName"}
              placeholder={defaultFont}
              selection
              value={line.fontName}
              options={fonts}
              onChange={this.props.onChange}
            />
          </Form.Field>
          <Form.Field width={2}>
            <label>Font Size</label>
            <Input name={this.props.name + "FontSize"} className="labeled" placeholder="16" value={line.fontSize || ""} onChange={this.props.onChange}></Input>
          </Form.Field>
          <Form.Field width={2}>
            <label>Font Color</label>
            <Input name={this.props.name + "Color"} className="labeled" placeholder="#000000" value={line.color || ""} onChange={this.props.onChange}></Input>
          </Form.Field>
          <Form.Field width={3}>
            <label>Text Position</label>
            <Dropdown
              name={this.props.name + "Position"}
              placeholder="Center"
              selection
              value={line.position}
              options={positionOptions}
              onChange={this.props.onChange}
            />
          </Form.Field>
        </Form.Group>
        <Form.Group>
          <Form.Field width={3}>
            <Checkbox
              name={this.props.name + "AutoSize"}
              label="Auto size text"
              className="labeled"
              placeholder="true"
              checked={line.autoSize}
              onChange={this.props.onChange}
            />
          </Form.Field>
          <Form.Field width={3}>
            <Checkbox
              name={this.props.name + "UpperCase"}
              label="UpperCase Text"
              className="labeled"
              placeholder="true"
              checked={line.upperCaseText}
              onChange={this.props.onChange}
            />
          </Form.Field>
          <Form.Field width={3}>
            <Checkbox
              name={this.props.name + "Barcode"}
              label="Barcode"
              className="labeled"
              placeholder="true"
              checked={line.barcode}
              onChange={this.props.onChange}
            />
          </Form.Field>
        </Form.Group>
        <Form.Group>
          <Form.Field width={3}>
            <label>Margin Left</label>
            <Input
              name={this.props.name + "MarginLeft"}
              className="labeled"
              placeholder="0"
              value={line.marginLeft || ""}
              onChange={this.props.onChange}
            ></Input>
          </Form.Field>
          <Form.Field width={3}>
            <label>Margin Top</label>
            <Input name={this.props.name + "MarginTop"} className="labeled" placeholder="0" value={line.marginTop || ""} onChange={this.props.onChange}></Input>
          </Form.Field>
          <Form.Field width={3}>
            <label>Rotate Degrees</label>
            <Input name={this.props.name + "Rotate"} className="labeled" placeholder="0" value={line.rotate || ""} onChange={this.props.onChange}></Input>
          </Form.Field>
        </Form.Group>
      </Segment>
    );
  }
}
