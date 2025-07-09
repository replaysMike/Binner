import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Header, Form, Segment, Breadcrumb } from "semantic-ui-react";
import { Clipboard } from "../../components/Clipboard";
import { fetchApi } from "../../common/fetchApi";
import { FormHeader } from "../../components/FormHeader";
import { getCurrencySymbol } from "../../common/Utils";
import { getUserAccount } from "../../common/authentication";
import { getAccountTypesLabel } from "../../common/Types";

export const SystemInformation = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [info, setInfo] = useState({
    version: ''
  });

  useEffect(() => {
    fetchSystemInfo();
    function fetchSystemInfo() {
      setLoading(true);
      fetchApi("/api/system/info").then((response) => {
        const { data } = response;
        if (data) {
          setInfo(data);
        }
        setLoading(false);
      });
    }
  }, []);

  const currentUser = getUserAccount();

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t("bc.home", "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin")}>{t("bc.admin", "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.systemInfo", "System Information")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.systemInfo.title", "System Information")} to={".."}>
        {t("page.admin.systemInfo.description", "Information about your Binner installation.")}
      </FormHeader>

      <Segment loading={loading} raised>
        <Form>
          <Form.Group>
            <Form.Field width={3}>
              <label>{t("page.admin.systemInfo.version", "Version")}</label>
              <div>{info.version}</div>
            </Form.Field>
            <Form.Field width={3}>
              <label>{t("page.admin.systemInfo.lastestVersion", "Latest Binner Version")}</label>
              <div>{info.latestVersion}</div>
            </Form.Field>
            <Form.Field>
              <label>{t("page.admin.systemInfo.license", "License")}</label>
              <div>{info.license || "Free"}</div>
            </Form.Field>
            <Form.Field>
              <label>{t("page.admin.systemInfo.currentUser", "Current User")}</label>
              <div>{currentUser.name} ({getAccountTypesLabel(currentUser)})</div>
            </Form.Field>
          </Form.Group>

          <Segment color="red" style={{marginTop: '50px'}}>
            <Header>{t("page.admin.systemInfo.api", "Api")}</Header>
            <Form.Group>
              <Form.Field width={6}>
                <label>{t("page.admin.systemInfo.enabledIntegrations", "Enabled Integrations")}</label>
                <div>{info.enabledIntegrations || ""}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.language", "Language")}</label>
                <div>{info.language || "En"}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.currency", "Currency")}</label>
                <div><span>{getCurrencySymbol(info.currency || "USD")}</span> {info.currency || "USD"}</div>
              </Form.Field>
            </Form.Group>
          </Segment>

          <Segment color="blue" style={{marginTop: '50px'}}>
            <Header>{t("page.admin.systemInfo.server", "Server")}</Header>
            <Form.Group>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.storageProvider", "Storage Provider")}</label>
                <div>{info.storageProvider || "Binner"}</div>
              </Form.Field>
              <Form.Field width={13}>
                <label>{t("page.admin.systemInfo.storageProviderConfig", "Storage Provider Config")}</label>
                <div className="ellipsis-container nowrap resizable">
                  <Clipboard text={info.storageProviderSettings} /><div title={info.storageProviderSettings}>{info.storageProviderSettings || ""}</div>
                </div>
              </Form.Field>
            </Form.Group>

            <Form.Group>
              <Form.Field width={6}>
                <label>{t("page.admin.systemInfo.installationPath", "Installation Path")}</label>
                <div className="ellipsis-container nowrap resizable">
                  <Clipboard text={info.installPath} /><div title={info.installPath}>{info.installPath || ""}</div>
                </div>
              </Form.Field>
              <Form.Field width={5}>
                <label>{t("page.admin.systemInfo.userFilesUploadPath", "User Files Upload Path")}</label>
                <div className="ellipsis-container nowrap resizable">
                <Clipboard text={info.userFilesLocation} /><div title={info.userFilesLocation}>{info.userFilesLocation || ""}</div>
                </div>
              </Form.Field>
              <Form.Field width={5}>
                <label>{t("page.admin.systemInfo.logPath", "Log Path")}</label>
                <div className="ellipsis-container nowrap resizable">
                <Clipboard text={info.logPath} /><div title={info.logPath}>{info.logPath || ""}</div>
                </div>
              </Form.Field>
            </Form.Group>

            <Form.Group>
            <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.ip", "IP")}</label>
                <div>{info.ip || "localhost"}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.port", "Port")}</label>
                <div>{info.port || ""}</div>
              </Form.Field>
            </Form.Group>
          </Segment>

          <Segment color="green" style={{marginTop: '50px'}}>
            <Header>{t("page.admin.systemInfo.data", "Data")}</Header>
            <Form.Group>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.totalParts", "Total Parts")}</label>
                <div>{info.totalParts || ""}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.totalUsers", "Total Users")}</label>
                <div>{info.totalUsers || ""}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.totalPartTypes", "Total Part Types")}</label>
                <div>{info.totalPartTypes || ""}</div>
              </Form.Field>
              <Form.Field width={3}>
                <label>{t("page.admin.systemInfo.totalUserFiles", "Total User Files")}</label>
                <div>{info.totalUserFiles || ""}</div>
              </Form.Field>
              <Form.Field width={4}>
                <label>{t("page.admin.systemInfo.userFilesSize", "User Files Size")}</label>
                <div>{info.userFilesSize || ""}</div>
              </Form.Field>
            </Form.Group>
          </Segment>
        </Form>

        
      </Segment>
    </div>
  );
};
