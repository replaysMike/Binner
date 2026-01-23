import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Segment, Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { BulkEditPartsComponent } from "@binner/binner-licensed";

export function BulkEditParts() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (<div>
    <Breadcrumb>
      <Breadcrumb.Section link onClick={() => navigate("/")}>{t("bc.home", "Home")}</Breadcrumb.Section>
      <Breadcrumb.Divider />
      <Breadcrumb.Section link onClick={() => navigate("/manage")}>{t("bc.manage", "Manage")}</Breadcrumb.Section>
      <Breadcrumb.Divider />
      <Breadcrumb.Section active>{t("bc.bulkEditParts", "Bulk Edit Parts")}</Breadcrumb.Section>
    </Breadcrumb>
    <FormHeader name={t("page.manage.bulkEditParts.title", "Bulk Edit Parts")} to={".."}>
      {t("page.manage.bulkEditParts.description", "Edit multiple parts at once.")}
    </FormHeader>

    <Segment raised>
      <BulkEditPartsComponent />
    </Segment>
  </div>);
}