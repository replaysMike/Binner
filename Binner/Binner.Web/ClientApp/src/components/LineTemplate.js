import React, { Component } from "react";
import { Link } from "react-router-dom";
import { Input, Checkbox, Form, Segment, Header, Dropdown, Popup } from "semantic-ui-react";
import { createImportSpecifier } from "typescript";

const TimeoutLength = 1500;

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
    this.open = this.open.bind(this);
    this.handleClose = this.handleClose.bind(this);
  }

  open() {
    this.setState({ isOpen: true });
  }

  handleClose = () => {
    // delay the close by a set timeout, so links can be clicked
    this.timeout = setTimeout(() => {
      this.setState({ isOpen: false });
      clearTimeout(this.timeout);
    }, TimeoutLength);
  };

  render() {
    const { line, fonts, title, color, font } = this.props;
    const { positionOptions } = this.state;

    return (
      <Segment color={color} {...this.props}>
        <Header dividing as="h3">
          {title}
        </Header>
        <Form.Group>
          <Popup
            disabled={false}
            open={this.state.isOpen}
            onOpen={this.open}
            onClose={this.handleClose}
            content={
              <p>
                Template can reference any part field, example: &#123;partNumber&#125;, &#123;description&#125;, &#123;manufacturer&#125;, &#123;location&#125;,
                &#123;binNumber&#125;, &#123;cost&#125; etc. See{" "}
                <a href="https://github.com/replaysMike/Binner/wiki/Printing-Configuration#partlabeltemplate-line-options" target="_blank" rel="noreferrer">
                  Wiki
                </a>{" "}
                for all available tags.
              </p>
            }
            trigger={
              <Form.Field width={4}>
                <label>Content</label>
                <Input
                  name={this.props.name + "Content"}
                  className="labeled"
                  placeholder="{partNumber}"
                  value={line.content || ""}
                  onChange={this.props.onChange}
                />
              </Form.Field>
            }
          />

          <Form.Field width={4}>
            <label>Font</label>
            <Dropdown name={this.props.name + "FontName"} placeholder={font} selection value={line.fontName} options={fonts} onChange={this.props.onChange} />
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
          <Popup
            content={<p>Text size will be automatically determined.</p>}
            trigger={
              <Form.Field width={3}>
                <Checkbox
                  name={this.props.name + "AutoSize"}
                  label="Auto size text"
                  className="labeled"
                  checked={line.autoSize}
                  onChange={this.props.onChange}
                />
              </Form.Field>
            }
          />

          <Popup
            content={<p>Render the text as all upper-case characters.</p>}
            trigger={
              <Form.Field width={3}>
                <Checkbox
                  name={this.props.name + "UpperCase"}
                  label="UpperCase Text"
                  className="labeled"
                  checked={line.upperCase}
                  onChange={this.props.onChange}
                />
              </Form.Field>
            }
          />
          <Popup
            content={<p>Render the text as all lower-case characters.</p>}
            trigger={
              <Form.Field width={3}>
                <Checkbox
                  name={this.props.name + "LowerCase"}
                  label="LowerCase Text"
                  className="labeled"
                  checked={line.lowerCase}
                  onChange={this.props.onChange}
                />
              </Form.Field>
            }
          />
          <Popup
            content={<p>Render the Content value encoded as a barcode.</p>}
            trigger={
              <Form.Field width={3}>
                <Checkbox name={this.props.name + "Barcode"} label="Barcode" className="labeled" checked={line.barcode} onChange={this.props.onChange} />
              </Form.Field>
            }
          />
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
          <Popup
            content={<p>Rotate the text in degrees. Example: 90</p>}
            trigger={
              <Form.Field width={3}>
                <label>Rotate Degrees</label>
                <Input name={this.props.name + "Rotate"} className="labeled" placeholder="0" value={line.rotate || ""} onChange={this.props.onChange}></Input>
              </Form.Field>
            }
          />
        </Form.Group>
      </Segment>
    );
  }
}
