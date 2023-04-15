import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb, Icon } from "semantic-ui-react";
import "./help.css";

export const Help = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.help', "Help")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>Help</h1>

      <p>Need assistance? Try using our help topics below to solve your issue.</p>

      <Segment raised>
        <h3>Choose a Help topic</h3>

        <div className="helpcontainer">
          <ul>
						<li onClick={() => navigate("/help/scanning")}>
              <h1>Barcode Scanning</h1>
              <p>Learn more about what types of features are available using a handheld barcode scanner.</p>
            </li>
            <li onClick={() => navigate("/help/api-integrations")}>
              <h1>Api Integrations</h1>
              <p>Configuring Api integrations are an important part of using Binner effectively.</p>
            </li>
            <li onClick={() => navigate("/help/bom")}>
              <h1>BOM</h1>
              <p>Learn how to configure BOM (Bill of Materials) projects to aid in managing your inventory usage.</p>
            </li>
            <li onClick={() => window.open("https://github.com/replaysMike/Binner/wiki", "_blank")}>
              <h1>Wiki</h1>
              <p>Get more help from the wiki on GitHub</p>
            </li>
						<li onClick={() => window.open("https://github.com/replaysMike/Binner/issues", "_blank")}>
							<h1>Report a Bug</h1>
							<Icon name="bug" color="blue" size="large" />
              <p>Help the community build a great free product by reporting bugs online.</p>
            </li>
          </ul>
        </div>
      </Segment>
    </div>
  );
};
