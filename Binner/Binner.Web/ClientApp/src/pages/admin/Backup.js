import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Button, Form, Divider, Grid, Segment, Breadcrumb } from "semantic-ui-react";
import { toast } from "react-toastify";
import axios from "axios";
import { useDropzone } from "react-dropzone";
import { fetchApi } from "../../common/fetchApi";
import { humanFileSize } from "../../common/files";
import { FormHeader } from "../../components/FormHeader";
import { getAuthToken } from "../../common/authentication";

export const Backup = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [exportFormat, setExportFormat] = useState("");
  const [isDirty, setIsDirty] = useState(false);
  const [error, setError] = useState(null);
  const [file, setFile] = useState(null);
  const [importResult, setImportResult] = useState(null);
  const [filesToUpload, setFilesToUpload] = useState([]);
  const [disableCreateBackupButton, setDisableCreateBackupButton] = useState(false);
  const { acceptedFiles, getRootProps, getInputProps } = useDropzone({
    maxFiles: 3,
    onDrop: (acceptedFiles, rejectedFiles, e) => {
      const acceptedMimeTypes = ["application/x-zip-compressed", "application/octet-stream"];
      let errorMsg = "";
      const newFilesToUpload = [];
      for (let i = 0; i < acceptedFiles.length; i++) {
        if (!acceptedFiles[i].type) {
          // check file extension
          const parts = acceptedFiles[i].name.split(".");
          const extension = parts[parts.length - 1];
          if (extension !== "bak") {
            errorMsg += `File '${acceptedFiles[i].name}' is not a Binner backup file!\r\n`;
          } else {
            newFilesToUpload.push(acceptedFiles[i]);
          }
        } else if (!acceptedMimeTypes.includes(acceptedFiles[i].type)) {
          errorMsg += `File '${acceptedFiles[i].name}' with mime type '${acceptedFiles[i].type}' is not a Binner backup file!\r\n`;
        } else {
          newFilesToUpload.push(acceptedFiles[i]);
        }
      }

      if (errorMsg.length > 0) {
        setError(errorMsg);
        setFilesToUpload([]);
        setFile(null);
        setIsDirty(false);
      } else {
        setFile(URL.createObjectURL(acceptedFiles[0]));
        setFilesToUpload([...newFilesToUpload]);
        setIsDirty(true);
        setError(null);
      }
    }
  });
  const acceptedFileItems = filesToUpload.map((file) => (
    <li key={file.path} className="small">
      {file.path} - {humanFileSize(file.size)}
    </li>
  ));

  const onBackupSubmit = async (e) => {
    setLoading(true);
    setDisableCreateBackupButton(true);

    // first fetch some data using fetchApi, to leverage 401 token refresh
    await fetchApi("api/authentication/identity").then((_) => {
      axios
        .request({
          method: "post",
          url: `api/system/backup`,
          headers: { Authorization: `Bearer ${getAuthToken()}` },
          responseType: "blob"
        })
        .then((blob) => {
          // specifying blob filename, must create an anchor tag and use it as suggested: https://stackoverflow.com/questions/19327749/javascript-blob-filename-without-link
          var file = window.URL.createObjectURL(blob.data);
          var a = document.createElement("a");
          document.body.appendChild(a);
          a.style = "display: none";
          a.href = file;
          const today = new Date();
          a.download = `binner-backup-${today.getFullYear()}-${today.getMonth() + 1}-${today.getDate()}.bak`;
          a.click();
          window.URL.revokeObjectURL(file);
          toast.success(`Binner backed up successfully!`);
          setDisableCreateBackupButton(false);
          setLoading(false);
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(`Binner backup generation failed!`);
          setDisableCreateBackupButton(false);
          setLoading(false);
        });
    });
  };

  const onImportSubmit = async (e) => {
    setLoading(true);
    if (acceptedFiles && acceptedFiles.length > 0) {
      const formData = new FormData();
      formData.append("file", acceptedFiles[0], acceptedFiles[0].name);

      // first fetch some data using fetchApi, to leverage 401 token refresh
      await fetchApi("api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: "api/system/restore",
            data: formData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((data) => {
            toast.info(`Binner restored successfully!`);
            setImportResult(data.data);
            setIsDirty(false);
          })
          .catch((error) => {
            toast.dismiss();
            console.error("error", error);
            toast.error(`Binner restore failed!\n${error.response.data.message}`, { autoClose: 10000 });
            setIsDirty(false);
          });
      });
    } else {
      toast.error("No files selected for upload!");
    }
    setLoading(false);
  };

  const handleChange = (e, control) => {
    setExportFormat(control.value);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t("bc.home", "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin")}>{t("bc.admin", "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.backupRestore", "Backup / Restore")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.backup.title", "Backup / Restore")} to={".."}>
        {t("page.backup.description", "Create a backup or restore from a backup.")}
      </FormHeader>

      <Segment loading={loading}>
        <Grid columns={2}>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onImportSubmit}>
              <div
                style={{ border: "1px dashed #000", padding: "50px", marginBottom: "20px", backgroundColor: "#f5f5f5" }}
                {...getRootProps({ className: "dropzone" })}
              >
                <span style={{ fontSize: "0.6em" }}>{t("page.exportData.uploadNote", "Drag a document to upload, or click to select files")}</span>
                <input {...getInputProps()} />
                <div style={{ fontSize: "0.6em" }}>{t("page.backup.acceptedFileTypes", 'Accepted file types: "*.bak"')}</div>
              </div>
              {error && (
                <div className="error small">
                  <span style={{ color: "#cc0000" }}>
                    <b>{t("label.error", "Error")}:</b>
                  </span>{" "}
                  {error}
                </div>
              )}
              <aside>
                <ol>{acceptedFileItems}</ol>
              </aside>
              <Button primary disabled={!isDirty}>
                {t("button.restore", "Restore")}
              </Button>
            </Form>
          </Grid.Column>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onBackupSubmit}>
              <div style={{ padding: "50px", marginBottom: "20px", height: "140px" }}>
                <p>{t("page.backup.backupDescription", "Create a snapshot of your Binner installation.")}</p>
              </div>
              <Button primary disabled={disableCreateBackupButton}>
                {t("button.createBackup", "Create Backup")}
              </Button>
            </Form>
          </Grid.Column>
        </Grid>

        <Divider vertical>{t("label.or", "Or")}</Divider>
      </Segment>
    </div>
  );
};
