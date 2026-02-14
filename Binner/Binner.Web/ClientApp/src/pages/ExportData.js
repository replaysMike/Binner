import React, { useState } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { Button, Form, Divider, Grid, Segment, Breadcrumb, Icon } from "semantic-ui-react";
import { toast } from "react-toastify";
import axios from "axios";
import { fetchApi } from "../common/fetchApi";
import { useDropzone } from "react-dropzone";
import { humanFileSize } from "../common/files";
import { FormHeader } from "../components/FormHeader";
import { getAuthToken } from "../common/authentication";

export const ExportData = (props) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [exportFormat, setExportFormat] = useState("");
  const [isDirty, setIsDirty] = useState(false);
  const [errors, setErrors] = useState([]);
  const [file, setFile] = useState(null);
  const [acceptedFile, setAcceptedFile] = useState(null);
  const [importResult, setImportResult] = useState(null);
  const [dragOverClass, setDragOverClass] = useState("");
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
  const { acceptedFiles, getRootProps, getInputProps } = useDropzone({
    maxFiles: 10,
    onDrop: (acceptedFiles, rejectedFiles, e) => {
      setDragOverClass("");
      // do accept manually
      const acceptedMimeTypes = ["application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/sql", "application/x-sql", "text/csv", "application/zip"];
      const errors = [];
      for (let i = 0; i < acceptedFiles.length; i++) {
        if (!acceptedMimeTypes.includes(acceptedFiles[i].type)) {
          errors.push(`${t('page.exportData.fileNotSupported', "File '{{name}}' with mime type '{{type}}' is not an accepted type!", { name: acceptedFiles[i].name, type: acceptedFiles[i].type })}\r\n`);
        }
      }
      for (let i = 0; i < rejectedFiles.length; i++) {
        errors.push(`File ${rejectedFiles[i].file.name}: ${rejectedFiles[i].errors.map(i => i.message).join('.')}`);
      }
      if (errors.length > 0) {
        setFile(null);
        setAcceptedFile(null);
        setErrors(errors);
      } else {
        setFile(URL.createObjectURL(acceptedFiles[0]));
        setAcceptedFile(acceptedFiles[0]);
        setIsDirty(true);
        setErrors(null);
      }
    },
    onDragEnter: (e) => {
      setDragOverClass("droptarget");
    },
    onDragLeave: (e) => {
      setDragOverClass("");
    },
  });

  const onExportSubmit = async (e) => {
    setLoading(true);
    fetchApi("/api/authentication/identity").then((_) => {
      axios
        .request({
          method: "get",
          url: `/api/data/export?exportFormat=${exportFormat}`,
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
          toast.success(t('page.exportData.exportSuccess', "Data exported successfully!"));
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(t('page.exportData.exportFailed', "Export data failed!"));
        });
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

      fetchApi("/api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: "/api/data/import",
            data: formData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((data) => {
            console.debug("data", data);
            setImportResult(data.data);
            if (data.data.success) {
              if (data.data.warnings.length > 0)
                toast.warning(t('page.exportData.importSuccessWithWarnings', "Data imported successfully (with warnings). Check the output below."));
              else
                toast.success(t('page.exportData.importSuccess', "Data imported successfully!"));
            } else {
              toast.error(t('page.exportData.importFailed', "Failed to import data."));
            }
          })
          .catch((error) => {
            toast.dismiss();
            console.error("error", error);
            toast.error(t('page.exportData.importUploadFailed', "Import upload failed!"));
          });
      });
    } else {
      toast.error(t('page.exportData.noFilesSelected', "No files selected for upload!"));
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
        <Breadcrumb.Section active>{t('page.exportData.title', "Import/Export Data")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.exportData.title', "Import/Export Data")} to="/">
        {t('page.exportData.description', "Import or Export your Binner database to a human-readable format.")}
			</FormHeader>
      <Segment loading={loading} className="exportData">
        <Grid columns={2}>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onImportSubmit}>
              <div
                {...getRootProps({ className: `dropzone ${dragOverClass}` })}
              >
                <span style={{ fontSize: "0.6em" }}>{t('page.exportData.uploadNote', "Drag a document to upload, or click to select files")}</span>
                <input {...getInputProps()} />
                <div style={{ fontSize: "0.6em" }}>{t('page.exportData.acceptedFileTypes', "Accepted file types: \"*.sql, *.xls, *.xlsx, *.csv, *.zip\"")}</div>
              </div>
              {errors?.length > 0 && (
                <div className="error small">
                  <b>{t('label.error', "Error")}:</b>
                  <ul className="errors">
                    {errors.map((err, key) => (<li key={key}>{err}</li>))}
                  </ul>
                </div>
              )}
              <aside>
                <ol>
                  {acceptedFiles.length > 0 && acceptedFiles.map((file, key) => (<li key={key}>{file.path} - {humanFileSize(file.size)}</li>))}
                </ol>
              </aside>
              <Button primary disabled={!isDirty || acceptedFiles?.length === 0}>{t('button.import', "Import")}</Button>
            </Form>
          </Grid.Column>
          <Grid.Column className="centered" style={{ padding: "50px" }}>
            <Form onSubmit={onExportSubmit}>
              <div style={{ padding: "50px", marginBottom: "20px", height: "140px" }}>
                <Form.Dropdown
                  label="Format"
                  placeholder={t('page.exportData.chooseFormat', "Choose a format")}
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
            {t('label.status', "Status")}: <b>{importResult.success ? <><Icon name="check circle" color="green" />{t('label.success', "Success")}</> : <><Icon name="times circle" color="red" />{t('label.failed', "Failed")}</>}</b>
          </div>
          <div style={{fontSize: '1.2em'}}>{t('page.exportData.totalRowsImported', "Total Rows Imported")}: <b>{importResult.totalRowsImported}</b></div>
          <br />
          <div>{t('page.exportData.totalProjectsImported', "Projects Imported")}: <b>{importResult.rowsImportedByTable.Projects}</b></div>
          <div>{t('page.exportData.totalPartTypesImported', "Part Types Imported")}: <b>{importResult.rowsImportedByTable.PartTypes}</b></div>
          <div>{t('page.exportData.totalPartsImported', "Parts Imported")}: <b>{importResult.rowsImportedByTable.Parts}</b></div>

          {importResult.errors && importResult.errors.length > 0 && (
            <div style={{ marginTop: "20px" }}>
              <h6>{t('label.errors', "Errors")}:</h6>
              <ul className="errors">
                {importResult.errors.map((msg, k) => (
                  <li key={k}>{msg}</li>
                ))}
              </ul>
            </div>
          )}

          {importResult.warnings && importResult.warnings.length > 0 && (
            <div style={{ marginTop: "20px" }}>
              <h6>{t('label.warnings', "Warnings")}:</h6>
              <ul className="warnings">
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
