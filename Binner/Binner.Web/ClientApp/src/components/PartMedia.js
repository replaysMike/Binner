import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Icon, Button, Image, Popup, Loader, Header, Card, Menu, Placeholder } from "semantic-ui-react";
import _ from "underscore";
import { fetchApi } from "../common/fetchApi";
import PropTypes from "prop-types";
import { GetTypeName, GetTypeValue } from "../common/Types";
import { StoredFileType } from "../common/StoredFileType";
import Dropzone from "../components/Dropzone";
import Carousel from "react-bootstrap/Carousel";
import { toast } from "react-toastify";
import { getAuthToken, getImagesToken } from "../common/authentication";
import axios from "axios";

const ProductImageIntervalMs = 10 * 1000;

export function PartMedia({ infoResponse, datasheet, part }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [datasheetTitle, setDatasheetTitle] = useState(datasheet?.title);
  const [datasheetPartName, setDatasheetPartName] = useState(datasheet?.partName);
  const [datasheetDescription, setDatasheetDescription] = useState(datasheet?.description);
  const [datasheetManufacturer, setDatasheetManufacturer] = useState(datasheet?.manufacturer);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);
  const [files, setFiles] = useState([]);
  const [isDirty, setIsDirty] = useState(false);
  const [metadata, setMetadata] = useState({});
  const [loadingPartMetadata, setLoadingPartMetadata] = useState(true);
  const [confirmLocalFileDeleteContent, setConfirmLocalFileDeleteContent] = useState(null);
  const [confirmDeleteLocalFileIsOpen, setConfirmDeleteLocalFileIsOpen] = useState(false);
  const [selectedLocalFile, setSelectedLocalFile] = useState(null);

  useEffect(() => {
    setMetadata(infoResponse);
  }, [infoResponse]);

  useEffect(() => {
    setDatasheetMeta(datasheet);
  }, [datasheet]);

  const setDatasheetMeta = (datasheet) => {
		console.log('setDatasheetMeta', datasheet);
		if (!datasheet)
			return;
    const partName = datasheet.name;
    const title = datasheet.value.title;
    const description = datasheet.value.description;
    const manufacturer = datasheet.value.manufacturer;
    setDatasheetTitle(title);
    setDatasheetPartName(partName);
    setDatasheetManufacturer(manufacturer);
    setDatasheetDescription(description);
  };

  const onUploadSubmit = async (uploadFiles, type) => {
    setUploading(true);
    if (!part.partId) {
      toast.warn(t("message.uploadWait", "Files can't be uploaded until the part is saved."));
      return;
    }
    if (uploadFiles && uploadFiles.length > 0) {
      const requestData = new FormData();
      requestData.append("partId", part.partId);
      requestData.append("storedFileType", GetTypeValue(StoredFileType, type));
      for (let i = 0; i < uploadFiles.length; i++) requestData.append("files", uploadFiles[i], uploadFiles[i].name);

      // first fetch some data using fetchApi, to leverage 401 token refresh
      fetchApi("api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: `api/storedFile`,
            data: requestData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((response) => {
            const { data } = response;
            let errorMessage;
            toast.dismiss();
            if (data.errors && data.errors.length > 0) {
              errorMessage = data.errors.map((err, key) => <li key={key}>{err.message}</li>);
              toast.error(t("message.uploadFailed", "Failed to upload file!"), { autoClose: 10000 });
              setError(<ul className="errors">{errorMessage}</ul>);
            } else {
              // success uploading
              if (uploadFiles.length === 1) toast.success(t("message.uploadSuccess", "File uploaded."));
              else toast.success(t("message.uploadSuccessX", "{{count}} files uploaded.", { count: uploadFiles.length }));

              // add it to the local data
              var typeValue = GetTypeValue(StoredFileType, type);
              var i = 0;
              switch (typeValue) {
                case StoredFileType.ProductImage:
                  const productImages = [...metadata.productImages];
                  for (i = 0; i < data.length; i++) {
                    productImages.unshift({
                      name: data[i].originalFileName,
                      value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                      id: data[i].storedFileId
                    });
                  }
                  setMetadata({ ...metadata, productImages });
                  break;
                case StoredFileType.Datasheet:
                  const datasheets = [...metadata.datasheets];
                  for (i = 0; i < data.length; i++) {
                    const datasheet = {
                      name: data[i].originalFileName,
                      value: {
                        datasheetUrl: `/api/storedFile/local?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        description: data[i].originalFileName,
                        imageUrl: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        manufacturer: "",
                        title: data[i].originalFileName
                      },
                      id: data[i].storedFileId
                    };
                    datasheets.unshift(datasheet);
                    setDatasheetMeta(datasheet);
                  }
                  setMetadata({ ...metadata, datasheets });
                  break;
                case StoredFileType.Pinout:
                  const pinoutImages = [...metadata.pinoutImages];
                  for (i = 0; i < data.length; i++) {
                    pinoutImages.unshift({
                      name: data[i].originalFileName,
                      value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                      id: data[i].storedFileId
                    });
                  }
                  setMetadata({ ...metadata, pinoutImages });
                  break;
                case StoredFileType.ReferenceDesign:
                  const circuitImages = [...metadata.circuitImages];
                  for (i = 0; i < data.length; i++) {
                    circuitImages.unshift({
                      name: data[i].originalFileName,
                      value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                      id: data[i].storedFileId
                    });
                  }
                  setMetadata({ ...metadata, circuitImages });
                  break;
                default:
              }

              setError(null);
              setIsDirty(false);
            }
            setUploading(false);
            setFiles([]);
          })
          .catch((error) => {
            toast.dismiss();
            console.error("error", error);
            if (error.code === "ERR_NETWORK") {
              const msg = t("message.unableToUpload", "Unable to upload. Check that the file is not locked or deleted.");
              toast.error(msg, { autoClose: 10000 });
              setError(msg);
            } else {
              toast.error(t("message.uploadFailed", "Failed to upload file!"));
              setError(error.message);
            }
            setIsDirty(false);
            setUploading(false);
            setFiles([]);
          });
      });
    } else {
      toast.error(t("message.noFilesSelected", "No files selected for upload!"));
    }
  };

  const onUploadError = (errors) => {
    for (let i = 0; i < errors.length; i++) toast.error(errors[i], { autoClose: 10000 });
  };

  const visitAnchor = (e, anchor) => {
    e.preventDefault();
    var redirectToURL = document.URL.replace(/#.*$/, "");

    redirectToURL = redirectToURL + anchor;
    window.location.href = redirectToURL;
  };

  const getDatasheetAttributes = (datasheet) => {
    return {
      "data-id": datasheet.id || "0",
      "data-resourceid": datasheet.value.resourceId
    };
  };

  const onCurrentDatasheetChanged = (activeIndex, control) => {
    setDatasheetMeta(infoResponse.datasheets[activeIndex]);
  };

  const confirmDeleteLocalFileOpen = (e, localFile, type) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteLocalFileIsOpen(true);
    setSelectedLocalFile({ localFile, type });
    setConfirmLocalFileDeleteContent(
      <p>
        <Trans i18nKey="confirm.deleteLocalFile" name={localFile.name}>
          Are you sure you want to delete this local file named <b>{{ name: localFile.name }}</b>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  return (
    <>
      <Menu className="shortcuts">
        <Menu.Item onClick={(e) => visitAnchor(e, "#datasheets")}>{t("page.inventory.datasheets", "Datasheets")}</Menu.Item>
        <Menu.Item onClick={(e) => visitAnchor(e, "#pinout")}>{t("page.inventory.pinout", "Pinout")}</Menu.Item>
        <Menu.Item onClick={(e) => visitAnchor(e, "#circuits")}>{t("page.inventory.circuits", "Circuits")}</Menu.Item>
      </Menu>

      {/* Product Images Carousel */}
      <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ProductImage)}>
        <Card color="blue">
          {metadata && metadata.productImages && metadata.productImages.length > 0 ? (
            <Carousel variant="dark" interval={ProductImageIntervalMs} className="centered product-images">
              {metadata.productImages
                ?.filter((x) => x.value.length > 0)
                ?.map((productImage, imageKey) => (
                  <Carousel.Item key={imageKey}>
                    <Image src={productImage.value} size="large" />
                    {productImage.id && (
                      <Popup
                        position="top left"
                        content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                        trigger={
                          <Button
                            onClick={(e) => confirmDeleteLocalFileOpen(e, productImage, "productImages")}
                            type="button"
                            size="tiny"
                            style={{ position: "absolute", top: "4px", right: "2px", padding: "2px", zIndex: "9999" }}
                            color="red"
                          >
                            <Icon name="delete" style={{ margin: 0 }} />
                          </Button>
                        }
                      />
                    )}
                    <Carousel.Caption>
                      <h5>{productImage.name}</h5>
                    </Carousel.Caption>
                  </Carousel.Item>
                ))}
            </Carousel>
          ) : (
            <Placeholder>
              <img src="/image/microchip.png" className="square" alt="" />
            </Placeholder>
          )}

          <Card.Content>
            <Loader active={loadingPartMetadata} inline size="small" as="i" style={{ float: "right" }} />
            <Header as="h4">
              <Icon name="images" />
              {t("page.inventory.productImages", "Product Images")}
            </Header>
          </Card.Content>
        </Card>
      </Dropzone>

      {/* DATASHEETS */}
      <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Datasheet)}>
        <Card id="datasheets" color="green">
          {metadata && metadata.datasheets && metadata.datasheets.length > 0 ? (
            <div>
              <Carousel variant="dark" interval={null} onSelect={onCurrentDatasheetChanged} className="datasheets">
                {metadata.datasheets.map((datasheet, datasheetKey) => (
                  <Carousel.Item key={datasheetKey} onClick={(e) => handleVisitLink(e, datasheet.value.datasheetUrl)} {...getDatasheetAttributes(datasheet)}>
                    <Image src={datasheet.value.imageUrl} size="large" />
                    {datasheet.id && (
                      <Popup
                        position="top left"
                        content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                        trigger={
                          <Button
                            onClick={(e) => confirmDeleteLocalFileOpen(e, datasheet, "datasheets")}
                            type="button"
                            size="tiny"
                            style={{ position: "absolute", top: "4px", right: "2px", padding: "2px", zIndex: "9999" }}
                            color="red"
                          >
                            <Icon name="delete" style={{ margin: 0 }} />
                          </Button>
                        }
                      />
                    )}
                  </Carousel.Item>
                ))}
              </Carousel>
              <Card.Content style={{ textAlign: "left" }}>
                <Card.Header>{datasheetTitle}</Card.Header>
                <Card.Meta>
                  {datasheetPartName}, {datasheetManufacturer}
                </Card.Meta>
                <Card.Description>{datasheetDescription}</Card.Description>
              </Card.Content>
            </div>
          ) : (
            <Placeholder>
              <img src="/image/datasheet.png" className="square" alt="" />
              <Placeholder.Header>
                <Placeholder.Line length="very long" />
                <Placeholder.Line length="medium" />
                <Placeholder.Line length="short" />
              </Placeholder.Header>
            </Placeholder>
          )}
          <Card.Content extra>
            <Header as="h4">
              <Icon name="file pdf" />
              {t("page.inventory.datasheets", "Datasheets")}
            </Header>
          </Card.Content>
        </Card>
      </Dropzone>

      {/* PINOUT */}

      <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Pinout)}>
        <Card id="pinout" color="purple">
          {metadata && metadata.pinoutImages && metadata.pinoutImages.length > 0 ? (
            <div>
              <Carousel variant="dark" interval={null} className="pinout-images">
                {metadata.pinoutImages.map((pinout, pinoutKey) => (
                  <Carousel.Item key={pinoutKey}>
                    <Image src={pinout.value} size="large" />
                    {pinout.id && (
                      <Popup
                        position="top left"
                        content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                        trigger={
                          <Button
                            onClick={(e) => confirmDeleteLocalFileOpen(e, pinout, "pinoutImages")}
                            type="button"
                            size="tiny"
                            style={{ position: "absolute", top: "4px", right: "2px", padding: "2px", zIndex: "9999" }}
                            color="red"
                          >
                            <Icon name="delete" style={{ margin: 0 }} />
                          </Button>
                        }
                      />
                    )}
                    <Carousel.Caption>
                      <h5>{pinout.name}</h5>
                    </Carousel.Caption>
                  </Carousel.Item>
                ))}
              </Carousel>
            </div>
          ) : (
            <Placeholder>
              <img src="/image/pinout.png" className="square" alt="" />
            </Placeholder>
          )}
          <Card.Content extra>
            <Header as="h4">
              <Icon name="pin" />
              {t("page.inventory.pinout", "Pinout")}
            </Header>
          </Card.Content>
        </Card>
      </Dropzone>

      {/* CIRCUITS */}

      <Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ReferenceDesign)}>
        <Card id="circuits" color="violet">
          {metadata && metadata.circuitImages && metadata.circuitImages.length > 0 ? (
            <div>
              <Carousel variant="dark" interval={null} className="circuit-images">
                {metadata.circuitImages.map((circuit, circuitKey) => (
                  <Carousel.Item key={circuitKey}>
                    <Image src={circuit.value} size="large" />
                    {circuit.id && (
                      <Popup
                        position="top left"
                        content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                        trigger={
                          <Button
                            onClick={(e) => confirmDeleteLocalFileOpen(e, circuit, "circuitImages")}
                            type="button"
                            size="tiny"
                            style={{ position: "absolute", top: "4px", right: "2px", padding: "2px", zIndex: "9999" }}
                            color="red"
                          >
                            <Icon name="delete" style={{ margin: 0 }} />
                          </Button>
                        }
                      />
                    )}
                    <Carousel.Caption>
                      <h5>{circuit.name}</h5>
                    </Carousel.Caption>
                  </Carousel.Item>
                ))}
              </Carousel>
            </div>
          ) : (
            <Placeholder>
              <img src="/image/referencedesign.png" className="square" alt="" />
            </Placeholder>
          )}
          <Card.Content extra>
            <Header as="h4">
              <Icon name="microchip" />
              {t("page.inventory.referenceDesigns", "Reference Designs")}
            </Header>
          </Card.Content>
        </Card>
      </Dropzone>
    </>
  );
}

PartMedia.propTypes = {
  /** part metadata info */
  infoResponse: PropTypes.object.isRequired,
  datasheet: PropTypes.object,
  part: PropTypes.object
};

PartMedia.defaultProps = {};
