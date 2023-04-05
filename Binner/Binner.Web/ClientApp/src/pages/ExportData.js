import React, { useState } from "react";
import { useTranslation, Trans } from "react-i18next";
import { Button, Form, Divider, Grid, Segment } from "semantic-ui-react";
import { toast } from "react-toastify";
import axios from "axios";
import { useDropzone } from "react-dropzone";
import { getAuthToken } from "../common/authentication";

export const ExportData = (props) => {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [exportFormat, setExportFormat] = useState("");
  const [isDirty, setIsDirty] = useState(false);
  const [error, setError] = useState(null);
  const [file, setFile] = useState(null);
  const [importResult, setImportResult] = useState(null);
  const [exportFormats] = useState([
    {
      key: 1,
      value: "Excel",
      text: "Excel"
    },
    {
      key: 2,
      value: "CSV",
      text: "CSV"
    },
    {
      key: 3,
      value: "SQL",
      text: "SQL"
    }
  ]);
  const { acceptedFiles, fileRejections, isDragAccept, isDragReject, getRootProps, getInputProps } = useDropzone({
    maxFiles: 3,
    accept: "application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/sql,text/csv",
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
    <li key={file.path}>
      {file.path} - {file.size} bytes
    </li>
  ));

  const onExportSubmit = async (e) => {
    setLoading(true);

    axios
      .request({
        method: "get",
        url: `data/export?exportFormat=${exportFormat}`,
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
        a.download = `binner-export-${today.getFullYear()}-${today.getMonth() + 1}-${today.getDate()}.zip`;
        a.click();
        window.URL.revokeObjectURL(file);
        toast.info(`Data exported successfully!`);
      })
      .catch((error) => {
        toast.dismiss();
        console.error("error", error);
        toast.error(`Export data failed!`);
      });

    setLoading(false);
  };

  const onImportSubmit = async (e) => {
    setLoading(true);
    if (acceptedFiles && acceptedFiles.length > 0) {
      const formData = new FormData();
      for (let i = 0; i < acceptedFiles.length; i++) {
        formData.append("files", acceptedFiles[i], acceptedFiles[i].name);
      }

      axios
        .request({
          method: "post",
          url: "data/import",
          data: formData,
          headers: { Authorization: `Bearer ${getAuthToken()}` }
        })
        .then((data) => {
          console.log("data", data);
          toast.info(`Data imported successfully!`);
          setImportResult(data.data);
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(`Import upload failed!`);
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
      <h1>{t('page.exportData.title', "Import/Export Data")}</h1>
      <p>{t('page.exportData.description', "Import or Export your Binner database to a human-readable format.")}</p>
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
                <div style={{ fontSize: "0.6em" }}>{t('page.exportData.acceptedFileTypes', "Accepted file types: \"*.sql, *.xls, *.xlsx, *.csv\"")}</div>
              </div>
              {error && (
                <div className="error small">
                  <b>{t('label.error', "Error")}:</b> {error}
                </div>
              )}
              <aside>
                <ol>{acceptedFileItems}</ol>
              </aside>
              <Button primary>{t('button.import', "Import")}</Button>
            </Form>
          </Grid.Column>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onExportSubmit}>
              <div style={{ padding: "50px", marginBottom: "20px", height: "140px" }}>
                <Form.Dropdown
                  label="Format"
                  placeholder="Choose a format"
                  selection
                  value={exportFormat}
                  options={exportFormats}
                  onChange={handleChange}
                  name="exportFormat"
                  style={{ maxWidth: "50%" }}
                />
              </div>
              <Button primary>{t('button.export', "Export")}</Button>
            </Form>
          </Grid.Column>
        </Grid>

        <Divider vertical>{t('label.or', "Or")}</Divider>
      </Segment>

      {importResult && (
        <div style={{ border: "1px dashed #666", padding: "10px" }}>
          <h5>{t('page.exportData.importResult', "Import Result")}</h5>
          <div>
            {t('label.status', "Status")}: <b>{importResult.success ? t('label.success', "Success") : t('label.failed', "Failed")}</b>
          </div>
          <div>{t('page.exportData.totalRowsImported', "Total Rows Imported")}: {importResult.totalRowsImported}</div>
          <br />
          <div>{t('page.exportData.totalProjectsImported', "Projects Imported")}: {importResult.rowsImportedByTable.Projects}</div>
          <div>{t('page.exportData.totalPartTypesImported', "Part Types Imported")}: {importResult.rowsImportedByTable.PartTypes}</div>
          <div>{t('page.exportData.totalPartsImported', "Parts Imported")}: {importResult.rowsImportedByTable.Parts}</div>

          {importResult.errors && importResult.errors.length > 0 && (
            <div style={{ marginTop: "20px" }}>
              <h6>{t('label.errors', "Errors")}:</h6>
              <ul>
                {importResult.errors.map((msg, k) => {
                  <li key={k}>{msg}</li>;
                })}
              </ul>
            </div>
          )}

          {importResult.warnings && importResult.warnings.length > 0 && (
            <div style={{ marginTop: "20px" }}>
              <h6>{t('label.warnings', "Warnings")}:</h6>
              <ul>
                {importResult.warnings.map((msg, k) => (
                  <li key={k}>{msg}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
