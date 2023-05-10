import React from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";

export function BulkPrint(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();

	return (
		<div>
			<Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/printing")}>{t('bc.printing', "Printing")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.bulkPrint', "Bulk Print")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.printing.bulkPrint.title', "Bulk Print")} to="/">
        <Trans i18nKey="page.printing.bulkPrint.description">
        Print labels for multiple parts at once.
        </Trans>
			</FormHeader>
		</div>
	);
};