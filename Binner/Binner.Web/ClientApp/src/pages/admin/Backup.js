import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Button, Form, Divider, Grid, Segment, Breadcrumb } from "semantic-ui-react";
import { toast } from "react-toastify";
import axios from "axios";
import { useDropzone } from "react-dropzone";
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
  const { acceptedFiles, fileRejections, isDragAccept, isDragReject, getRootProps, getInputProps } = useDropzone({
    maxFiles: 3,
    accept: "application/octet-stream",
    onDrop: (acceptedFiles, rejectedFiles, e) => {
      if (acceptedFiles.length > 0) {
        setFile(URL.createObjectURL(acceptedFiles[0]));
        setIsDirty(true);
        setError(null);
      }
      if (rejectedFiles.length > 0) {
        let errorMsg = "";
        for (let i = 0; i < rejectedFiles.length; i++)
          errorMsg += `File '${rejectedFiles[i].file.name}' with mime type '${rejectedFiles[i].file.type}' invalid!\r\n`;
        setError(errorMsg);
      }
    }
  });
  const acceptedFileItems = acceptedFiles.map((file) => (
    <li key={file.path} className="small">
      {file.path} - {humanFileSize(file.size)}
    </li>
  ));

  const onBackupSubmit = async (e) => {
    setLoading(true);

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
        a.download = `binner-export-${today.getFullYear()}-${today.getMonth() + 1}-${today.getDate()}.bak`;
        a.click();
        window.URL.revokeObjectURL(file);
        toast.info(`Binner backed up successfully!`);
      })
      .catch((error) => {
        toast.dismiss();
        console.error("error", error);
        toast.error(`Binner backup generation failed!`);
      });

    setLoading(false);
  };

  const onImportSubmit = async (e) => {
    setLoading(true);
    if (acceptedFiles && acceptedFiles.length > 0) {
      const formData = new FormData();
      formData.append("file", acceptedFiles[0], acceptedFiles[0].name);

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
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(`Binner restore failed!\n${error.response.data.message}`, { autoClose: 10000 });
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
				<Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.backupRestore', "Backup / Restore")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.backup.title', "Backup / Restore")} to={".."}>
        {t('page.backup.description', "Create a backup or restore from a backup.")}
			</FormHeader>

      <Segment loading={loading}>
        <Grid columns={2}>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onImportSubmit}>
              <div
                style={{ border: "1px dashed #000", padding: "50px", marginBottom: "20px", backgroundColor: "#f5f5f5" }}
                {...getRootProps({ className: "dropzone" })}
              >
                <span style={{ fontSize: "0.6em" }}>{t('page.exportData.uploadNote', "Drag a document to upload, or click to select files")}</span>
                <input {...getInputProps()} />
                <div style={{ fontSize: "0.6em" }}>{t('page.backup.acceptedFileTypes', "Accepted file types: \"*.bak\"")}</div>
              </div>
              {error && (
                <div className="error small">
                  <b>{t('label.error', "Error")}:</b> {error}
                </div>
              )}
              <aside>
                <ol>{acceptedFileItems}</ol>
              </aside>
              <Button primary>{t('button.restore', "Restore")}</Button>
            </Form>
          </Grid.Column>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onBackupSubmit}>
              <div style={{ padding: "50px", marginBottom: "20px", height: "140px" }}>
                <p>{t('page.backup.backupDescription', "Create a snapshot of your Binner installation.")}</p>
              </div>
              <Button primary>{t('button.createBackup', "Create Backup")}</Button>
            </Form>
          </Grid.Column>
        </Grid>

        <Divider vertical>{t('label.or', "Or")}</Divider>
      </Segment>
    </div>
  );
};
