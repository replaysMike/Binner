import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb, Icon } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";

export const ActivateLicense = () => {
  const { t } = useTranslation();
	const navigate = useNavigate();
  return (
    <div>
      <Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.activateLicense', "Activate License")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.activateLicense.title", "Activate License")} to="..">
        <Trans i18nKey="page.admin.activateLicense.description">
        Enter your license key to enable pro features on your local Binner installation. Don't have a license key? Visit <a href="https://binner.io" target="_blank" rel="noreferrer">Binner Cloud</a>
        </Trans>
      </FormHeader>

      <Segment raised>
        
      </Segment>
    </div>
  );
};
