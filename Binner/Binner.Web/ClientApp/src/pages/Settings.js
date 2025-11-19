import { useState, useEffect, useMemo } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Icon, Label, Button, Form, Segment, Header, Popup, Dropdown, Confirm, Breadcrumb, Table, Tab, TabPane, Checkbox } from "semantic-ui-react";
import ClearableInput from "../components/ClearableInput";
import { BarcodeProfiles, GetAdvancedTypeDropdown, GetTypeDropdown, GetTypeName } from "../common/Types";
import { isAdmin } from "../common/authentication";
import { DigiKeySites } from "../common/digiKeySites";
import { TmeCountries } from "../common/tmeCountries";
import { Element14Countries } from "../common/element14Countries";
import { FormHeader } from "../components/FormHeader";
import { fetchApi } from "../common/fetchApi";
import { setSystemSettings } from "../common/applicationSettings";
import { toast } from "react-toastify";
import { Languages } from "../common/Languages";
import { Currencies } from "../common/Currencies";
import { KiCadExportPartFields } from "../common/KiCadExportPartFields";
import { ExportPartFields } from "../common/ExportPartFields";
import { CustomFieldTypes } from "../common/customFieldTypes";
import { config } from "../common/config";
import "./Settings.css";

export const Settings = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  const [loading, setLoading] = useState(true);
  const [currentTab, setCurrentTab] = useState(0);
  const [saveMessage, setSaveMessage] = useState("");
  const [testing, setTesting] = useState(false);
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationApiName, setAuthorizationApiName] = useState('');
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const [isDirty, setIsDirty] = useState(false);
  const [confirmDeleteCustomFieldIsOpen, setConfirmDeleteCustomFieldIsOpen] = useState(false);
  const [confirmDeleteCustomFieldContent, setConfirmDeleteCustomFieldContent] = useState(null);
  const [selectedCustomField, setSelectedCustomField] = useState(null);
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
  const languageOptions = Languages;
  const currencyOptions = Currencies;
  const [kiCadExportFieldOptions, setKiCadExportFieldOptions] = useState(KiCadExportPartFields);
  const exportFieldOptions = ExportPartFields;
  const barcodeProfileOptions = GetTypeDropdown(BarcodeProfiles);
  const customFieldTypeOptions = GetAdvancedTypeDropdown(CustomFieldTypes);
  const digikeySites = GetAdvancedTypeDropdown(DigiKeySites);
  const noFlags = ['ss', 'sk', 'sx', 'mf', 'bl', 'im', 'xk', 'je', 'gg', 'cw', 'bq', 'aq'];
  const tmeCountries = TmeCountries.map((i, key) => ({
    key,
    value: i.iso2,
    text: i.name,
    flag: !noFlags.includes(i.iso2.toLowerCase()) ? i.iso2.toLowerCase() : ""
  }));
  const element14Countries = Element14Countries.map((i, key) => ({
    key,
    value: i.name,
    text: i.name,
    flag: i.iso2 != null ? (!noFlags.includes(i.iso2.toLowerCase()) ? i.iso2.toLowerCase() : "") : ""
  }));
  // breakup settings into multiple objects to improve render memoization
  const [globalSettings, setGlobalSettings] = useState({
    licenseKey: "",
    language: "",
    currency: "",
    maxCacheItems: 1024,
    cacheAbsoluteExpirationMinutes: 0,
    cacheSlidingExpirationMinutes: 30,
    enableAutoPartSearch: true,
    enableDarkMode: false,
    enableCheckNewVersion: true,
    enableAutomaticMetadataFetchingForExistingParts: true,
  });
  const [integrationSettings, setIntegrationSettings] = useState({
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
    tme: {
      enabled: false,
      applicationSecret: "",
      apiKey: "",
      apiUrl: "",
      country: "us",
      resolveExternalLinks: true
    },
    element14: {
      enabled: false,
      apiKey: "",
      apiUrl: "",
      country: "uk.farnell.com"
    },
});
  const [printerSettings, setPrinterSettings] = useState({
    printer: {
      printMode: 0,
      printerName: "",
      partLabelSource: "",
      partLabelName: "",
    }
  });
  const [barcodeSettings, setBarcodeSettings] = useState({
    barcode: {
      enabled: true,
      isDebug: false,
      maxKeystrokeThresholdMs: 300,
      bufferTime: 80,
      profile: BarcodeProfiles.Default,
      prefix2D: "[)>",
    }
  });
  const [customFieldSettings, setCustomFieldSettings] = useState({
    customFields: []
  });
  const [kiCadSettings, setKiCadSettings] = useState({
    kiCad: {
      enabled: false,
      exportFields: []
    }
  });
  const [apiTestResults, setApiTestResults] = useState([]);

  useEffect(() => {
    const loadSettings = async () => {
      await fetchApi("/api/settings", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((response) => {
        const { data } = response;
        setLoading(false);
        // break out data into multiple state variables to optimize render performance
        const { licenseKey, maxCacheItems, cacheAbsoluteExpirationMinutes, cacheSlidingExpirationMinutes, enableAutoPartSearch, enableDarkMode, enableCheckNewVersion, enableAutomaticMetadataFetchingForExistingParts } = data;
        const language = data.locale.language;
        const currency = data.locale.currency;
        setGlobalSettings({ licenseKey, language, currency, maxCacheItems, cacheAbsoluteExpirationMinutes, cacheSlidingExpirationMinutes, enableAutoPartSearch, enableDarkMode, enableCheckNewVersion, enableAutomaticMetadataFetchingForExistingParts });
        const { binner, digikey, mouser, arrow, octopart, tme, element14 } = data;
        setIntegrationSettings({ binner, digikey, mouser, arrow, octopart, tme, element14 });
        setPrinterSettings({ printer: data.printer });
        setBarcodeSettings({ barcode: data.barcode });
        setCustomFieldSettings({ customFields: data.customFields });
        setKiCadSettings({ kiCad: data.kiCad })
        setSystemSettings(data);
      });
    };

    loadSettings();
  }, []);

  useEffect(() => {
    var isReturningFromApi = searchParams.get("api-authenticate");
    if (isReturningFromApi === 'true') {
      var apiName = searchParams.get("api");
      setCurrentTab(1);
      setTimeout(() => {
        const element = document.getElementById(`api-${apiName.toLowerCase()}`);
        if (element) element.scrollIntoView({ behavior: 'smooth' });
      }, [100]);
    }
  }, [searchParams]);

  /**
   * Save the system settings
   *
   * @param {any} e
   */
  const onSubmit = async (e) => {
    const newSettings = {
      ...globalSettings,
      ...integrationSettings,
      ...printerSettings,
      ...kiCadSettings,
      locale: {
        currency: globalSettings.currency,
        language: globalSettings.language,
      },
      customFields: [ ..._.filter(customFieldSettings.customFields, x => x.name?.length > 0).map((cf) => ({...cf, name: cf.name.trim()})) ],
      barcode: {
        ...barcodeSettings, 
        bufferTime: parseInt(barcodeSettings.barcode.bufferTime), 
        maxKeystrokeThresholdMs: parseInt(barcodeSettings.barcode.maxKeystrokeThresholdMs)
      }};
    const response = await saveSettings(e, newSettings);
    if (response.responseObject.ok) {
      setSystemSettings(newSettings);
      const saveMessage = t('success.systemSettingsSaved', "System settings were saved.");
      toast.success(saveMessage);
      setSaveMessage(saveMessage);
      navigate(-1);
    } else {
      const errorMessage = t('success.failedToSaveSettings', "Failed to save settings!");
      toast.error(errorMessage);
      setSaveMessage(saveMessage);
    }
  };

  const saveSettings = async (e, newSettings) => {
    return await fetchApi("/api/settings", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(newSettings),
    });
  };

  const handleIntegrationSettingsChange = (e, control) => {
    setIntegrationSettings({ ...integrationSettings });
    const newSettings = { ...integrationSettings };
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
    else if (control.name.startsWith("element14")) {
      setControlValue(newSettings.element14, "element14", control);
    }
    setIntegrationSettings(newSettings);
  };

  const handleGlobalSettingsChange = (e, control) => {
    const newSettings = { ...globalSettings };
    setControlValue(newSettings, "", control);
    setGlobalSettings(newSettings);
  };

  const handleBarcodeSettingsChange = (e, control) => {
    const newSettings = { ...barcodeSettings };
    if (control.name.startsWith("barcode")) {
      setControlValue(newSettings.barcode, "barcode", control);
    }
    setBarcodeSettings(newSettings);
  };

  const handlePrinterSettingsChange = (e, control) => {
    const newSettings = { ...printerSettings };
    if (control.name.startsWith("printer")) {
        setControlValue(newSettings.printer, "printer", control);
    }
    setPrinterSettings(newSettings);
  };

  const handleCustomFieldSettingsChange = (e, control, field) => {
    const newSettings = { ...customFieldSettings };
    newSettings[control.name] = control.value;
    setControlValue(field, "customFields", control);
    setCustomFieldSettings(newSettings);
  };

  const handleKiCadSettingsChange = (e, control) => {
    const newSettings = { ...kiCadSettings };
    if (control.name.startsWith("kiCad")) {
      setControlValue(newSettings.kiCad, "kiCad", control);
    }
    setKiCadSettings(newSettings);
  };

  const handleChange = (e, control, settingsName, field) => {
    switch(settingsName) {
      case 'global':
        handleGlobalSettingsChange(e, control);
        break;
      case 'integration':
        handleIntegrationSettingsChange(e, control);
        break;
      case 'customFields':
        handleCustomFieldSettingsChange(e, control, field);
        break;
      case 'kiCad':
        handleKiCadSettingsChange(e, control, field);
        break;
      case 'printer':
        handlePrinterSettingsChange(e, control);
        break;
      case 'barcode':
        handleBarcodeSettingsChange(e, control);
        break;
      default:
        console.error(`Unknown setting type: ${settingsName}`);
        break;
    }
    setIsDirty(true);
  };

  const setControlValue = (setting, name, control) => {
    switch (control.type) {
      case "checkbox":
        // type is a checkbox, we only care about the checked value
        setting[getControlInstanceName(control, name)] = control.checked;
        break;
      case "bool-dropdown":
        // bool dropdowns
        setting[getControlInstanceName(control, name)] = control.value > 0 ? true : false;
        break;
      default:
        // this is a text input or any other value type control
        setting[getControlInstanceName(control, name)] = control.value;
        break;
    }
  };

  const handleAddCustomField = (e) => {
    customFieldSettings.customFields.push({ customFieldId: 0, name: '', description: '', customFieldTypeId: CustomFieldTypes.Inventory.value, isNew: true });
    setCustomFieldSettings({ ...customFieldSettings });
  };

  const handleDeleteCustomField = (e, targetField) => {
    e.preventDefault();
    e.stopPropagation();
    const field = selectedCustomField || targetField;
    const newCustomFieldSettings = { 
      customFields: _.filter(customFieldSettings.customFields, x => !(x.customFieldId === field?.customFieldId && x.customFieldTypeId === field?.customFieldTypeId && x.name === field?.name.trim()))
    };
    setCustomFieldSettings(newCustomFieldSettings);
    if (field?.customFieldId !== 0) setIsDirty(true);
    setConfirmDeleteCustomFieldIsOpen(false);
    setSelectedCustomField(null);
  };

  const closeDeleteCustomField = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteCustomFieldIsOpen(false);
    setSelectedCustomField(null);
  };

  const openDeleteCustomField = (e, customField) => {
    e.preventDefault();
    e.stopPropagation();
    if (customField.customFieldId > 0) {
      setSelectedCustomField(customField);
      setConfirmDeleteCustomFieldContent(
        <p>
          <Trans i18nKey="confirm.deleteCustomField" name={customField.name}>
            Are you sure you want to delete custom field <b>{{ name: customField.name }}</b>?
          </Trans>
          <br />
          <br />
          <Trans i18nKey="confirm.deleteCustomFieldValues">
            <i><u>All of the values</u></i> will be deleted for the custom field.
          </Trans>
          <br />
          <br />
          <Trans i18nKey="confirm.permanent">
            This action is <i>permanent and cannot be recovered</i>.
          </Trans>
        </p>
      );
      setConfirmDeleteCustomFieldIsOpen(true);
    } else {
      handleDeleteCustomField(e, customField);
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
    fetchApi("/api/settings/forgetcredentials", {
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
        if (!integrationSettings.binner.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.binner.apiKey) return toast.error(t('apiNoApiKey', "Api Key must be specified!"));
        if (!integrationSettings.binner.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.binner.enabled + "" });
        configuration.push({ key: "apiKey", value: integrationSettings.binner.apiKey });
        configuration.push({ key: "apiUrl", value: integrationSettings.binner.apiUrl });
        configuration.push({ key: "timeout", value: integrationSettings.binner.timeout });
        break;
      case "digikey":
        if (!integrationSettings.digikey.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.digikey.clientId) return toast.error(t('apiNoClientId', "Client Id must be specified!"));
        if (!integrationSettings.digikey.clientSecret) return toast.error(t('apiNoClientSecret', "Client Secret must be specified!"));
        if (!integrationSettings.digikey.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        if (!integrationSettings.digikey.oAuthPostbackUrl) return toast.error(t('apiNoPostbackUrl', "Postback Url must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.digikey.enabled + "" });
        configuration.push({ key: "site", value: integrationSettings.digikey.site + "" });
        configuration.push({ key: "clientId", value: integrationSettings.digikey.clientId });
        configuration.push({ key: "clientSecret", value: integrationSettings.digikey.clientSecret });
        configuration.push({ key: "apiUrl", value: integrationSettings.digikey.apiUrl });
        configuration.push({ key: "oAuthPostbackUrl", value: integrationSettings.digikey.oAuthPostbackUrl });
        break;
      case "mouser":
        if (!integrationSettings.mouser.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.mouser.searchApiKey && !integrationSettings.mouser.orderApiKey && !integrationSettings.mouser.cartApiKey) return toast.error(t('apiNoKey', "At least one api key must be specified!"));
        if (!integrationSettings.mouser.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.mouser.enabled + "" });
        configuration.push({ key: "searchApiKey", value: integrationSettings.mouser.searchApiKey });
        configuration.push({ key: "orderApiKey", value: integrationSettings.mouser.orderApiKey });
        configuration.push({ key: "cartApiKey", value: integrationSettings.mouser.cartApiKey });
        configuration.push({ key: "apiUrl", value: integrationSettings.mouser.apiUrl });
        break;
      case "arrow":
        if (!integrationSettings.arrow.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.mouser.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.arrow.enabled + "" });
        configuration.push({ key: "username", value: integrationSettings.arrow.username });
        configuration.push({ key: "apiKey", value: integrationSettings.arrow.apiKey });
        configuration.push({ key: "apiUrl", value: integrationSettings.arrow.apiUrl });
        break;
      case "octopart":
        if (!integrationSettings.octopart.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.octopart.clientId) return toast.error(t('apiNoClientId', "Client Id must be specified!"));
        if (!integrationSettings.octopart.clientSecret) return toast.error(t('apiNoClientSecret', "Client Secret must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.octopart.enabled + "" });
        configuration.push({ key: "clientId", value: integrationSettings.octopart.clientId });
        configuration.push({ key: "clientSecret", value: integrationSettings.octopart.clientSecret });
        break;
      case "tme":
        if (!integrationSettings.tme.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.tme.applicationSecret) return toast.error(t('apiNoAppSecret', "Application Secret must be specified!"));
        if (!integrationSettings.tme.apiKey) return toast.error(t('apiNoApiKey', "Api Key must be specified!"));
        if (!integrationSettings.tme.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        if (!integrationSettings.tme.country) return toast.error(t('apiNoCountry', "Country must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.tme.enabled + "" });
        configuration.push({ key: "applicationSecret", value: integrationSettings.tme.applicationSecret });
        configuration.push({ key: "country", value: integrationSettings.tme.country });
        configuration.push({ key: "apiKey", value: integrationSettings.tme.apiKey });
        configuration.push({ key: "apiUrl", value: integrationSettings.tme.apiUrl });
        configuration.push({ key: "resolveExternalLinks", value: integrationSettings.tme.resolveExternalLinks + "" });
        break;
      case "element14":
        if (!integrationSettings.element14.enabled) return toast.error(t('apiNotEnabled', "Api is not enabled!"));
        if (!integrationSettings.element14.apiKey) return toast.error(t('apiNoApiKey', "Api Key must be specified!"));
        if (!integrationSettings.element14.apiUrl) return toast.error(t('apiNoApiUrl', "Api Url must be specified!"));
        if (!integrationSettings.element14.country) return toast.error(t('apiNoCountry', "Country must be specified!"));
        configuration.push({ key: "enabled", value: integrationSettings.element14.enabled + "" });
        configuration.push({ key: "country", value: integrationSettings.element14.country });
        configuration.push({ key: "apiKey", value: integrationSettings.element14.apiKey });
        configuration.push({ key: "apiUrl", value: integrationSettings.element14.apiUrl });
        break;
      default:
        break;
    }

    // save settings first, as the backend must be aware of things like DigiKey's clientId/clientSecret
    const newSettings = {
      ...globalSettings,
      ...integrationSettings,
      ...printerSettings,
      locale: {
        currency: globalSettings.currency,
        language: globalSettings.language,
      },
      customFields: [..._.filter(customFieldSettings.customFields, x => x.name?.length > 0).map((cf) => ({ ...cf, name: cf.name.trim() }))],
      barcode: {
        ...barcodeSettings,
        bufferTime: parseInt(barcodeSettings.barcode.bufferTime),
        maxKeystrokeThresholdMs: parseInt(barcodeSettings.barcode.maxKeystrokeThresholdMs)
      }
    };
    await saveSettings(e, newSettings);

    const request = {
      name: apiName,
      configuration
    };
    setTesting(true);
    await fetchApi("/api/settings/testapi", {
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

  const handleKiCadFieldSettingsChange = (e, control, field) => {
    const exportFields = kiCadSettings.kiCad.exportFields.map((f) => {
      if (f.field === field.field && f.kiCadFieldName === field.kiCadFieldName) {
        switch(control.name){
          case 'enabled':
            f.enabled = control.checked;
            break;
          case 'field':
            f.field = control.value;
            break;
          case 'kiCadFieldName':
            f.kiCadFieldName = control.value;
            break;
        }
      }
      return f;
    });
    setKiCadSettings({...kiCadSettings, kiCad: { ...kiCadSettings.kiCad, exportFields } });
    setIsDirty(true);
  };

  const handleAddKiCadExportField = (e, { value }) => {
    const newOption = {
      key: _.max(kiCadExportFieldOptions, i=>i.key) + 1,
      text: value,
      value
    };
    setKiCadExportFieldOptions([...kiCadExportFieldOptions, newOption]);
    setIsDirty(true);
  };

  const userSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
      <Header dividing as="h3">
        {t('page.settings.application', "Application")}
      </Header>

      <Form.Field width={10}>
        <label>{t('page.settings.language', "Api Language")}</label>
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
              value={globalSettings.language || 0}
              options={languageOptions}
              onChange={(e, control) => handleChange(e, control, 'global')}
            />
          }
        />
      </Form.Field>

      <Form.Field width={10}>
        <label>{t('page.settings.currency', "Api Currency")}</label>
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
              value={globalSettings.currency || 0}
              options={currencyOptions}
              onChange={(e, control) => handleChange(e, control, 'global')}
            />
          }
        />
      </Form.Field>

      <Form.Group>
        <Form.Field>
          <label>{t('page.settings.enableAutoSearch', "Enable Auto Search")}</label>
          <Popup
            wide
            position="top left"
            offset={[0, 20]}
            hoverable
            content={<Trans i18nKey="page.settings.popup.enableAutoPartSearch">
              Select this option to enable auto searching of part details when adding a new part.
            </Trans>}
            trigger={
              <Form.Checkbox 
                name="enableAutoPartSearch"
                type="checkbox"
                toggle
                checked={globalSettings.enableAutoPartSearch}
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
            }
          />
        </Form.Field>
        {/*<Form.Field>
          <label>{t('page.settings.enableDarkMode', "Enable Dark Mode")}</label>
          <Popup
            wide
            position="top left"
            offset={[0, 20]}
            hoverable
            content={<Trans i18nKey="page.settings.popup.enableDarkMode">
              Select this option to enable dark mode across all sessions.
            </Trans>}
            trigger={
              <Form.Checkbox
                name="enableDarkMode"
                type="checkbox"
                toggle
                checked={globalSettings.enableDarkMode}
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
            }
          />
        </Form.Field>*/}
        <Form.Field>
          <label>{t('page.settings.enableCheckNewVersion', "Enable Check for new version")}</label>
          <Popup
            wide
            position="top left"
            offset={[0, 20]}
            hoverable
            content={<Trans i18nKey="page.settings.popup.enableCheckNewVersion">
              Select this option to enable checking for a new version of Binner
            </Trans>}
            trigger={
              <Form.Checkbox
                name="enableCheckNewVersion"
                type="checkbox"
                toggle
                checked={globalSettings.enableCheckNewVersion}
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
            }
          />
        </Form.Field>
      </Form.Group>
    </Segment>);
  }, [globalSettings, loading]);

  const organizationSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
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
              <div>
              <ClearableInput
                className="labeled"
                placeholder=""
                value={globalSettings.licenseKey || ""}
                name="licenseKey"
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
              </div>
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
              <div>
              <ClearableInput
                className="labeled"
                placeholder=""
                value={globalSettings.maxCacheItems || 1024}
                name="maxCacheItems"
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
              </div>
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
              <div>
              <ClearableInput
                className="labeled"
                placeholder=""
                value={globalSettings.cacheAbsoluteExpirationMinutes || 0}
                name="cacheAbsoluteExpirationMinutes"
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
              </div>
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
              <div>
              <ClearableInput
                className="labeled"
                placeholder=""
                value={globalSettings.cacheSlidingExpirationMinutes || 30}
                name="cacheSlidingExpirationMinutes"
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
              </div>
            }
          />
        </Form.Field>

        <Form.Field>
          <label>{t('page.settings.enableAutomaticMetadataFetchingForExistingParts', "Enable automatic updating of metadata using the APIs for existing parts")}</label>
          <Popup
            wide
            position="top left"
            offset={[0, 20]}
            hoverable
            content={<Trans i18nKey="page.settings.popup.enableAutomaticMetadataFetchingForExistingParts">
              Select this option to enable automatic updating using the part search APIs to fetch metadata for existing parts in your inventory when opening them in the inventory.
            </Trans>}
            trigger={
              <Form.Checkbox 
                name="enableAutomaticMetadataFetchingForExistingParts"
                type="checkbox"
                toggle
                checked={globalSettings.enableAutomaticMetadataFetchingForExistingParts}
                onChange={(e, control) => handleChange(e, control, 'global')}
              />
            }
          />
        </Form.Field>
      </Segment>

    </Segment>);
  }, [globalSettings, loading]);

  const integrationSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
      <Header dividing as="h3">
        {t('page.settings.integrations', "Integrations")}
      </Header>
      <p>
        <i>
          <Trans i18nKey="page.settings.integrationsDescription">
            To integrate with DigiKey, Mouser, Arrow or Octopart/Nexar API&apos;s you must
            obtain API keys for each service you wish to use.
            <br />
            Adding integrations will greatly enhance your experience.
          </Trans>
        </i>
      </p>

      <Segment loading={loading} color="green" secondary id="api-swarm">
        <Header dividing as="h3">
          {t('page.settings.swarm', "Swarm")}
        </Header>
        <p>
          <Trans i18nKey="page.settings.swarmDescription">
            Swarm is a free API service provided by <a href="https://binner.io" target="_blank" rel="noreferrer">Binner&apos;s cloud service</a> that
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
                type="bool-dropdown"
                placeholder="Enabled"
                selection
                value={integrationSettings.binner.enabled ? 1 : 0}
                className={!integrationSettings.binner.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiKey', "Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.binner.apiKey || ""}
            name="swarmApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.swarmApiKey', "Swarm api key is optional. By registering a free or paid api key you will receive higher request limits accordingly.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiUrl', "Api Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder="swarm.binner.io"
            value={(integrationSettings.binner.apiUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="swarmApiUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpHoverable
            help={<p>{t('page.settings.popup.swarmApiUrl', "Swarm's API Url")}</p>}
            disabled={config.BINNERIO === "true"}
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.timeout', "Timeout")}</label>
            <ClearableInput
              className="labeled"
              placeholder=""
              value={integrationSettings.binner.timeout || ""}
              name="swarmTimeout"
              onChange={(e, control) => handleChange(e, control, 'integration')}
              helpWide
              helpHoverable
              help={<p>{t('page.settings.popup.swarmTimeout', "Swarm api request timeout. Default: '00:00:05' (5 seconds)")}</p>}
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

      <Segment loading={loading} color="green" secondary id="api-digikey">
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
                type="bool-dropdown"
                placeholder="Enabled"
                selection
                value={integrationSettings.digikey.enabled ? 1 : 0}
                className={!integrationSettings.digikey.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
                value={integrationSettings.digikey.site || 0}
                options={digikeySites}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.clientId', "Client Id")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.digikey.clientId || ""}
            name="digikeyClientId"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={
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
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.clientSecret', "Client Secret")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.digikey.clientSecret || ""}
            name="digikeyClientSecret"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={
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
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiUrl', "Api Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder="sandbox-api.digikey.com"
            value={(integrationSettings.digikey.apiUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="digikeyApiUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpWideVery
            helpHoverable
            help={
              <p>
                <Trans i18nKey={"page.settings.popup.digikeyApiUrl"}>
                  DigiKey&apos;s API Url. This will either be <i>api.digikey.com</i> (live) or <i>sandbox-api.digikey.com</i> (for testing only)
                </Trans>
              </p>
            }
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.postbackUrl', "Postback Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder={`${config.API_URL.replace('https://','')}/Authorization/Authorize`}
            value={(integrationSettings.digikey.oAuthPostbackUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="digikeyOAuthPostbackUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpWideVery
            helpHoverable
            help={
              <div>
                <Trans i18nKey={"page.settings.popup.digikeyOAuthPostbackUrl"}>
                  Binner&apos;s postback url must be registered with DigiKey exactly as specified here, on DigiKey this is named <b>Callback URL</b>.
                  This should almost always be localhost, and no firewall settings are required as your web browser will be making the request.
                  <br /><br />
                  <b>Example: </b><i>{config.API_URL.replace('https://','')}/Authorization/Authorize</i>
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
            disabled={config.BINNERIO === "true"}
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
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

      <Segment loading={loading} color="green" secondary id="api-mouser">
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
                type="bool-dropdown"
                placeholder="Enabled"
                selection
                value={integrationSettings.mouser.enabled ? 1 : 0}
                className={!integrationSettings.mouser.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('page.settings.searchApiKey', "Search Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.mouser.searchApiKey || ""}
            name="mouserSearchApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.mouserSearchApiKey', "Your api key for accessing the search api.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('page.settings.ordersApiKey', "Orders Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.mouser.orderApiKey || ""}
            name="mouserOrderApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.mouserOrderApiKey', "Your api key for accessing the orders api.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('page.settings.cartApiKey', "Cart Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.mouser.cartApiKey || ""}
            name="mouserCartApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.mouserCartApiKey', "Your api key for accessing the shopping cart api.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiUrl', "Api Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder="api.mouser.com"
            value={(integrationSettings.mouser.apiUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="mouserApiUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.mouserApiUrl', "Mouser's API Url. This will be api.mouser.com")}</p>}
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
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

      <Segment loading={loading} color="green" secondary id="api-arrow">
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
                type="bool-dropdown"
                placeholder="Disabled"
                selection
                value={integrationSettings.arrow.enabled ? 1 : 0}
                className={!integrationSettings.arrow.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.username', "Username")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.arrow.username || ""}
            name="arrowUsername"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.arrowUsername', "Your username/login for Arrow.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiKey', "Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.arrow.apiKey || ""}
            name="arrowApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.arrowApiKey', "Your api key for Arrow.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiUrl', "Api Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder="api.arrow.com"
            value={(integrationSettings.arrow.apiUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="arrowApiUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.arrowApiUrl', "Arrow's API Url. This will be api.arrow.com")}</p>}
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
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

      <Segment loading={loading} color="green" secondary id="api-nexar">
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
                type="bool-dropdown"
                placeholder="Disabled"
                selection
                value={integrationSettings.octopart.enabled ? 1 : 0}
                className={!integrationSettings.octopart.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.clientId', "Client Id")}</label>          
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.octopart.clientId || ""}
            name="octopartClientId"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.octopartClientId', "Your Client Id for Octopart/Nexar.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.clientSecret', "Client Secret")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.octopart.clientSecret || ""}
            name="octopartClientSecret"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.octopartClientSecret', "Your Client Secret for Octopart/Nexar.")}</p>}
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

      <Segment loading={loading} color="green" secondary id="api-tme">
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
                type="bool-dropdown"
                placeholder="Disabled"
                selection
                value={integrationSettings.tme.enabled ? 1 : 0}
                className={!integrationSettings.tme.enabled ? "bool-disabled" : ""}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
                value={integrationSettings.tme.country || 'us'}
                options={tmeCountries}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
            }
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.applicationSecret', "Application Secret")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.tme.applicationSecret || ""}
            name="tmeApplicationSecret"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.tmeApplicationSecret', "Your application secret for TME.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiKey', "Api Key")}</label>
          <ClearableInput
            className="labeled"
            placeholder=""
            value={integrationSettings.tme.apiKey || ""}
            name="tmeApiKey"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.tmeApiKey', "Your api key for TME.")}</p>}
          />
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('label.apiUrl', "Api Url")}</label>
          <ClearableInput
            className="labeled"
            placeholder="api.tme.eu"
            value={(integrationSettings.tme.apiUrl || "")
              .replace("http://", "")
              .replace("https://", "")}
            name="tmeApiUrl"
            onChange={(e, control) => handleChange(e, control, 'integration')}
            type="Input"
            helpWide
            helpHoverable
            help={<p>{t('page.settings.popup.tmeApiUrl', "TME's API Url. This will be api.tme.eu")}</p>}
          >
            <Label>https://</Label>
            <input />
          </ClearableInput>
        </Form.Field>
        <Form.Field width={10}>
          <label>{t('page.settings.tmeResolveExternalLinks', "Resolve External Links")}</label>
          <Popup
            wide
            position="top left"
            offset={[130, 0]}
            hoverable
            content={
              <p>{t('page.settings.popup.tmeResolveExternalLinks', "Choose if you would like to resolve external document links (slower).")}</p>
            }
            trigger={
              <Dropdown
                name="tmeResolveExternalLinks"
                type="bool-dropdown"
                placeholder="Disabled"
                selection
                value={integrationSettings.tme.resolveExternalLinks ? 1 : 0}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
              />
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

      <Segment loading={loading} color="green" secondary>
        <Header dividing as="h3">
          {t('page.settings.element14', "Farnell/Newark/Element14")}
        </Header>
        <p>
          <Trans i18nKey="page.settings.element14Description">
            Element14 API Keys can be obtained at <a href="https://partner.element14.com" target="_blank" rel="noreferrer">https://partner.element14.com</a>
          </Trans>
        </p>
        <Form.Field width={10}>
          <label>{t('page.settings.element14Support', "Element14 Support")}</label>
          <Popup
            wide
            position="top left"
            offset={[130, 0]}
            hoverable
            content={
              <p>{t('page.settings.popup.element14Enabled', "Choose if you would like to enable Element14 support.")}</p>
            }
            trigger={
              <Dropdown
                name="element14Enabled"
                placeholder="Disabled"
                type="bool-dropdown"
                selection
                value={integrationSettings.element14.enabled ? 1 : 0}
                options={enabledSources}
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
                name="element14Country"
                placeholder=""
                selection
                value={integrationSettings.element14.country || 'uk.farnell.com'}
                options={element14Countries}
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
            content={<p>{t('page.settings.popup.element14ApiKey', "Your api key for Element14.")}</p>}
            trigger={
              <ClearableInput
                className="labeled"
                placeholder=""
                value={integrationSettings.element14.apiKey || ""}
                name="element14ApiKey"
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
            content={<p>{t('page.settings.popup.element14ApiUrl', "Element14's API Url. This will be api.element14.com")}</p>}
            trigger={
              <ClearableInput
                action
                className="labeled"
                placeholder="api.element14.com"
                value={(integrationSettings.element14.apiUrl || "")
                  .replace("http://", "")
                  .replace("https://", "")}
                name="element14ApiUrl"
                onChange={(e, control) => handleChange(e, control, 'integration')}
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
            onClick={(e) => handleTestApi(e, "element14")}
            disabled={testing}
          >
            {t('button.testApi', "Test Api")}
          </Button>
          {getTestResultIcon("element14")}
        </Form.Field>
      </Segment>

    </Segment>);
  }, [integrationSettings, apiTestResults, loading]);

  const customFieldSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
      <Header dividing as="h3">
        {t('page.settings.customFields', "Custom Fields")}
      </Header>
      <p>
        <i>
          {t('page.settings.customFieldsDescription', "Add custom fields to store along with your data. This allows for extensions of Inventory, BOM Projects, etc.")}
        </i>
      </p>

      <div style={{ float: 'right', marginBottom: '5px' }}>
        <Button type='button' size='tiny' onClick={handleAddCustomField}><Icon name="add" /> Add</Button>
      </div>
      <Table compact celled sortable selectable striped size='small'>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell width={5}>Name</Table.HeaderCell>
            <Table.HeaderCell width={7}>Description</Table.HeaderCell>
            <Table.HeaderCell width={3}>Type</Table.HeaderCell>
            <Table.HeaderCell width={1}></Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {customFieldSettings.customFields?.length > 0
            ? customFieldSettings.customFields.map((customField, customFieldsKey) => (
              !customField.isNew
                ? <Table.Row key={customFieldsKey}>
                  <Table.Cell >{customField.name}</Table.Cell>
                  <Table.Cell>{customField.description}</Table.Cell>
                  <Table.Cell>{GetTypeName(CustomFieldTypes, customField.customFieldTypeId)}</Table.Cell>
                  <Table.Cell textAlign="center"><Button type="button" icon="delete" size='tiny' onClick={(e) => openDeleteCustomField(e, customField)} /></Table.Cell>
                </Table.Row>
                : <Table.Row key={customFieldsKey}>
                  <Table.Cell>
                    <Form.Field>
                      <label>{t('label.name', "Name")}</label>
                      <Popup
                        wide
                        position="top left"
                        offset={[65, 0]}
                        hoverable
                        content={t('page.settings.popup.customFieldName', "Enter the name of the custom field.")}
                        trigger={
                          <ClearableInput
                            className="labeled"
                            placeholder="ROHS"
                            value={customField.name || ""}
                            name="name"
                            onChange={(e, control) => handleChange(e, control, 'customFields', customField)}
                          />
                        }
                      />
                    </Form.Field>
                  </Table.Cell>
                  <Table.Cell>
                    <Form.Field>
                      <label>{t('label.description', "Description")}</label>
                      <Popup
                        wide
                        position="top left"
                        offset={[65, 0]}
                        hoverable
                        content={t('page.settings.popup.customFieldDescription', "Add a description to identify the custom field.")}
                        trigger={
                          <ClearableInput
                            className="labeled"
                            placeholder="Indicates ROHS status"
                            value={customField.description || ""}
                            name="description"
                            onChange={(e, control) => handleChange(e, control, 'customFields', customField)}
                          />
                        }
                      />
                    </Form.Field>
                  </Table.Cell>
                  <Table.Cell>
                    <Form.Field>
                      <label>{t('page.settings.fieldType', "Field Type")}</label>
                      <Popup
                        wide
                        hoverable
                        content={
                          <p>{t('page.settings.popup.fieldType', "Choose the type of data you would like to add the custom field to.")}</p>
                        }
                        trigger={
                          <Dropdown
                            name="customFieldTypeId"
                            selection
                            value={customField.customFieldTypeId || 0}
                            options={customFieldTypeOptions}
                            onChange={(e, control) => handleChange(e, control, 'customFields', customField)}
                          />
                        }
                      />
                    </Form.Field>
                  </Table.Cell>
                  <Table.Cell textAlign="center"><Button type="button" icon="delete" size='tiny' onClick={(e) => openDeleteCustomField(e, customField)} /></Table.Cell>
                </Table.Row>))
            : <Table.Row><Table.Cell colSpan={4} textAlign="center">{t('page.settings.noCustomFieldsDefined', "No custom fields defined.")}</Table.Cell></Table.Row>
          }
        </Table.Body>
      </Table>
    </Segment>);
  }, [customFieldSettings, loading]);

  const kiCadSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
      <Header dividing as="h3">
        {t('page.settings.kicadName', "KiCad")}
      </Header>
      <p>
        <i>
          {t('page.settings.kicadDescription', "Customize KiCad Integration")}
        </i>
      </p>

      <Form.Field width={10}>
        <label>{t('page.settings.kicadSupport', "KiCad HTTP Library Server Support")}</label>
        <Popup
          wide
          position="top left"
          offset={[150, 0]}
          hoverable
          content={
            <p>{t('page.settings.popup.kicadSupportEnabled', "Choose if you would like to enable KiCad HTTP Library Server support.")}</p>
          }
          trigger={
            <Dropdown
              name="kiCadEnabled"
              type="bool-dropdown"
              placeholder="Disabled"
              selection
              value={kiCadSettings.kiCad.enabled ? 1 : 0}
              className={!kiCadSettings.kiCad.enabled ? "bool-disabled" : ""}
              options={enabledSources}
              onChange={(e, control) => handleChange(e, control, 'kiCad')}
            />
          }
        />
      </Form.Field>

      <Header dividing as="h4">
        {t('page.settings.kicad.exportFields', "Export Fields")}
      </Header>
      <p>
        <i>
          {t('page.settings.kicad.exportFieldsDescription', "Choose the Inventory fields to export to KiCad. KiCad field names can be customized.")}
        </i>
      </p>
      <Table compact celled sortable selectable striped size='small'>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell width={1}>
              <Popup
                wide
                content={<p>{t('page.settings.kicad.exportQuestionHelp', "Only selected fields will be exported to KiCad.")}</p>}
                trigger={<span>{t('label.exportQuestion', "Export?")}</span>}
              />
            </Table.HeaderCell>
            <Table.HeaderCell width={7}>
              <Popup 
                wide 
                content={<p>{t('page.settings.kicad.binnerInventoryFieldHelp', "Choose the Binner Inventory field to export to KiCad.")}</p>} 
                trigger={<span>{t('page.settings.kicad.binnerInventoryField', "Binner Inventory Field")}</span>}
              />
            </Table.HeaderCell>
            <Table.HeaderCell width={7}>
              <Popup 
                wide 
                content={<p>{t('page.settings.kicad.kicadFieldHelp', "Choose the KiCad to export the field as. You can choose the preset option or add a customized value.")}</p>} 
                trigger={<span>{t('page.settings.kicad.kicadField', "KiCad Field Name")}</span>}
              />
            </Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {kiCadSettings.kiCad.exportFields.map((field, key) => (
          <Table.Row key={key}>
            <Table.Cell><Checkbox name="enabled" checked={field.enabled} onChange={(e, control) => handleKiCadFieldSettingsChange(e, control, field)} /></Table.Cell>
            <Table.Cell><Dropdown search selection name="field" value={field.field} options={exportFieldOptions} onChange={(e, control) => handleKiCadFieldSettingsChange(e, control, field)} /></Table.Cell>
            <Table.Cell><Dropdown search selection allowAdditions name="kiCadFieldName" value={field.kiCadFieldName} options={kiCadExportFieldOptions} onChange={(e, control) => handleKiCadFieldSettingsChange(e, control, field)} onAddItem={handleAddKiCadExportField} /></Table.Cell>
          </Table.Row>
          ))}
        </Table.Body>
      </Table>
    </Segment>);
  }, [kiCadSettings, loading]);

  const printerSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
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
              value={printerSettings.printer.printMode}
              options={printModes}
              onChange={(e, control) => handleChange(e, control, 'printer')}
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
              value={printerSettings.printer.printerName || ""}
              name="printerPrinterName"
              onChange={(e, control) => handleChange(e, control, 'printer')}
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
              value={printerSettings.printer.partLabelSource}
              options={labelSources}
              onChange={(e, control) => handleChange(e, control, 'printer')}
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
              value={printerSettings.printer.partLabelName || ""}
              name="printerPartLabelName"
              onChange={(e, control) => handleChange(e, control, 'printer')}
            />
          </Form.Field>
        }
      />

    </Segment>);
  }, [printerSettings, loading]);

  const barcodeSettingsMemoized = useMemo(() => {
    return (<Segment loading={loading} color="blue" raised padded>
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
              value={barcodeSettings.barcode.enabled ? 1 : 0}
              options={enabledSources}
              onChange={(e, control) => handleChange(e, control, 'barcode')}
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
              type="bool-dropdown"
              placeholder="Disabled"
              selection
              value={barcodeSettings.barcode.isDebug ? 1 : 0}
              options={enabledSources}
              onChange={(e, control) => handleChange(e, control, 'barcode')}
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
            <div>
            <ClearableInput
              icon="clock"
              className="labeled"
              placeholder=""
              value={barcodeSettings.barcode.bufferTime || ""}
              name="barcodeBufferTime"
              onChange={(e, control) => handleChange(e, control, 'barcode')}
            />
            </div>
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
            <div>
            <ClearableInput
              icon="clock"
              className="labeled"
              placeholder=""
              value={barcodeSettings.barcode.maxKeystrokeThresholdMs || ""}
              name="barcodeMaxKeystrokeThresholdMs"
              onChange={(e, control) => handleChange(e, control, 'barcode')}
            />
            </div>
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
            <div>
            <ClearableInput
              className="labeled"
              placeholder=""
              value={barcodeSettings.barcode.prefix2D || ""}
              name="barcodePrefix2D"
              onChange={(e, control) => handleChange(e, control, 'barcode')}
            />
            </div>
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
              value={barcodeSettings.barcode.profile}
              options={barcodeProfileOptions}
              onChange={(e, control) => handleChange(e, control, 'barcode')}
            />
          }
        />
      </Form.Field>

    </Segment>);
  }, [barcodeSettings, loading]);

  const tabPanes = [
    {
      menuItem: { key: 'mysettings', icon: 'user', content: t('page.settings.mySettings', "My Settings") },
      render: () => 
        <TabPane style={{ padding: '20px' }}>
          <Trans i18nKey="page.settings.mySettingsDescription">
            Configure settings associated with your user account.
          </Trans>
          <hr />
          {userSettingsMemoized}
          {printerSettingsMemoized}
          {barcodeSettingsMemoized}
      </TabPane>
    },
    isAdmin() &&
      { 
        menuItem: { key: 'orgSettings', icon: 'building', content: t('page.settings.orgSettings', "Organization Settings") },
        render: () => 
          <TabPane style={{ padding: '20px' }}>
            <Trans i18nKey="page.settings.orgSettingsDescription">
              Configure settings common to all users in your organization.
            </Trans>
            <hr />
            {organizationSettingsMemoized}
            {integrationSettingsMemoized}
            {customFieldSettingsMemoized}
            {kiCadSettingsMemoized}
          </TabPane> 
        },
  ];

  const handleTabChange = (e) => {
    setCurrentTab(e.target.value);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.settings.title', "Settings")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.settings.title', "Settings")} to="/">
        <Trans i18nKey="page.settings.description">
          Additional help on configuring Binner can be found on the <a href="https://github.com/replaysMike/Binner/wiki/Configuration" target="_blank" rel="noreferrer">Wiki</a>
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
      <Confirm
        className="confirm"
        header={t('confirm.header.deleteCustomField', "Delete Custom Field")}
        open={confirmDeleteCustomFieldIsOpen}
        onCancel={closeDeleteCustomField}
        onConfirm={handleDeleteCustomField}
        content={confirmDeleteCustomFieldContent}
      />
      <Form onSubmit={onSubmit}>
        <Tab panes={tabPanes} activeIndex={currentTab} onTabChange={handleTabChange} />

        <Form.Field inline>
          <Button type="submit" primary style={{ marginTop: "10px" }} disabled={!isDirty}>
            <Icon name="save" />
            {t('button.save', "Save")}
          </Button>
          {saveMessage.length > 0 && <Label pointing="left">{saveMessage}</Label>}
        </Form.Field>

        <div className="sticky-target" style={{ padding: '10px 10px 20px 10%' }} data-bounds={currentTab === 0 ? "0,0.4" : "0,0.82"}>
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
        
      </Form>
    </div>
  );
};
