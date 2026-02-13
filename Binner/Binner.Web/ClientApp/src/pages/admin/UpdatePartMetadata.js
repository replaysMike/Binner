import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Segment, Breadcrumb } from "semantic-ui-react";
import { FormHeader } from "../../components/FormHeader";
import { BulkUpdatePartMetadataComponent } from "@binner/binner-licensed";

export function UpdatePartMetadata() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  return (<div>
    <Breadcrumb>
      <Breadcrumb.Section link onClick={() => navigate("/")}>{t("bc.home", "Home")}</Breadcrumb.Section>
      <Breadcrumb.Divider />
      <Breadcrumb.Section link onClick={() => navigate("/manage")}>{t("bc.manage", "Manage")}</Breadcrumb.Section>
      <Breadcrumb.Divider />
      <Breadcrumb.Section active>{t("bc.updatePartMetadata", "Update Part Metadata")}</Breadcrumb.Section>
    </Breadcrumb>
    <FormHeader name={t("page.manage.updatePartMetadata.title", "Update Part Metadata")} to={".."}>
      {t("page.manage.updatePartMetadata.description", "Update the metadata associated with parts in your inventory.")}
    </FormHeader>

    <Segment raised>
      <BulkUpdatePartMetadataComponent />
    </Segment>
  </div>);
}