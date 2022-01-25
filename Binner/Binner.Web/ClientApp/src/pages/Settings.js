import React, { Component } from "react";
import _ from "underscore";
import { Icon, Input, Label, Button, Form, Segment, Header } from "semantic-ui-react";
import LineTemplate from "../components/LineTemplate";
import { DEFAULT_FONT } from "../common/Types";
import { HandleJsonResponse } from "../common/handleResponse.js";

export class Settings extends Component {
  static displayName = Settings.name;

  constructor(props) {
    super(props);
    this.state = {
      fonts: [],
      settings: {
        digikey: {
          clientId: "",
          clientSecret: "",
          oAuthPostbackUrl: "",
          apiUrl: ""
        },
        mouser: {
          searchApiKey: "",
          orderApiKey: "",
          cartApiKey: "",
          apiUrl: ""
        },
        octopart: {
          apiKey: "",
          apiUrl: ""
        },
        printer: {
          printerName: "",
          partLabelSource: "",
          partLabelName: "",
          lines: [
            // line 1
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            },
            // line 2
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            },
            // line 3
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            },
            // line 4
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            }
          ],
          identifiers: [
            // identifier 1
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            },
            // identifier 2
            {
              label: 0,
              content: "",
              fontName: "",
              fontSize: 8,
              autoSize: false,
              upperCase: false,
              barcode: false,
              color: "",
              rotate: 0,
              position: 2,
              margin: {
                top: 0,
                left: 0
              }
            }
          ]
        }
      },
      loading: true,
      saveMessage: ""
    };

    this.handleChange = this.handleChange.bind(this);
    this.loadFonts = this.loadFonts.bind(this);
    this.loadSettings = this.loadSettings.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
  }

  async componentDidMount() {
    await this.loadSettings();
    await this.loadFonts();
  }

  async loadFonts() {
    await fetch("print/fonts", {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    })
      .then((response) => response.json())
      .then((data) => {
        const newFonts = data.map((l, k) => {
          return {
            key: k,
            value: l,
            text: l
          };
        });
        const selectedFont = _.find(newFonts, (x) => x && x.text === DEFAULT_FONT);
        this.setState({
          fonts: newFonts,
          font: selectedFont.value
        });
      });
  }

  async loadSettings() {
    await fetch("system/settings", {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    })
      .then(HandleJsonResponse)
      .catch(HandleJsonResponse)
      .then((data) => {
        this.setState({
          loading: false,
          settings: data
        });
      });
  }

  /**
   * Save the system settings
   *
   * @param {any} e
   */
  async onSubmit(e) {
    const { settings } = this.state;
    await fetch("system/settings", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(settings)
    })
      .then(HandleJsonResponse)
      .catch(HandleJsonResponse)
      .then((response) => {
        const saveMessage = "Saved part";
        this.setState({ saveMessage });
      });
  }

  handleChange(e, control) {
    e.preventDefault();
    e.stopPropagation();
    const { settings } = this.state;

    // redirect new values to the correct state object
    if (control.name.startsWith("digikey")) {
      settings.digikey[this.getControlInstance(control, "digikey")] = control.value;
    }
    if (control.name.startsWith("mouser")) {
      settings.mouser[this.getControlInstance(control, "mouser")] = control.value;
    }
    if (control.name.startsWith("octopart")) {
      settings.octopart[this.getControlInstance(control, "octopart")] = control.value;
    }

    // todo: find a better way to clean up state changes
    if (control.name.startsWith("printer")) {
      if (control.name.startsWith("printerLine")) {
        if (control.name.startsWith("printerLine1")) settings.printer.lines[0][this.getControlInstance(control, "printerLine1")] = control.value;
        else if (control.name.startsWith("printerLine2")) settings.printer.lines[1][this.getControlInstance(control, "printerLine2")] = control.value;
        else if (control.name.startsWith("printerLine3")) settings.printer.lines[2][this.getControlInstance(control, "printerLine3")] = control.value;
        else if (control.name.startsWith("printerLine4")) settings.printer.lines[3][this.getControlInstance(control, "printerLine4")] = control.value;
      } else if (control.name.startsWith("printerIdentifier")) {
        if (control.name.startsWith("printerIdentifier1"))
          settings.printer.identifiers[0][this.getControlInstance(control, "printerIdentifier1")] = control.value;
        else if (control.name.startsWith("printerIdentifier2"))
          settings.printer.identifiers[1][this.getControlInstance(control, "printerIdentifier2")] = control.value;
      } else {
        settings.printer[this.getControlInstance(control, "printer")] = control.value;
      }
    }

    this.setState({ settings });
  }

  getControlInstance(control, prefix) {
    let controlName = control.name.replace(prefix, "");
    return controlName.charAt(0).toLowerCase() + controlName.slice(1);
  }

  render() {
    const { settings, loading, saveMessage, fonts, font } = this.state;

    return (
      <div>
        <h1>Settings</h1>
        <p>Configure your integrations, printer configuration, as well as label part templates.</p>

        <Form onSubmit={this.onSubmit}>
          <Segment loading={loading} color="blue" raised padded>
            <Header dividing as="h3">
              Integrations
            </Header>
            <p>
              <i>
                To integrate with DigiKey, Mouser or Octopart API's you must obtain API keys for each service you wish to use.
                <br />
                Adding integrations will greatly enhance your experience.
              </i>
            </p>

            <Segment loading={loading} color="green" secondary>
              <Header dividing as="h3">
                DigiKey
              </Header>
              <Form.Field width={10}>
                <label>ClientId</label>
                <Input className="labeled" placeholder="" value={settings.digikey.clientId || ""} name="digikeyClientId" onChange={this.handleChange}></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>Client Secret</label>
                <Input
                  className="labeled"
                  placeholder=""
                  value={settings.digikey.clientSecret || ""}
                  name="digikeyClientSecret"
                  onChange={this.handleChange}
                ></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>ApiUrl</label>
                <Input
                  className="labeled"
                  placeholder="sandbox-api.digikey.com"
                  value={(settings.digikey.apiUrl || "").replace("http://", "").replace("https://", "")}
                  name="digikeyApiUrl"
                  onChange={this.handleChange}
                >
                  <Label>http://</Label>
                  <input />
                </Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>PostbackUrl</label>
                <Input
                  className="labeled"
                  placeholder="localhost:8090/Authorization/Authorize"
                  value={(settings.digikey.oAuthPostbackUrl || "").replace("http://", "").replace("https://", "")}
                  name="digikeyOAuthPostbackUrl"
                  onChange={this.handleChange}
                >
                  <Label>http://</Label>
                  <input />
                </Input>
              </Form.Field>
            </Segment>

            <Segment loading={loading} color="green" secondary>
              <Header dividing as="h3">
                Mouser
              </Header>
              <Form.Field width={10}>
                <label>Search Api Key</label>
                <Input
                  className="labeled"
                  placeholder=""
                  value={settings.mouser.searchApiKey || ""}
                  name="mouserSearchApiKey"
                  onChange={this.handleChange}
                ></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>Order Api Key</label>
                <Input
                  className="labeled"
                  placeholder=""
                  value={settings.mouser.orderApiKey || ""}
                  name="mouserOrderApiKey"
                  onChange={this.handleChange}
                ></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>Cart Api Key</label>
                <Input className="labeled" placeholder="" value={settings.mouser.cartApiKey || ""} name="mouserCartApiKey" onChange={this.handleChange}></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>Api Url</label>
                <Input
                  className="labeled"
                  placeholder="api.mouser.com"
                  value={(settings.mouser.apiUrl || "").replace("http://", "").replace("https://", "")}
                  name="mouserApiUrl"
                  onChange={this.handleChange}
                >
                  <Label>http://</Label>
                  <input />
                </Input>
              </Form.Field>
            </Segment>

            <Segment loading={loading} color="green" secondary>
              <Header dividing as="h3">
                Octopart
              </Header>
              <Form.Field width={10}>
                <label>Api Key</label>
                <Input className="labeled" placeholder="" value={settings.octopart.apiKey || ""} name="octopartApiKey" onChange={this.handleChange}></Input>
              </Form.Field>
              <Form.Field width={10}>
                <label>Api Url</label>
                <Input
                  action
                  className="labeled"
                  placeholder="api.mouser.com"
                  value={(settings.octopart.apiUrl || "").replace("http://", "").replace("https://", "")}
                  name="octopartApiUrl"
                  onChange={this.handleChange}
                >
                  <Label>http://</Label>
                  <input />
                </Input>
              </Form.Field>
            </Segment>
          </Segment>

          <Segment loading={loading} color="blue" raised padded>
            <Header dividing as="h3">
              Printer Configuration
            </Header>
            <p>
              <i>Configure your printer name as it shows up in your environment (Windows Printers or CUPS Printer Name)</i>
            </p>

            <Form.Field width={10}>
              <label>Printer Name</label>
              <Input
                className="labeled"
                placeholder="DYMO LabelWriter 450 Twin Turbo"
                value={settings.printer.printerName || ""}
                name="printerPrinterName"
                onChange={this.handleChange}
              ></Input>
            </Form.Field>
            <Form.Field width={10}>
              <label>Part Label Source</label>
              <Input
                className="labeled"
                placeholder="Right"
                value={settings.printer.partLabelSource || ""}
                name="printerPartLabelSource"
                onChange={this.handleChange}
              ></Input>
            </Form.Field>
            <Form.Field width={10}>
              <label>Part Label Name</label>
              <Input
                className="labeled"
                placeholder="30346"
                value={settings.printer.partLabelName || ""}
                name="printerPartLabelName"
                onChange={this.handleChange}
              ></Input>
            </Form.Field>

            <Segment loading={loading} color="green" stacked secondary>
              <Header dividing as="h3">
                Part Label Template
              </Header>
              <p>
                <i>Part labels are printed according to this template.</i>
              </p>

              <LineTemplate
                name="printerLine1"
                line={settings.printer.lines[0]}
                title="Line 1"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
              <LineTemplate
                name="printerLine2"
                line={settings.printer.lines[1]}
                title="Line 2"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
              <LineTemplate
                name="printerLine3"
                line={settings.printer.lines[2]}
                title="Line 3"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
              <LineTemplate
                name="printerLine4"
                line={settings.printer.lines[3]}
                title="Line 4"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
              <LineTemplate
                name="printerIdentifier1"
                line={settings.printer.identifiers[0]}
                title="Identifier 1"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
              <LineTemplate
                name="printerIdentifier2"
                line={settings.printer.identifiers[1]}
                title="Identifier 2"
                color="red"
                fonts={fonts}
                defaultFont={font}
                onChange={this.handleChange}
                tertiary
              />
            </Segment>
          </Segment>
          <Form.Field inline>
            <Button type="submit" primary style={{ marginTop: "10px" }}>
              <Icon name="save" />
              Save
            </Button>
            {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
          </Form.Field>
        </Form>
      </div>
    );
  }
}
