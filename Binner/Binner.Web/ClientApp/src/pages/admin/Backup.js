import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb, Icon } from "semantic-ui-react";

export const Backup = () => {
  const { t } = useTranslation();
	const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.backupRestore', "Backup / Restore")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>Backup / Restore</h1>

      <p>Create a backup or restore from a backup.</p>

      <Segment raised>
      </Segment>
    </div>
  );
};
