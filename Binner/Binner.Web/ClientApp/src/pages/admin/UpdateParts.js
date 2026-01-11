import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb, Icon } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { getSubscriptionLevel, handleSubscriptionLink } from "../../common/authentication";
import { SubscriptionLevels } from "../../common/Types";

export const UpdateParts = () => {
  const { t } = useTranslation();
	const navigate = useNavigate();
  const subscriptionLevel = getSubscriptionLevel();
  return (
    <div>
      <Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.updatePartMetadata', "Update Part Metadata")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.updateParts.title", "Update Part Metadata")} to="..">
        <Trans i18nKey="page.admin.updateParts.description">
        Refresh information from external APIs and choose which fields you would like to update.
        </Trans>
      </FormHeader>

      <Segment raised>
      </Segment>
    </div>
  );
};
