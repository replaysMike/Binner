import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { SubscriptionLevels } from "../../common/Types";
import { SubscriptionFeatureLink } from "../../components/SubscriptionFeatureLink";

export const Printing = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.printing', "Printing")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.printing.title', "Printing")} to="/">
        <Trans i18nKey="page.printing.description">
        Printing of labels and label templates.
        </Trans>
      </FormHeader>

      <Segment raised>
        <h3>{t('page.printing.header', "Choose a printing task")}</h3>

        <div className="helpcontainer">
          <ul>
						<li onClick={() => navigate("/printing/labelTemplates")}>
              <h1><span>{t('page.printing.labelTemplates.title', "Label Templates")}</span></h1>
              <p>{t('page.printing.labelTemplates.baseDescription', "Create and edit label templates.")}</p>
            </li>
            <li onClick={() => navigate("/printing/printLabels")}>
            <h1><span>{t('page.printing.printLabels.title', "Print Custom Labels")}</span></h1>
              <p>{t('page.printing.printLabels.baseDescription', "Print custom labels for any task.")}</p>
            </li>
            <SubscriptionFeatureLink 
              requiredSubscriptionLevel={SubscriptionLevels.Maker}
              title={t('page.printing.bulkprint.title', "Bulk Print")}
              description={t('page.printing.bulkprint.baseDescription', "Batch print labels for parts.")}
              url="/printing/bulkPrint"
            />
          </ul>
        </div>
      </Segment>
    </div>
  );
};
