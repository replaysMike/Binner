import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Icon, Label, Button, Form, Segment, Header, Popup, Dropdown, Confirm, Breadcrumb, Flag } from "semantic-ui-react";
import ClearableInput from "../components/ClearableInput";
import LineTemplate from "../components/LineTemplate";
import { DEFAULT_FONT, BarcodeProfiles, GetAdvancedTypeDropdown, GetTypeDropdown } from "../common/Types";
import { DigiKeySites } from "../common/digiKeySites";
import { TmeCountries } from "../common/tmeCountries";
import { FormHeader } from "../components/FormHeader";
import { fetchApi } from "../common/fetchApi";
import { getLocalData, setLocalData, removeLocalData } from "../common/storage";
import { setSystemSettings } from "../common/applicationSettings";
import { toast } from "react-toastify";
import { getCurrencySymbol } from "../common/Utils";
import "./Settings.css";

export const Settings = (props) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'settings' })
  };

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'settings' });
  };

  const removeViewPreference = (preferenceName, value) => {
    removeLocalData(preferenceName, value, { settingsName: 'settings' });
  };

  const [loading, setLoading] = useState(true);
  const [fonts, setFonts] = useState([]);
  const [font, setFont] = useState(null);
  const [saveMessage, setSaveMessage] = useState("");
  const [testing, setTesting] = useState(false);
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationApiName, setAuthorizationApiName] = useState('');
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const [isDirty, setIsDirty] = useState(false);
  const [printModes] = useState([
    {
      key: 1,
      value: 0,
      text: "Direct",
      description: "Print using built-in SDK support"
    },
    {
      key: 2,
      value: 1,
      text: "Web Browser",
      description: "Print using the web browser's print dialog"
    },
  ]);
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
  const [languages] = useState([{
    key: 1,
    value: "en",
    text: "English",
  }, {
    key: 2,
    value: "br",
    text: "Breton",
  }, {
    key: 3,
    value: "cs",
    text: "Czech",
  }, {
    key: 4,
    value: "da",
    text: "Danish",
  }, {
    key: 5,
    value: "de",
    text: "German",
  }, {
    key: 6,
    value: "es",
    text: "Spanish",
  }, {
    key: 7,
    value: "fi",
    text: "Finnish",
  }, {
    key: 8,
    value: "fr",
    text: "French",
  }, {
    key: 9,
    value: "he",
    text: "Hebrew",
  }, {
    key: 10,
    value: "hu",
    text: "Hungarian",
  }, {
    key: 11,
    value: "it",
    text: "Italian",
  }, {
    key: 12,
    value: "ja",
    text: "Japanese",
  }, {
    key: 13,
    value: "ko",
    text: "Korean",
  }, {
    key: 14,
    value: "nl",
    text: "Dutch",
  }, {
    key: 15,
    value: "no",
    text: "Norwegian",
  }, {
    key: 16,
    value: "pl",
    text: "Polish",
  }, {
    key: 17,
    value: "pt",
    text: "Portuguese",
  }, {
    key: 18,
    value: "ro",
    text: "Romanian",
  }, {
    key: 19,
    value: "sv",
    text: "Swedish",
  }, {
    key: 20,
    value: "th",
    text: "Thai",
  }, {
    key: 21,
    value: "zhs",
    text: "Chinese (Simplified)",
  }, {
    key: 22,
    value: "zht",
    text: "Chinese (Traditional)",
  },
  // languages below are compatible with TME, they don't use official language codes but rather country codes :P
  {
    key: 23,
    value: "bg",
    country: "BG",
    text: "Bulgarian",
  }, {
    key: 24,
    value: "rm",
    country: "CH",
    text: "Romansh",
  }, {
    key: 25,
    value: "el",
    country: "GR",
    text: "Greek",
  }, {
    key: 26,
    value: "hr",
    country: "HR",
    text: "Croatian",
  }, {
    key: 27,
    value: "lt",
    country: "LT",
    text: "Lithuanian",
  }, {
    key: 28,
    value: "lv",
    country: "LV",
    text: "Latvian",
  }, {
    key: 29,
    value: "ru",
    country: "RU",
    text: "Russian",
  }, {
    key: 30,
    value: "sk",
    country: "SK",
    text: "Slovak",
  }, {
    key: 31,
    value: "tr",
    country: "TR",
    text: "Turkish",
  }, {
    key: 32,
    value: "uk",
    country: "UA",
    text: "Ukraine",
  },
  ]);
  const [currencies] = useState([{
    key: 1,
    value: "USD",
    text: `${getCurrencySymbol("USD")} - US Dollar`,
  },{
    key: 2,
    value: "CAD",
    text: `${getCurrencySymbol("CAD")} - Canadian Dollar`,
  },{
    key: 3,
    value: "JPY",
    text: `${getCurrencySymbol("JPY")} - Japanese Yen`,
  },{
    key: 4,
    value: "GBP",
    text: `${getCurrencySymbol("GBP")} - Pound sterling`,
  },{
    key: 5,
    value: "EUR",
    text: `${getCurrencySymbol("EUR")} - Euro`,
  },{
    key: 6,
    value: "HKD",
    text: `${getCurrencySymbol("HKD")} - Hong Kong dollar`,
  },{
    key: 7,
    value: "SGD",
    text: `${getCurrencySymbol("SGD")} - Singapore dollar`,
  },{
    key: 8,
    value: "TWD",
    text: `${getCurrencySymbol("TWD")} - New Taiwan dollar`,
  },{
    key: 9,
    value: "KRW",
    text: `${getCurrencySymbol("KRW")} - South Korean won`,
  },{
    key: 10,
    value: "AUD",
    text: `${getCurrencySymbol("AUD")} - Australian dollar`,
  },{
    key: 11,
    value: "NZD",
    text: `${getCurrencySymbol("NZD")} - New Zealand dollar`,
  },{
    key: 12,
    value: "INR",
    text: `${getCurrencySymbol("INR")} - Indian Rupee`,
  },{
    key: 13,
    value: "DKK",
    text: `${getCurrencySymbol("DKK")} - Danish krone`,
  },{
    key: 14,
    value: "NOK",
    text: `${getCurrencySymbol("NOK")} - Norwegian krone`,
  },{
    key: 15,
    value: "SEK",
    text: `${getCurrencySymbol("SEK")} - Swedish krona`,
  },{
    key: 16,
    value: "ILS",
    text: `${getCurrencySymbol("ILS")} - Israeli new shekel`,
  },{
    key: 17,
    value: "CNY",
    text: `${getCurrencySymbol("CNY")} - Chinese Yuan (Renminbi)`,
  },{
    key: 18,
    value: "PLN",
    text: `${getCurrencySymbol("PLN")} - Polish Zloty`,
  },{
    key: 19,
    value: "CHF",
    text: `${getCurrencySymbol("CHF")} - Swiss Franc`,
  },{
    key: 20,
    value: "CZK",
    text: `${getCurrencySymbol("CZK")} - Czech Koruna`,
  },{
    key: 21,
    value: "HUF",
    text: `${getCurrencySymbol("HUF")} - Hungarian Forint`,
  },{
    key: 22,
    value: "RON",
    text: `${getCurrencySymbol("RON")} - Romanian Leu`,
  },{
    key: 23,
    value: "ZAR",
    text: `${getCurrencySymbol("ZAR")} - South African Rand`,
  },{
    key: 24,
    value: "MYR",
    text: `${getCurrencySymbol("MYR")} - Malaysian Ringgit`,
  },{
    key: 25,
    value: "THB",
    text: `${getCurrencySymbol("THB")} - Thai Baht`,
  },{
    key: 26,
    value: "PHP",
    text: `${getCurrencySymbol("PHP")} - Philippine peso`,
  },]);
  const barcodeProfileOptions = GetTypeDropdown(BarcodeProfiles);
  const digikeySites = GetAdvancedTypeDropdown(DigiKeySites);
  const noFlags = ['ss', 'sk', 'sx', 'mf', 'bl', 'im', 'xk', 'je', 'gg', 'cw', 'bq', 'aq'];
  const tmeCountries = TmeCountries.map((i, key) => ({
    key,
    value: i.iso2,
    text: i.name,
    flag: !noFlags.includes(i.iso2.toLowerCase()) ? i.iso2.toLowerCase() : ""
  }));
  const [settings, setSettings] = useState({
    licenseKey: "",
    language: "",
    currency: "",
    maxCacheItems: 1024,
    cacheAbsoluteExpirationMinutes: 0,
    cacheSlidingExpirationMinutes: 30,
    binner: {
      enabled: true,
      apiKey: "",
      apiUrl: "",
      timeout: "00:00:05"
    },
    digikey: getViewPreference("digikey") 
      || {
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
    tme: {
      enabled: false,
      applicationSecret: "",
      apiKey: "",
      apiUrl: "",
      country: "us"
    },
    printer: {
      printMode: 0,
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
    barcode: {
      enabled: true,
      isDebug: false,
      maxKeystrokeThresholdMs: 300,
      bufferTime: 80,
      profile: BarcodeProfiles.Default,
      prefix2D: "[)>",
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
        setSettings({ ...data, barcode: {...data.barcode }});
        setSystemSettings(data);
        
        const digikeyTempSettings = getViewPreference("digikey");
        if (digikeyTempSettings) {
          setSettings({...data, digikey: digikeyTempSettings});
          setIsDirty(true);
        }
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
    const newSettings = {...settings, barcode: {...settings.barcode, bufferTime: parseInt(settings.barcode.bufferTime), maxKeystrokeThresholdMs: parseInt(settings.barcode.maxKeystrokeThresholdMs) }};
    await fetchApi("api/system/settings", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(newSettings),
    }).then((response) => {
      if (response.responseObject.ok) {
        setSystemSettings(newSettings);
        const saveMessage = t('success.systemSettingsSaved', "System settings were saved.");
        toast.success(saveMessage);
        setSaveMessage(saveMessage);
        removeViewPreference('digikey', {});
        navigate(-1);
      } else {
        const errorMessage = t('success.failedToSaveSettings', "Failed to save settings!");
        toast.error(errorMessage);
        setSaveMessage(saveMessage);
      }
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
    else if (control.name.startsWith("digikey")) {
      setControlValue(newSettings.digikey, "digikey", control);
    }
    else if (control.name.startsWith("mouser")) {
      setControlValue(newSettings.mouser, "mouser", control);
    }
    else if (control.name.startsWith("arrow")) {
      setControlValue(newSettings.arrow, "arrow", control);
    }
    else if (control.name.startsWith("octopart")) {
      setControlValue(newSettings.octopart, "octopart", control);
    }
    else if (control.name.startsWith("tme")) {
      setControlValue(newSettings.tme, "tme", control);
    }
    else if (control.name.startsWith("barcode")) {
      setControlValue(newSettings.barcode, "barcode", control);
    } else if (control.name.startsWith("printer")) {
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
    } else {
      setControlValue(newSettings, "", control);
    }
    setSettings(newSettings);
    setIsDirty(true);
  };

  const setControlValue = (setting, name, control) => {
    if (control.name === `${name}Enabled` || control.name === `${name}IsDebug`) {
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
  
  const handleTestApi = async (e, apiName) => {
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
        setViewPreference('digikey', {
          enabled: settings.digikey.enabled,
          site: settings.digikey.site,
          clientId: settings.digikey.clientId,
          clientSecret: settings.digikey.clientSecret,
          oAuthPostbackUrl: settings.digikey.oAuthPostbackUrl,
          apiUrl: settings.digikey.apiUrl,
        });
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
      case "tme":
        configuration.push({ key: "enabled", value: settings.tme.enabled + "" });
        configuration.push({ key: "applicationSecret", value: settings.tme.applicationSecret });
        configuration.push({ key: "country", value: settings.tme.country });
        configuration.push({ key: "apiKey", value: settings.tme.apiKey });
        configuration.push({ key: "apiUrl", value: settings.tme.apiUrl });
        break;
      default:
        break;
    }
    const request = {
      name: apiName,
      configuration
    };
    setTesting(true);
    await fetchApi("api/settings/testapi", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      const { data } = response;
      const { success, message, authorizationUrl } = data;
      if (authorizationUrl && authorizationUrl.length > 0){
        setAuthorizationApiName(apiName);
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
    }).catch((err) => {
      toast.error(`Error: ${err}`);
      console.error('Error!', err);
    });
    setTesting(false);
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
    <div className="mask">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.settings.title', "Settings")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.settings.title', "Settings")} to="/">
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
        content={<p>
          <Trans i18nKey="page.settings.confirm.mustAuthenticate" name={authorizationApiName}>
            External Api (<b>{{ name: authorizationApiName }}</b>) is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.
          </Trans>
          </p>
        }
      />
      <Form onSubmit={onSubmit}>
        <Segment loading={loading} color="blue" raised padded>
          <Header dividing as="h3">
            {t('page.settings.application', "Application")}
          </Header>
          
          <Segment color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.global', "Global")}
            </Header>
            <p>
              <i>
                <Trans i18nKey="page.settings.globalDescription">
                Global application settings are used across the entire application.
                </Trans>
              </i>
            </p>

            <Form.Field width={10}>
              <label>{t('label.licenseKey', "License Key")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<Trans i18nKey="page.settings.popup.licenseKey">
                  If you have a paid Binner license, enter the key here. License keys can be obtained at <a href="https://binner.io" target="_blank" rel="noreferrer">Binner.io</a>
                </Trans>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.licenseKey || ""}
                    name="licenseKey"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.language', "Language")}</label>
              <Popup
                wide
                position="top left"
                offset={[120, 0]}
                hoverable
                content={
                  <p>
                    {t('page.settings.popup.language', "The language setting specified here is passed to external APIs as your preferred language. This does not indicate the language selected for the user interface.")}
                  </p>
                }
                trigger={
                  <Dropdown
                    name="language"
                    placeholder="English"
                    selection
                    value={settings.language}
                    options={languages}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.currency', "Currency")}</label>
              <Popup
                wide
                position="top left"
                offset={[120, 0]}
                hoverable
                content={
                  <p>
                    {t('page.settings.popup.currency', "The currency setting will be used as your default currency. It is passed to external APIs as your preferred currency.")}
                  </p>
                }
                trigger={
                  <Dropdown
                    name="currency"
                    placeholder="USD"
                    selection
                    value={settings.currency}
                    options={currencies}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('label.maxCacheItems', "Max Cache Items")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.maxCacheItems', "Specify the maximum number of items allowed in the cache.")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.maxCacheItems || 1024}
                    name="maxCacheItems"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('label.cacheAbsoluteExpirationMinutes', "Cache Absolute Expiration")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.cacheAbsoluteExpirationMinutes', "Specify the total minutes in which a cache item will be expired (default: 0).")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.cacheAbsoluteExpirationMinutes || 0}
                    name="cacheAbsoluteExpirationMinutes"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('label.cacheSlidingExpirationMinutes', "Cache Sliding Expiration")}</label>
              <Popup
                wide
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.cacheSlidingExpirationMinutes', "Specify the minutes in which a cache item will be kept if it's touched within the time period (default: 30).")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.cacheSlidingExpirationMinutes || 30}
                    name="cacheSlidingExpirationMinutes"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
          </Segment>

        </Segment>

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
                content={<p>{t('page.settings.popup.swarmApiKey', "Swarm api key is optional. By registering a free or paid api key you will receive higher request limits accordingly.")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.binner.apiKey || ""}
                    name="swarmApiKey"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder="swarm.binner.io"
                    value={(settings.binner.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="swarmApiUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.binner.timeout || ""}
                    name="swarmTimeout"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.digikey.clientId || ""}
                    name="digikeyClientId"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.digikey.clientSecret || ""}
                    name="digikeyClientSecret"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder="sandbox-api.digikey.com"
                    value={(settings.digikey.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="digikeyApiUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
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
                  <ClearableInput
                    className="labeled"
                    placeholder="localhost:8090/Authorization/Authorize"
                    value={(settings.digikey.oAuthPostbackUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="digikeyOAuthPostbackUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.searchApiKey || ""}
                    name="mouserSearchApiKey"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.orderApiKey || ""}
                    name="mouserOrderApiKey"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.mouser.cartApiKey || ""}
                    name="mouserCartApiKey"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder="api.mouser.com"
                    value={(settings.mouser.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="mouserApiUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.arrow.username || ""}
                    name="arrowUsername"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.arrow.apiKey || ""}
                    name="arrowApiKey"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    action
                    className="labeled"
                    placeholder="api.arrow.com"
                    value={(settings.arrow.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="arrowApiUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.octopart.clientId || ""}
                    name="octopartClientId"
                    onChange={handleChange}
                  />
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
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.octopart.clientSecret || ""}
                    name="octopartClientSecret"
                    onChange={handleChange}
                  />
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

          <Segment loading={loading} color="green" secondary>
            <Header dividing as="h3">
              {t('page.settings.tmeelectronics', "TME Electronics")}
            </Header>
            <p>
              <Trans i18nKey="page.settings.tmeDescription">
                TME API Keys can be obtained at <a href="https://developers.tme.eu" target="_blank" rel="noreferrer">https://developers.tme.eu</a>
              </Trans>
            </p>
            <Form.Field width={10}>
              <label>{t('page.settings.tmeSupport', "TME Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.tmeEnabled', "Choose if you would like to enable TME support.")}</p>
                }
                trigger={
                  <Dropdown
                    name="tmeEnabled"
                    placeholder="Disabled"
                    selection
                    value={settings.tme.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.country', "Country")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.country', "Choose the country to pass the API.")}</p>
                }
                trigger={
                  <Dropdown
                    name="tmeCountry"
                    placeholder=""
                    selection
                    value={settings.tme.country || 'us'}
                    options={tmeCountries}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.applicationSecret', "Application Secret")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.tmeApplicationSecret', "Your application secret for TME.")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.tme.applicationSecret || ""}
                    name="tmeApplicationSecret"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiKey', "Api Key")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.tmeApiKey', "Your api key for TME.")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.tme.apiKey || ""}
                    name="tmeApiKey"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            <Form.Field width={10}>
              <label>{t('label.apiUrl', "Api Url")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.tmeApiUrl', "TME's API Url. This will be api.tme.eu")}</p>}
                trigger={
                  <ClearableInput
                    action
                    className="labeled"
                    placeholder="api.tme.eu"
                    value={(settings.tme.apiUrl || "")
                      .replace("http://", "")
                      .replace("https://", "")}
                    name="tmeApiUrl"
                    onChange={handleChange}
                    type="Input"
                  >
                    <Label>https://</Label>
                    <input />
                  </ClearableInput>
                }
              />
            </Form.Field>
            <Form.Field>
              <Button
                primary
                className="test"
                type="button"
                onClick={(e) => handleTestApi(e, "tme")}
                disabled={testing}
              >
                {t('button.testApi', "Test Api")}
              </Button>
              {getTestResultIcon("tme")}
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
            wide
            content={
              <p>{t('page.settings.popup.printMode', "Choose if you would like to print directly to the printer (default) or using the web browser.")}</p>
            }
            trigger={
              <Form.Field width={10}>
                <label>{t('page.settings.printMode', "Print Mode")}</label>
                <Dropdown
                  name="printerPrintMode"
                  placeholder="Select the print mode"
                  selection
                  required
                  value={settings.printer.printMode}
                  options={printModes}
                  onChange={handleChange}
                />
              </Form.Field>
            }
          />
          <Popup
            content={
              <p>{t('page.settings.popup.printerPrinterName', "Your printer name as it appears in Windows, or CUPS (Unix).")}</p>
            }
            trigger={
              <Form.Field width={10}>
                <label>{t('page.settings.printerName', "Printer Name")}</label>
                <ClearableInput
                  className="labeled"
                  placeholder="DYMO LabelWriter 450 Twin Turbo"
                  value={settings.printer.printerName || ""}
                  name="printerPrinterName"
                  onChange={handleChange}
                />
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
                <ClearableInput
                  className="labeled"
                  placeholder="30346"
                  value={settings.printer.partLabelName || ""}
                  name="printerPartLabelName"
                  onChange={handleChange}
                />
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

          <Segment loading={loading} color="blue" raised padded>
            <Header dividing as="h3">
              {t('page.settings.barcodeConfiguration', "Barcode Configuration")}
            </Header>

            <Form.Field width={10}>
              <label>{t('page.settings.barcodeSupport', "Barcode Support")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.barcodeSupportEnabled', "Choose if you would like to enable barcode support.")}</p>
                }
                trigger={
                  <Dropdown
                    name="barcodeEnabled"
                    placeholder="Disabled"
                    selection
                    value={settings.barcode.enabled ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.isBarcodeDebug', "Debug Mode")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.barcodeIsDebug', "Enabling Barcode debug mode will print diagnostic information to the browser console.")}</p>
                }
                trigger={
                  <Dropdown
                    name="barcodeIsDebug"
                    placeholder="Disabled"
                    selection
                    value={settings.barcode.isDebug ? 1 : 0}
                    options={enabledSources}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>
            
            <Form.Field width={10}>
              <label>{t('page.settings.bufferTime', "Buffer Time (ms)")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.barcodeBufferTime', "The buffer time in milliseconds the barcode will scan data for. Some scanners require more or less time. Default: 80")}</p>}
                trigger={
                  <ClearableInput
                    icon="clock"
                    className="labeled"
                    placeholder=""
                    value={settings.barcode.bufferTime || ""}
                    name="barcodeBufferTime"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.maxKeystrokeThresholdMs', "Max Keystroke Threshold (ms)")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.maxKeystrokeThresholdMs', "The maximum amount of time in milliseconds to wait between keypresses.")}</p>}
                trigger={
                  <ClearableInput
                    icon="clock"
                    className="labeled"
                    placeholder=""
                    value={settings.barcode.maxKeystrokeThresholdMs || ""}
                    name="barcodeMaxKeystrokeThresholdMs"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.prefix2d', "2D Barcode Prefix")}</label>
              <Popup
                position="top left"
                offset={[65, 0]}
                hoverable
                content={<p>{t('page.settings.popup.prefix2d', "The prefix used for 2D barcodes, usually '[)>'.")}</p>}
                trigger={
                  <ClearableInput
                    className="labeled"
                    placeholder=""
                    value={settings.barcode.prefix2D || ""}
                    name="barcodePrefix2D"
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

            <Form.Field width={10}>
              <label>{t('page.settings.barcodeProfile', "Barcode Profile")}</label>
              <Popup
                wide
                position="top left"
                offset={[130, 0]}
                hoverable
                content={
                  <p>{t('page.settings.popup.barcodeProfile', "Choose the barcode profile to use for your scanner.")}</p>
                }
                trigger={
                  <Dropdown
                    name="barcodeProfile"
                    placeholder="Default"
                    selection
                    value={settings.barcode.profile}
                    options={barcodeProfileOptions}
                    onChange={handleChange}
                  />
                }
              />
            </Form.Field>

          </Segment>

          <Form.Field inline>
            <Button type="submit" primary style={{ marginTop: "10px" }} disabled={!isDirty}>
              <Icon name="save" />
              {t('button.save', "Save")}
            </Button>
            {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
          </Form.Field>

          <div className="sticky-target" style={{padding: '10px 10px 20px 10%'}} data-bounds={"0.05,0.8"}>
            <Form.Field inline>
              <Button type="submit" primary style={{ marginTop: "10px" }} disabled={!isDirty}>
                <Icon name="save" />
                {t('button.save', "Save")}
              </Button>
              {saveMessage.length > 0 && (
                <Label pointing="left">{saveMessage}</Label>
              )}
            </Form.Field>
          </div>
        </Segment>
        
      </Form>
    </div>
  );
};
