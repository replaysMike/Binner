import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Segment, Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { SubscriptionLevels } from "../../common/Types";
import { SubscriptionFeatureLink } from "../../components/SubscriptionFeatureLink";

export const Admin = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.admin', "Admin")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.admin.title', "Admin")} to="/">
        <Trans i18nKey="page.admin.description">
          System administration for managing your installation.
        </Trans>
      </FormHeader>

      <Segment raised>
        <h3>{t('page.admin.header', "Choose an administration task")}</h3>

        <div className="helpcontainer">
          <ul>
            <li onClick={() => navigate("/admin/users")}>
              <h1><span>{t('page.admin.users.title', "User Management")}</span></h1>
              <p>{t('page.admin.users.baseDescription', "Manage the users of your system.")}</p>
            </li>
            <li onClick={() => navigate("/admin/backup")}>
              <h1><span>{t('page.admin.backupRestore.title', "Backup/Restore")}</span></h1>
              <p>{t('page.admin.backupRestore.baseDescription', "Backup or restore your Binner installation.")}</p>
            </li>
            <li onClick={() => navigate("/admin/info")}>
              <h1><span>{t('page.admin.systemInfo.title', "System Information")}</span></h1>
              <p>{t('page.admin.systemInfo.baseDescription', "Get information on your Binner installation.")}</p>
            </li>
            <li onClick={() => navigate("/admin/logs")}>
              <h1><span>{t('page.admin.systemLogs.title', "System Logs")}</span></h1>
              <p>{t('page.admin.systemLogs.baseDescription', "Get the system logs.")}</p>
            </li>
            <SubscriptionFeatureLink 
              requiredSubscriptionLevel={SubscriptionLevels.Maker}
              title="Update Part Metadata"
              description="Refresh information from external APIs and choose which fields you would like to update."
              url="/admin/updatemetadata"
              />
          </ul>
        </div>
      </Segment>
    </div>
  );
};
