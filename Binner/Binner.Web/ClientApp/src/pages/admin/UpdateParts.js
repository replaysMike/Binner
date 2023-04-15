import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb, Icon } from "semantic-ui-react";

export const UpdateParts = () => {
  const { t } = useTranslation();
	const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.updatePartMetadata', "Update Part Metadata")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>Update Part Metadata</h1>

      <p>Refresh information from external APIs and choose which fields you would like to update.</p>

      <Segment raised>
      </Segment>
    </div>
  );
};
