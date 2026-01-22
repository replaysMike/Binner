import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { SubscriptionLevels } from "../../common/Types";
import { SubscriptionFeatureLink } from "../../components/SubscriptionFeatureLink";

export const Manage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.manage', "Manage")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.manage.title', "Manage")} to="/">
        <Trans i18nKey="page.manage.description">
          Tools for managing your data.
        </Trans>
      </FormHeader>

      <Segment raised>
        <h3>{t('page.manage.header', "Choose a management task")}</h3>

        <div className="helpcontainer">
          <ul>
            <SubscriptionFeatureLink 
              requiredSubscriptionLevel={SubscriptionLevels.Maker}
              title="Bulk Edit Parts"
              description="Edit many parts at once."
              url="/manage/bulkedit/parts"
             />
          </ul>
        </div>
      </Segment>
    </div>
  );
};
