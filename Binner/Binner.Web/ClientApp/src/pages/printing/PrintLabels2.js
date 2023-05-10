import React from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { LabelEditor } from "./labelEditor/index.js";

export function PrintLabels2(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();

	return (
		<div>
			<Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/printing")}>{t('bc.printing', "Printing")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.labelTemplates', "Label Templates")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.printing.labelTemplates.title', "Label Templates")} to="/">
        <Trans i18nKey="page.printing.labelTemplates.description">
        Edit part label and custom label templates.
        </Trans>
			</FormHeader>
			<LabelEditor />
		</div>
	);
};