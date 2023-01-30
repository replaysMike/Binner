import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import _ from "underscore";
import { Icon, Input, Label, Button, Form, Segment, Header, Popup, Dropdown } from "semantic-ui-react";
import LineTemplate from "../components/LineTemplate";
import { DEFAULT_FONT } from "../common/Types";
import { HandleJsonResponse } from "../common/handleResponse.js";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";

export const Settings = (props) => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [fonts, setFonts] = useState([]);
  const [font, setFont] = useState(null);
  const [saveMessage, setSaveMessage] = useState("");
  const [labelSources] = useState([
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
  ]);
  const [enabledSources] = useState([
    {
      key: 1,
      value: 0,
      text: "Disabled"
    },
    {
      key: 2,
      value: 1,
      text: "Enabled"
    }
  ]);
  const [settings, setSettings] = useState({
    binner: {
      enabled: true,
      apiKey: "",
      apiUrl: ""
    },
    digikey: {
      enabled: false,
      clientId: "",
      clientSecret: "",
      oAuthPostbackUrl: "",
      apiUrl: ""
    },
    mouser: {
      enabled: false,
      searchApiKey: "",
      orderApiKey: "",
      cartApiKey: "",
      apiUrl: ""
    },
    octopart: {
      enabled: false,
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
          lowerCase: false,
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
          lowerCase: false,
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
          lowerCase: false,
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
          lowerCase: false,
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
          lowerCase: false,
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
          lowerCase: false,
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
  });

  useEffect(() => {
    const loadFonts = async () => {
      await fetchApi("print/fonts", {
        method: "GET",
        headers: {
          "Content-Type": "application/json"
        }
      }).then((response) => {
        const { data } = response;
        const newFonts = data.map((l, k) => {
          return {
            key: k,
            value: l,
            text: l
          };
        });
        const selectedFont = _.find(newFonts, (x) => x && x.text === DEFAULT_FONT);
        setFonts(newFonts);
        setFont(selectedFont.value);
      });
    };

    const loadSettings = async () => {
      await fetchApi("system/settings", {
        method: "GET",
        headers: {
          "Content-Type": "application/json"
        }
      }).then((response) => {
        const { data } = response;
        setLoading(false);
        setSettings(data);
      });
    };

    loadSettings();
    loadFonts();
  }, []);

  /**
   * Save the system settings
   *
   * @param {any} e
   */
  const onSubmit = async (e) => {
    await fetchApi("system/settings", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(settings)
    }).then((response) => {
      console.log('response', response);
      const saveMessage = "System settings were saved.";
      toast.success(saveMessage);
      setSaveMessage(saveMessage);
      navigate(-1);
    });
  };

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    const newSettings = { ...settings };
    // redirect new values to the correct state object
    if (control.name.startsWith("swarm")) {
      setControlValue(newSettings.binner, "swarm", control);
    }
    if (control.name.startsWith("digikey")) {
      setControlValue(newSettings.digikey, "digikey", control);
    }
    if (control.name.startsWith("mouser")) {
      setControlValue(newSettings.mouser, "mouser", control);
    }
    if (control.name.startsWith("octopart")) {
      setControlValue(newSettings.octopart, "octopart", control);
    }

    // todo: find a better way to clean up state changes
    if (control.name.startsWith("printer")) {
      if (control.name.startsWith("printerLine")) {
        if (control.name.startsWith("printerLine1")) setControlValue(newSettings.printer.lines[0], "printerLine1", control);
        else if (control.name.startsWith("printerLine2")) setControlValue(newSettings.printer.lines[1], "printerLine2", control);
        else if (control.name.startsWith("printerLine3")) setControlValue(newSettings.printer.lines[2], "printerLine3", control);
        else if (control.name.startsWith("printerLine4")) setControlValue(newSettings.printer.lines[3], "printerLine4", control);
      } else if (control.name.startsWith("printerIdentifier")) {
        if (control.name.startsWith("printerIdentifier1")) setControlValue(newSettings.printer.identifiers[0], "printerIdentifier1", control);
        else if (control.name.startsWith("printerIdentifier2")) setControlValue(newSettings.printer.identifiers[1], "printerIdentifier2", control);
      } else {
        setControlValue(newSettings.printer, "printer", control);
      }
    }
    setSettings(newSettings);
  };

  const setControlValue = (setting, name, control) => {
    if (control.name === `${name}Enabled`) {
      // for enabled dropdowns, they don't advertise type!
      setting[getControlInstanceName(control, name)] = control.value > 0 ? true : false;
      return;
    }
    switch (control.type) {
      case "checkbox":
        // type is a checkbox, we only care about the checked value
        setting[getControlInstanceName(control, name)] = control.checked;
        break;
      default:
        // this is a text input or any other value type control
        setting[getControlInstanceName(control, name)] = control.value;
        break;
    }
  };

  const getControlInstanceName = (control, prefix) => {
    let controlName = control.name.replace(prefix, "");
    const instance = controlName.charAt(0).toLowerCase() + controlName.slice(1);
    return instance;
  };

  return (
    <div>
      <h1>Settings</h1>
      <p>Configure your integrations, printer configuration, as well as label part templates.</p>

      <Form onSubmit={onSubmit}>
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
              Swarm
            </Header>
            <p>
              Swarm is a free API service provided by Binner that contains part information from many aggregate sources. It is the primary source of part, media
              and datasheet information. Registering for your own API Keys will give you higher request limits and can be obtained at{" "}
              <a href="https://binner.io/swarm" target="_blank" rel="noreferrer">
                https://binner.io/swarm
              </a>
            </p>
            <Popup
              wide
              content={<p>Check this box to enable Binner Swarm support.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Swarm Support</label>
                  <Dropdown
                    name="swarmEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.binner.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                </Form.Field>
              }
            />
            <Popup
              wide
              content={<p>Swarm api key is optional. By registering a free or paid api key you will receive higher request limits accordingly.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Api Key</label>
                  <Input className="labeled" placeholder="" value={settings.binner.apiKey || ""} name="swarmApiKey" onChange={handleChange}></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Swarm's API Url</p>}
              trigger={
                <Form.Field width={10}>
                  <label>ApiUrl</label>
                  <Input
                    className="labeled"
                    placeholder="swarm.binner.io"
                    value={(settings.binner.apiUrl || "").replace("http://", "").replace("https://", "")}
                    name="swarmApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                </Form.Field>
              }
            />
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              DigiKey
            </Header>
            <p>
              Digikey API Keys are free and can be obtained at{" "}
              <a href="https://developer.digikey.com/" target="_blank" rel="noreferrer">
                https://developer.digikey.com/
              </a>
            </p>
            <Popup
              wide
              content={
                <p>
                  Check this box to enable DigiKey support. You will occasionally be asked to login to your DigiKey account to allow Binner to access your
                  information.
                </p>
              }
              trigger={
                <Form.Field width={10}>
                  <label>DigiKey Support</label>
                  <Dropdown
                    name="digikeyEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.digikey.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your DigiKey client id.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>ClientId</label>
                  <Input className="labeled" placeholder="" value={settings.digikey.clientId || ""} name="digikeyClientId" onChange={handleChange}></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your DigiKey client secret (api key).</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Client Secret</label>
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.digikey.clientSecret || ""}
                    name="digikeyClientSecret"
                    onChange={handleChange}
                  ></Input>
                </Form.Field>
              }
            />
            <Popup
              wide="very"
              content={<p>DigiKey's API Url. This will either be api.digikey.com (live) or sandbox-api.digikey.com (for testing)</p>}
              trigger={
                <Form.Field width={10}>
                  <label>ApiUrl</label>
                  <Input
                    className="labeled"
                    placeholder="sandbox-api.digikey.com"
                    value={(settings.digikey.apiUrl || "").replace("http://", "").replace("https://", "")}
                    name="digikeyApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                </Form.Field>
              }
            />
            <Popup
              wide="very"
              content={
                <p>
                  Binner's postback url must be registered with your DigiKey Api account and be accessable through your router. Ensure your firewall rules allow
                  incoming requests to Binner's port (default 8090) and is port forwarded to the correct machine on your network (localhost won't work, must be
                  your public IP). Example: <i>54.55.123.46:8090/Authorization/Authorize</i>
                </p>
              }
              trigger={
                <Form.Field width={10}>
                  <label>PostbackUrl</label>
                  <Input
                    className="labeled"
                    placeholder="localhost:8090/Authorization/Authorize"
                    value={(settings.digikey.oAuthPostbackUrl || "").replace("http://", "").replace("https://", "")}
                    name="digikeyOAuthPostbackUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                </Form.Field>
              }
            />
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              Mouser
            </Header>
            <p>
              Mouser API Keys can be obtained at{" "}
              <a href="https://www.mouser.com/api-hub/" target="_blank" rel="noreferrer">
                https://www.mouser.com/api-hub/
              </a>
            </p>
            <Popup
              wide
              content={<p>Check this box to enable Mouser support.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Mouser Support</label>
                  <Dropdown
                    name="mouserEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.mouser.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your api key for accessing the search api.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Search Api Key</label>
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.searchApiKey || ""}
                    name="mouserSearchApiKey"
                    onChange={handleChange}
                  ></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your api key for accessing the orders api.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Order Api Key</label>
                  <Input className="labeled" placeholder="" value={settings.mouser.orderApiKey || ""} name="mouserOrderApiKey" onChange={handleChange}></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your api key for accessing the shopping cart api.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Cart Api Key</label>
                  <Input className="labeled" placeholder="" value={settings.mouser.cartApiKey || ""} name="mouserCartApiKey" onChange={handleChange}></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Mouser's API Url. This will be api.mouser.com</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Api Url</label>
                  <Input
                    className="labeled"
                    placeholder="api.mouser.com"
                    value={(settings.mouser.apiUrl || "").replace("http://", "").replace("https://", "")}
                    name="mouserApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                </Form.Field>
              }
            />
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              Octopart
            </Header>
            <p>
              Octopart API Keys can be obtained at{" "}
              <a href="https://octopart.com/api/home" target="_blank" rel="noreferrer">
                https://octopart.com/api/home
              </a>
            </p>
            <Popup
              wide
              content={<p>Check this box to enable Octopart support.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Octopart Support</label>
                  <Dropdown
                    name="octopartEnabled"
                    placeholder="Disabled"
                    selection
                    value={settings.octopart.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                </Form.Field>
              }
            />
            <Popup
              content={<p>Your api key for Octopart.</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Api Key</label>
                  <Input className="labeled" placeholder="" value={settings.octopart.apiKey || ""} name="octopartApiKey" onChange={handleChange}></Input>
                </Form.Field>
              }
            />
            <Popup
              content={<p>Octopart's API Url. This will be octopart.com</p>}
              trigger={
                <Form.Field width={10}>
                  <label>Api Url</label>
                  <Input
                    action
                    className="labeled"
                    placeholder="api.mouser.com"
                    value={(settings.octopart.apiUrl || "").replace("http://", "").replace("https://", "")}
                    name="octopartApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                </Form.Field>
              }
            />
          </Segment>
        </Segment>

        <Segment loading={loading} color="blue" raised padded>
          <Header dividing as="h3">
            Printer Configuration
          </Header>
          <p>
            <i>Configure your printer name as it shows up in your environment (Windows Printers or CUPS Printer Name)</i>
          </p>
          <Popup
            content={<p>Your printer name as it appears in Windows, or CUPS (Unix).</p>}
            trigger={
              <Form.Field width={10}>
                <label>Printer Name</label>
                <Input
                  className="labeled"
                  placeholder="DYMO LabelWriter 450 Twin Turbo"
                  value={settings.printer.printerName || ""}
                  name="printerPrinterName"
                  onChange={handleChange}
                ></Input>
              </Form.Field>
            }
          />
          <Popup
            content={<p>The label paper source to use.</p>}
            trigger={
              <Form.Field width={10}>
                <label>Part Label Source</label>
                <Dropdown
                  name="printerPartLabelSource"
                  placeholder="Right"
                  selection
                  value={settings.printer.partLabelSource}
                  options={labelSources}
                  onChange={handleChange}
                />
              </Form.Field>
            }
          />
          <Popup
            content={
              <p>
                The name of the label model installed in your printer. This will be specific to your printer, and must be defined in your appsettings.json under{" "}
                <i>WebHostServiceConfiguration.PrinterConfiguration.LabelDefinitions</i>
              </p>
            }
            trigger={
              <Form.Field width={10}>
                <label>Part Label Name</label>
                <Input
                  className="labeled"
                  placeholder="30346"
                  value={settings.printer.partLabelName || ""}
                  name="printerPartLabelName"
                  onChange={handleChange}
                ></Input>
              </Form.Field>
            }
          />

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
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine2"
              line={settings.printer.lines[1]}
              title="Line 2"
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine3"
              line={settings.printer.lines[2]}
              title="Line 3"
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine4"
              line={settings.printer.lines[3]}
              title="Line 4"
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerIdentifier1"
              line={settings.printer.identifiers[0]}
              title="Identifier 1"
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerIdentifier2"
              line={settings.printer.identifiers[1]}
              title="Identifier 2"
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
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
};
