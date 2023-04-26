import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Icon, Input, Label, Button, Form, Segment, Header, Popup, Dropdown, Confirm, Breadcrumb } from "semantic-ui-react";
import LineTemplate from "../components/LineTemplate";
import { DEFAULT_FONT, GetTypeDropdown, GetAdvancedTypeDropdown } from "../common/Types";
import { DigiKeySites } from "../common/digiKeySites";
import { FormHeader } from "../components/FormHeader";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import "./Settings.css";

export const Settings = (props) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [fonts, setFonts] = useState([]);
  const [font, setFont] = useState(null);
  const [saveMessage, setSaveMessage] = useState("");
  const [testing, setTesting] = useState(false);
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const [labelSources] = useState([
    {
      key: 1,
      value: 0,
      text: "Auto",
    },
    {
      key: 2,
      value: 1,
      text: "Left",
    },
    {
      key: 3,
      value: 2,
      text: "Right",
    },
  ]);
  const [enabledSources] = useState([
    {
      key: 1,
      value: 0,
      text: "Disabled",
    },
    {
      key: 2,
      value: 1,
      text: "Enabled",
    },
  ]);
  const digikeySites = GetAdvancedTypeDropdown(DigiKeySites);
  const [settings, setSettings] = useState({
    binner: {
      enabled: true,
      apiKey: "",
      apiUrl: "",
      timeout: "00:00:05"
    },
    digikey: {
      enabled: false,
      site: 0,
      clientId: "",
      clientSecret: "",
      oAuthPostbackUrl: "",
      apiUrl: "",
    },
    mouser: {
      enabled: false,
      searchApiKey: "",
      orderApiKey: "",
      cartApiKey: "",
      apiUrl: "",
    },
    arrow: {
      enabled: false,
      username: "",
      apiKey: "",
      apiUrl: "",
    },
    octopart: {
      enabled: false,
      clientId: "",
      clientSecret: "",
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
            left: 0,
          },
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
            left: 0,
          },
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
            left: 0,
          },
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
            left: 0,
          },
        },
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
            left: 0,
          },
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
            left: 0,
          },
        },
      ],
    },
  });
  const [apiTestResults, setApiTestResults] = useState([]);

  useEffect(() => {
    const loadFonts = async () => {
      await fetchApi("api/print/fonts", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((response) => {
        const { data } = response;
        const newFonts = data.map((l, k) => {
          return {
            key: k,
            value: l,
            text: l,
          };
        });
        const selectedFont = _.find(
          newFonts,
          (x) => x && x.text === DEFAULT_FONT
        );
        setFonts(newFonts);
        setFont(selectedFont.value);
      });
    };

    const loadSettings = async () => {
      await fetchApi("api/system/settings", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
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
    await fetchApi("api/system/settings", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(settings),
    }).then((response) => {
      const saveMessage = t('success.systemSettingsSaved', "System settings were saved.");
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
    if (control.name.startsWith("arrow")) {
      setControlValue(newSettings.arrow, "arrow", control);
    }
    if (control.name.startsWith("octopart")) {
      setControlValue(newSettings.octopart, "octopart", control);
    }

    // todo: find a better way to clean up state changes
    if (control.name.startsWith("printer")) {
      if (control.name.startsWith("printerLine")) {
        if (control.name.startsWith("printerLine1"))
          setControlValue(
            newSettings.printer.lines[0],
            "printerLine1",
            control
          );
        else if (control.name.startsWith("printerLine2"))
          setControlValue(
            newSettings.printer.lines[1],
            "printerLine2",
            control
          );
        else if (control.name.startsWith("printerLine3"))
          setControlValue(
            newSettings.printer.lines[2],
            "printerLine3",
            control
          );
        else if (control.name.startsWith("printerLine4"))
          setControlValue(
            newSettings.printer.lines[3],
            "printerLine4",
            control
          );
      } else if (control.name.startsWith("printerIdentifier")) {
        if (control.name.startsWith("printerIdentifier1"))
          setControlValue(
            newSettings.printer.identifiers[0],
            "printerIdentifier1",
            control
          );
        else if (control.name.startsWith("printerIdentifier2"))
          setControlValue(
            newSettings.printer.identifiers[1],
            "printerIdentifier2",
            control
          );
      } else {
        setControlValue(newSettings.printer, "printer", control);
      }
    }
    setSettings(newSettings);
  };

  const setControlValue = (setting, name, control) => {
    if (control.name === `${name}Enabled`) {
      // for enabled dropdowns, they don't advertise type!
      setting[getControlInstanceName(control, name)] =
        control.value > 0 ? true : false;
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

  const handleAuthRedirect = (e) => {
    e.preventDefault();
    window.location.href = authorizationUrl;
  };

  const handleForgetCredentials = (e, apiName) => {
    e.preventDefault();
    const request = {
      name: apiName
    };
    fetchApi("api/settings/forgetcredentials", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      const { data } = response;
      if (data.success) {
        toast.success(t('clearedCredentials', "Successfully cleared cached credentials for {{apiName}}", { apiName }));
      } else {
        toast.error(t('failedClearedCredentials', "Failed to clear cached credentials for {{apiName}}", { apiName }));
      }
    }).catch((err) => {
      toast.error(`Error: ${err}`);
      console.error('Error!', err);
    });
  };
  
  const handleTestApi = (e, apiName) => {
    e.preventDefault();
    const configuration = [];
    switch(apiName){
      case "swarm":
        configuration.push({ key: "enabled", value: settings.binner.enabled + "" });
        configuration.push({ key: "apiKey", value: settings.binner.apiKey });
        configuration.push({ key: "apiUrl", value: settings.binner.apiUrl });
        configuration.push({ key: "timeout", value: settings.binner.timeout });
        break;
      case "digikey":
        configuration.push({ key: "enabled", value: settings.digikey.enabled + "" });
        configuration.push({ key: "site", value: settings.digikey.site + "" });
        configuration.push({ key: "clientId", value: settings.digikey.clientId });
        configuration.push({ key: "clientSecret", value: settings.digikey.clientSecret });
        configuration.push({ key: "apiUrl", value: settings.digikey.apiUrl });
        configuration.push({ key: "oAuthPostbackUrl", value: settings.digikey.oAuthPostbackUrl });
        break;
      case "mouser":
        configuration.push({ key: "enabled", value: settings.mouser.enabled + "" });
        configuration.push({ key: "searchApiKey", value: settings.mouser.searchApiKey });
        configuration.push({ key: "orderApiKey", value: settings.mouser.orderApiKey });
        configuration.push({ key: "cartApiKey", value: settings.mouser.cartApiKey });
        configuration.push({ key: "apiUrl", value: settings.mouser.apiUrl });
        break;
      case "arrow":
        configuration.push({ key: "enabled", value: settings.arrow.enabled + "" });
        configuration.push({ key: "username", value: settings.arrow.username });
        configuration.push({ key: "apiKey", value: settings.arrow.apiKey });
        configuration.push({ key: "apiUrl", value: settings.arrow.apiUrl });
        break;
      case "octopart":
        configuration.push({ key: "enabled", value: settings.octopart.enabled + "" });
        configuration.push({ key: "clientId", value: settings.octopart.clientId });
        configuration.push({ key: "clientSecret", value: settings.octopart.clientSecret });
        break;
      default:
        break;
    }
    const request = {
      name: apiName,
      configuration
    };
    setTesting(true);
    fetchApi("api/settings/testapi", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      const { data } = response;
      const { success, message, authorizationUrl } = data;
      if (authorizationUrl && authorizationUrl.length > 0){
        setAuthorizationUrl(authorizationUrl);
        setConfirmAuthIsOpen(true);
        return;
      }

      const testResult = _.find(apiTestResults, (i) => i.name === apiName);
      if (testResult === undefined)
        apiTestResults.push({
          name: apiName,
          result: success,
          message: message,
        });
      else {
        testResult.result = success;
        testResult.message = message;
      }
      setApiTestResults([...apiTestResults]);
      setTesting(false);
    }).catch((err) => {
      toast.error(`Error: ${err}`);
      console.error('Error!', err);
      setTesting(false);
    });
  };

  const getTestResultIcon = (apiName) => {
    const testResult = _.find(apiTestResults, (i) => i.name === apiName);

    if (testResult !== undefined) {
      if (testResult.result) {
        return (
          <span>
            <Icon
              style={{ marginLeft: "5px" }}
              name="check circle"
              color="green"
            /> {t('success.testPassed', "Test passed")}
          </span>
        );
      } else {
        return (
          <span>
            <Icon style={{ marginLeft: "5px" }} name="dont" color="red" /> {t('error.testFailed', "Test failed")} - {testResult.message}
          </span>
        );
      }
    }
    return <></>;
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.settings.title', "Settings")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.settings.title', "Settings")} to={".."}>
        <Trans i18nKey="page.settings.description">
          Configure your integrations, printer configuration, as well as label
          part templates.<br />Additional help can be found on the <a href="https://github.com/replaysMike/Binner/wiki/Configuration" target="_blank" rel="noreferrer">Wiki</a>
        </Trans>
			</FormHeader>
      <Confirm
        className="confirm"
        header={t('page.settings.confirm.mustAuthenticateHeader', "Must Authenticate")}
        open={confirmAuthIsOpen}
        onCancel={() => setConfirmAuthIsOpen(false)}
        onConfirm={handleAuthRedirect}
        content={t('page.settings.confirm.mustAuthenticate', "External Api is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.")}
      />
      <Form onSubmit={onSubmit}>
        <Segment loading={loading} color="blue" raised padded>
          <Header dividing as="h3">
            {t('page.settings.integrations', "Integrations")}
          </Header>
          <p>
            <i>
              <Trans i18nKey="page.settings.integrationsDescription">
              To integrate with DigiKey, Mouser, Arrow or Octopart/Nexar API's you must
              obtain API keys for each service you wish to use.
              <br />
              Adding integrations will greatly enhance your experience.
              </Trans>
            </i>
          </p>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.swarm', "Swarm")}
            </Header>
            <p>
              <Trans i18nKey="page.settings.swarmDescription">
              Swarm is a free API service provided by <a href="https://binner.io" target="_blank" rel="noreferrer">Binner's cloud service</a> that 
              contains part metadata from many aggregate sources. It is the primary source of part, media and datasheet information. 
              Registering for your own API Keys will give you higher request limits and can be obtained
              at <a href="https://binner.io/swarm" target="_blank" rel="noreferrer">https://binner.io/swarm</a>
              </Trans>
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.swarmSupport', "Swarm Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[120, 0]}
                hoverable
                content={
                  <p>
                    {t('page.settings.popup.swarmEnabled', "Choose if you would like to enable Binner Swarm support.")}
                  </p>
                }
                trigger={
                  <Dropdown
                    name="swarmEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.binner.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiKey', "Api Key")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.swarmApiKey', "Swarm api key is optional. By registering a free or paid api key you will receive higher request limits accordingly.")}
                    
                  </p>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.binner.apiKey || ""}
                    name="swarmApiKey"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiUrl', "Api Url")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.swarmApiUrl', "Swarm's API Url")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder="swarm.binner.io"
                    value={(settings.binner.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="swarmApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.timeout', "Timeout")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.swarmTimeout', "Swarm api request timeout. Default: '00:00:05' (5 seconds)")}</p>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.binner.timeout || ""}
                    name="swarmTimeout"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "swarm")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("swarm")}
            </Form.Field>
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.digikey', "DigiKey")}
            </Header>
            <p>
            <Trans i18nKey="page.settings.digikeyDescription">
            Digikey API Keys are free and can be obtained at <a href="https://developer.digikey.com/" target="_blank" rel="noreferrer">https://developer.digikey.com/</a>
            </Trans>
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.digikeySupport', "DigiKey Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                    <p>{t('page.settings.popup.digikeyEnabled', "Choose if you would like to enable DigiKey support. You will occasionally be asked to login to your DigiKey account to allow Binner to access your information.")}</p>
                }
                trigger={
                  <Dropdown
                    name="digikeyEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.digikey.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.site', "Site")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={
                  <div>
                    <Trans i18nKey={"page.settings.popup.digikeySite"}>
                      Choose the DigiKey site to use.
                    </Trans>
                  </div>
                }
                trigger={
                  <Dropdown 
                    name="digikeySite"
                    selection
                    value={settings.digikey.site || 0}
                    options={digikeySites}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.clientId', "Client Id")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={
                  <div>
                    <Trans i18nKey={"page.settings.popup.digikeyClientId"}>
                      Your DigiKey <b>Client ID</b>.
                      <div className="popupimage">
                        <img
                          src="/image/help/digikey-apikeys.png"
                          alt="DigiKey Client ID"
                        />
                        Figure 1. DigiKey Client Id settings
                      </div>
                    </Trans>
                  </div>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.digikey.clientId || ""}
                    name="digikeyClientId"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.clientSecret', "Client Secret")}</label>
              <Popup
                wide
                position="top left"
                offset={[95, 0]}
                hoverable
                content={
                  <div>
                    <Trans i18nKey={"page.settings.popup.digikeyClientSecret"}>
                      Your DigiKey <b>Client Secret</b>.
                      <div className="popupimage">
                        <img
                          src="/image/help/digikey-apikeys.png"
                          alt="DigiKey Client Secret"
                        />
                        Figure 1. DigiKey Client Secret settings
                      </div>
                    </Trans>
                  </div>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.digikey.clientSecret || ""}
                    name="digikeyClientSecret"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiUrl', "Api Url")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={
                  <p>
                    <Trans i18nKey={"page.settings.popup.digikeyApiUrl"}>
                      DigiKey's API Url. This will either be <i>api.digikey.com</i> (live) or <i>sandbox-api.digikey.com</i> (for testing only)
                    </Trans>
                  </p>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder="sandbox-api.digikey.com"
                    value={(settings.digikey.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="digikeyApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.postbackUrl', "Postback Url")}</label>
              <Popup
                wide="very"
                position="top left"
                offset={[95, 0]}
                hoverable
                content={
                  <div>
                    <Trans i18nKey={"page.settings.popup.digikeyOAuthPostbackUrl"}>
                    Binner's postback url must be registered with DigiKey exactly as specified here, on DigiKey this is named <b>Callback URL</b>.
                    This should almost always be localhost, and no firewall settings are required as your web browser will be making the request.
                    <br /><br />
                    <b>Example: </b><i>localhost:8090/Authorization/Authorize</i>
                    <br />
                    <div className="popupimage">
                      <img
                        src="/image/help/digikey-callbackurl.png"
                        alt="DigiKey Callback URL"
                      />
                      Figure 1. DigiKey Callback URL
                    </div>
                    </Trans>
                  </div>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder="localhost:8090/Authorization/Authorize"
                    value={(settings.digikey.oAuthPostbackUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="digikeyOAuthPostbackUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                }
              />
            </Form.Field>
            <Form.Field>
              <Popup 
                wide
                content={<p>{t('page.settings.popup.forgetCredentials', "Forget any cached credentials and force reauthentication with DigiKey")}</p>}
                trigger={<Button
                  secondary
                  className="test"
                  type="button"
                  onClick={(e) => handleForgetCredentials(e, "digikey")}
                  disabled={testing}
                >
                  {t('button.forgetCredentials', "Forget Credentials")}
                </Button>}
              />              
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "digikey")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("digikey")}
            </Form.Field>
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.mouser', "Mouser")}
            </Header>
            <p>
              <Trans i18nKey="page.settings.mouserDescription">
              Mouser API Keys can be obtained at <a href="https://www.mouser.com/api-hub/" target="_blank" rel="noreferrer">https://www.mouser.com/api-hub/</a>
              </Trans>
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.mouserSupport', "Mouser Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[120, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.mouserEnabled', "Choose if you would like to enable Mouser support.")}</p>
                }
                trigger={
                  <Dropdown
                    name="mouserEnabled"
                    placeholder="Enabled"
                    selection
                    value={settings.mouser.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('page.settings.searchApiKey', "Search Api Key")}</label>
              <Popup
                position="top left"
                offset={[110, 0]}
                hoverable
                content={<p>{t('page.settings.popup.mouserSearchApiKey', "Your api key for accessing the search api.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.searchApiKey || ""}
                    name="mouserSearchApiKey"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('page.settings.ordersApiKey', "Orders Api Key")}</label>
              <Popup
                position="top left"
                offset={[110, 0]}
                hoverable
                content={<p>{t('page.settings.popup.mouserOrderApiKey', "Your api key for accessing the orders api.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.orderApiKey || ""}
                    name="mouserOrderApiKey"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('page.settings.cartApiKey', "Cart Api Key")}</label>
              <Popup
                position="top left"
                offset={[90, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.mouserCartApiKey', "Your api key for accessing the shopping cart api.")}</p>
                }
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.cartApiKey || ""}
                    name="mouserCartApiKey"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiUrl', "Api Url")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.mouserApiUrl', "Mouser's API Url. This will be api.mouser.com")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder="api.mouser.com"
                    value={(settings.mouser.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="mouserApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                }
              />
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "mouser")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("mouser")}
            </Form.Field>
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.arrow', "Arrow")}
            </Header>
            <p>
              <Trans i18nKey="page.settings.arrowDescription">
              Arrow API Keys can be obtained at <a href="https://developers.arrow.com/api/index.php/site/page?view=requestAPIKey" target="_blank" rel="noreferrer">https://developers.arrow.com/api/index.php/site/page?view=requestAPIKey</a>
              </Trans>
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.arrowSupport', "Arrow Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.arrowEnabled', "Choose if you would like to enable Arrow support.")}</p>
                }
                trigger={
                  <Dropdown
                    name="arrowEnabled"
                    placeholder="Disabled"
                    selection
                    value={settings.arrow.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.username', "Username")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.arrowUsername', "Your username/login for Arrow.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.arrow.username || ""}
                    name="arrowUsername"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiKey', "Api Key")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.arrowApiKey', "Your api key for Arrow.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.arrow.apiKey || ""}
                    name="arrowApiKey"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiUrl', "Api Url")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.arrowApiUrl', "Arrow's API Url. This will be api.arrow.com")}</p>}
                trigger={
                  <Input
                    action
                    className="labeled"
                    placeholder="api.arrow.com"
                    value={(settings.arrow.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="arrowApiUrl"
                    onChange={handleChange}
                  >
                    <Label>https://</Label>
                    <input />
                  </Input>
                }
              />
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "arrow")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("arrow")}
            </Form.Field>
          </Segment>

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.octopartNexar', "Octopart/Nexar")}
            </Header>
            <p>
              <Trans i18nKey="page.settings.octopartNexarDescription">
              Octopart/Nexar API Keys can be obtained at <a href="https://portal.nexar.com/sign-up" target="_blank" rel="noreferrer">https://portal.nexar.com/sign-up</a>
              </Trans>              
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.octopartNexarSupport', "Octopart/Nexar Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.octopartEnabled', "Choose if you would like to enable Octopart/Nexar support.")}</p>
                }
                trigger={
                  <Dropdown
                    name="octopartEnabled"
                    placeholder="Disabled"
                    selection
                    value={settings.octopart.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.clientId', "Client Id")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.octopartClientId', "Your Client Id for Octopart/Nexar.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.octopart.clientId || ""}
                    name="octopartClientId"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.clientSecret', "Client Secret")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.octopartClientSecret', "Your Client Secret for Octopart/Nexar.")}</p>}
                trigger={
                  <Input
                    className="labeled"
                    placeholder=""
                    value={settings.octopart.clientSecret || ""}
                    name="octopartClientSecret"
                    onChange={handleChange}
                  ></Input>
                }
              />
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "octopart")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("octopart")}
            </Form.Field>
          </Segment>
        </Segment>

        <Segment loading={loading} color="blue" raised padded>
          <Header dividing as="h3">
            {t('page.settings.printerConfiguration', "Printer Configuration")}
          </Header>
          <p>
            <i>
              {t('page.settings.printerConfigDescription', "Configure your printer name as it shows up in your environment (Windows Printers or CUPS Printer Name)")}
            </i>
          </p>
          <Popup
            content={
              <p>{t('page.settings.popup.printerPrinterName', "Your printer name as it appears in Windows, or CUPS (Unix).")}</p>
            }
            trigger={
              <Form.Field width={10}>
                <label>{t('page.settings.printerName', "Printer Name")}</label>
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
            content={<p>{t('page.settings.popup.printerPartLabelSource', "Your printer name as it appears in Windows, or CUPS (Unix).")}</p>}
            trigger={
              <Form.Field width={10}>
                <label>{t('page.settings.partLabelSource', "Part Label Source")}</label>
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
                <Trans i18nKey="page.settings.popup.partLabelName">
                The name of the label model installed in your printer. This will
                be specific to your printer, and must be defined in your
                appsettings.json under <i>WebHostServiceConfiguration.PrinterConfiguration.LabelDefinitions</i>
                </Trans>
              </p>
            }
            trigger={
              <Form.Field width={10}>
                <label>{t('page.settings.partLabelName', "Part Label Name")}</label>
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
              {t('page.settings.partLabelTemplate', "Part Label Template")}
            </Header>
            <p>
              <i>{t('page.settings.partLabelTemplateDescription', "Part labels are printed according to this template.")}</i>
            </p>

            <LineTemplate
              name="printerLine1"
              line={settings.printer.lines[0]}
              title={t('page.settings.lineX', "Line {{number}}", { number: 1 })}
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine2"
              line={settings.printer.lines[1]}
              title={t('page.settings.lineX', "Line {{number}}", { number: 2 })}
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine3"
              line={settings.printer.lines[2]}
              title={t('page.settings.lineX', "Line {{number}}", { number: 3 })}
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerLine4"
              line={settings.printer.lines[3]}
              title={t('page.settings.lineX', "Line {{number}}", { number: 4 })}
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerIdentifier1"
              line={settings.printer.identifiers[0]}
              title={t('page.settings.identifierX', "Identifier {{number}}", { number: 1 })}
              color="red"
              fonts={fonts}
              font={font}
              onChange={handleChange}
              tertiary
            />
            <LineTemplate
              name="printerIdentifier2"
              line={settings.printer.identifiers[1]}
              title={t('page.settings.identifierX', "Identifier {{number}}", { number: 2 })}
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
            {t('button.save', "Save")}
          </Button>
          {saveMessage.length > 0 && (
            <Label pointing="left">{saveMessage}</Label>
          )}
        </Form.Field>
      </Form>
    </div>
  );
};
