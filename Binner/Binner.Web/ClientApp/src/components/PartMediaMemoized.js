import React, { useState, useEffect, useMemo } from "react";
import { useTranslation, Trans } from "react-i18next";
import PropTypes from "prop-types";
import { Icon, Button, Image, Popup, Loader, Header, Card, Menu, Placeholder, Dimmer, Confirm } from "semantic-ui-react";
import _ from "underscore";
import Carousel from "react-bootstrap/Carousel";
import axios from "axios";
import { toast } from "react-toastify";
import { fetchApi } from "../common/fetchApi";
import { GetTypeName, GetTypeValue } from "../common/Types";
import { getUrlForResource } from "../common/resources";
import { StoredFileType } from "../common/StoredFileType";
import Dropzone from "./Dropzone";
import { getAuthToken, getImagesToken } from "../common/authentication";
import { CircuitViewerModal } from "./modals/CircuitViewerModal";
import { PinoutViewerModal } from "./modals/PinoutViewerModal";
import "./PartMedia.css";

/** 
 * Display the media for a part, from a part info response.
 * (Right side column of Inventory page)
 * [memoized]
 */
export function PartMediaMemoized({ infoResponse, datasheet, circuit, pinout, part, loadingPartMetadata }) {
	const ProductImageIntervalMs = 10 * 1000;
  const { t } = useTranslation();
  const [datasheetTitle, setDatasheetTitle] = useState(datasheet?.title);
  const [datasheetPartName, setDatasheetPartName] = useState(datasheet?.partName);
  const [datasheetDescription, setDatasheetDescription] = useState(datasheet?.description);
  const [datasheetManufacturer, setDatasheetManufacturer] = useState(datasheet?.manufacturer);
  const [circuitTitle, setCircuitTitle] = useState(circuit?.name);
  const [circuitPartName, setCircuitPartName] = useState(circuit?.partName);
  const [circuitDescription, setCircuitDescription] = useState(circuit?.description);
  const [circuitManufacturer, setCircuitManufacturer] = useState(circuit?.manufacturer);
  const [pinoutTitle, setPinoutTitle] = useState(pinout?.manufacturerPartName || pinout?.partName);
  const [pinoutPartName, setPinoutPartName] = useState(pinout?.manufacturerPartName || pinout?.partName);
  const [pinoutDescription, setPinoutDescription] = useState(`${pinout?.packageName} (${pinout?.pinCount} pins)`);
  const [pinoutManufacturer, setPinoutManufacturer] = useState(pinout?.manufacturerName);
	const [thePart, setThePart] = useState(part);
  const [uploading, setUploading] = useState(false);
  const [metadata, setMetadata] = useState({});
  const [confirmLocalFileDeleteContent, setConfirmLocalFileDeleteContent] = useState(null);
  const [confirmDeleteLocalFileIsOpen, setConfirmDeleteLocalFileIsOpen] = useState(false);
  const [selectedLocalFile, setSelectedLocalFile] = useState(null);
  const [pinoutIsOpen, setPinoutIsOpen] = useState(false);
  const [circuitIsOpen, setCircuitIsOpen] = useState(false);

  useEffect(() => {
    setMetadata(infoResponse);
  }, [infoResponse]);

  useEffect(() => {
    setDatasheetMeta(datasheet);
  }, [datasheet]);

  useEffect(() => {
    setPinoutMeta(pinout);
  }, [pinout]);

  useEffect(() => {
    setCircuitMeta(circuit);
  }, [circuit]);

	useEffect(() => {
    setThePart(part);
  }, [part]);

  const setDatasheetMeta = (datasheet) => {
		if (!datasheet)
			return;
    setDatasheetTitle(datasheet.value.title);
    setDatasheetPartName(datasheet.name);
    setDatasheetManufacturer(datasheet.value.manufacturer);
    setDatasheetDescription(datasheet.value.description);
  };

  const setPinoutMeta = (pinout) => {
    if (!pinout)
      return;
    setPinoutTitle(pinout?.manufacturerPartName || pinout?.partName);
    setPinoutPartName(pinout?.manufacturerPartName || pinout?.partName);
    setPinoutManufacturer(pinout?.manufacturerName);
    setPinoutDescription(`${pinout?.packageName} (${pinout?.pinCount} pins)`);
  };

  const setCircuitMeta = (circuit) => {
    if (!circuit)
      return;
    setCircuitTitle(circuit.name);
    setCircuitPartName(circuit.partName);
    setCircuitManufacturer(circuit.description);
    setCircuitDescription(circuit.manufacturer);
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
      fetchApi("/api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: `/api/storedFile`,
            data: requestData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((response) => {
            const { data } = response;
            let errorMessage;
            toast.dismiss();
            if (data.errors && data.errors.length > 0) {
              const errorMessage = data.errors.join('. ');
              toast.error(t("message.uploadFailed", `Failed to upload file! ${errorMessage}`), { autoClose: 10000 });
            } else {
              // success uploading
              if (uploadFiles.length === 1) toast.success(t("message.uploadSuccess", "File uploaded."));
              else toast.success(t("message.uploadSuccessX", "{{count}} files uploaded.", { count: uploadFiles.length }));

              // add it to the local data
              var typeValue = GetTypeValue(StoredFileType, type);
              var i = 0;
              switch (typeValue) {
                case StoredFileType.ProductImage:
                  {
                    const productImages = [...metadata.productImages];
                    for (i = 0; i < data.length; i++) {
                      productImages.unshift({
                        name: data[i].originalFileName,
                        value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        url: `/api/storedFile/local?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        id: data[i].storedFileId
                      });
                    }
                    setMetadata({ ...metadata, productImages });
                  }
                  break;
                case StoredFileType.Datasheet:
                  {
                    const datasheets = [...metadata.datasheets];
                    for (i = 0; i < data.length; i++) {
                      const datasheet = {
                        name: data[i].originalFileName,
                        value: {
                          datasheetUrl: `/api/storedFile/local?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                          description: data[i].originalFileName,
                          imageUrl: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                          manufacturer: "",
                          title: data[i].originalFileName,
                        },
                        id: data[i].storedFileId
                      };
                      datasheets.unshift(datasheet);
                      setDatasheetMeta(datasheet);
                    }
                    setMetadata({ ...metadata, datasheets });
                  }
                  break;
                case StoredFileType.Pinout:
                  {
                    const pinouts = [...metadata.pinouts];
                    for (i = 0; i < data.length; i++) {
                      pinouts.unshift({
                        name: data[i].originalFileName,
                        value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        url: `/api/storedFile/local?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        id: data[i].storedFileId,
                      });
                    }
                    setMetadata({ ...metadata, pinouts });
                  }
                  break;
                case StoredFileType.ReferenceDesign:
                  {
                    const circuits = [...metadata.circuits];
                    for (i = 0; i < data.length; i++) {
                      circuits.unshift({
                        name: data[i].originalFileName,
                        value: `/api/storedFile/preview?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        url: `/api/storedFile/local?fileName=${data[i].fileName}&token=${getImagesToken()}`,
                        id: data[i].storedFileId,
                      });
                    }
                    setMetadata({ ...metadata, circuits });
                  }
                  break;
                default:
              }
            }
            setUploading(false);
          })
          .catch((error) => {
            toast.dismiss();
            console.error("error", error);
            if (error.code === "ERR_NETWORK") {
              const msg = t("message.unableToUpload", "Unable to upload. Check that the file is not locked or deleted.");
              toast.error(msg, { autoClose: 10000 });
            } else {
              toast.error(t("message.uploadFailed", `Failed to upload file! ${error.message}`));
            }
            setUploading(false);
          });
      });
    } else {
      toast.error(t("message.noFilesSelected", "No files selected for upload!"));
    }
  };

	const handleDeleteLocalFile = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await fetchApi(`/api/storedfile`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ storedFileId: selectedLocalFile.localFile.id })
    });
    var itemsExceptDeleted;
    switch (selectedLocalFile.type) {
      case "productImages":
        itemsExceptDeleted = _.without(infoResponse.productImages, _.findWhere(infoResponse.productImages, { id: selectedLocalFile.localFile.id }));
        setMetadata({ ...infoResponse, productImages: itemsExceptDeleted });
        break;
      case "datasheets":
        itemsExceptDeleted = _.without(infoResponse.datasheets, _.findWhere(infoResponse.datasheets, { id: selectedLocalFile.localFile.id }));
        setMetadata({ ...infoResponse, datasheets: itemsExceptDeleted });
        if (itemsExceptDeleted.length > 0) setDatasheetMeta(itemsExceptDeleted[0]);
        break;
      case "pinouts":
        itemsExceptDeleted = _.without(infoResponse.pinouts, _.findWhere(infoResponse.pinouts, { id: selectedLocalFile.localFile.id }));
        setMetadata({ ...infoResponse, pinouts: itemsExceptDeleted });
        break;
      case "circuits":
        itemsExceptDeleted = _.without(infoResponse.circuits, _.findWhere(infoResponse.circuits, { id: selectedLocalFile.localFile.id }));
        setMetadata({ ...infoResponse, circuits: itemsExceptDeleted });
        break;
      default:
    }

    setConfirmDeleteLocalFileIsOpen(false);
    setSelectedLocalFile(null);
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
    setDatasheetMeta(metadata.datasheets[activeIndex]);
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

  const handleVisitLink = async (e, data) => {
    e.preventDefault();
    e.stopPropagation();

    if (data && data.localfile) {
      // check if the stored file exists first
      await fetchApi(`/api/storedfile/exists?fileName=${data.localfile}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json"
        },
      }).then(() => {
        // OK, file exists
        if (data.value.datasheetUrl)
          window.open(data.value.datasheetUrl, "_blank");
        else
          window.open(data.url || data.value, "_blank");
      }).catch(err => {
        // file not found, or file read error
        if (err.data.status === 404)
          toast.error(`User file '${data.localfile}' does not exist on disk!`)
        else
          toast.error(`Failed to read local file '${data.localfile}'!`)
      });
    } else {
      // open the external link
      if (data.value.datasheetUrl)
        window.open(data.value.datasheetUrl, "_blank");
      else
        window.open(data.url || data.value, "_blank");
    }
  };

	const confirmDeleteLocalFileClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteLocalFileIsOpen(false);
    setSelectedLocalFile(null);
  };

  const handleViewPinout = (e, pinout) => {
    setPinoutIsOpen(true);
  };

  const handleViewCircuit = (e, circuit) => {
    setCircuitIsOpen(true);
  };

	const renderPartMedia = useMemo(() => {
		return (
			<>
				<Confirm
					className="confirm"
					header={t('confirm.header.deleteFile', "Delete File")}
					open={confirmDeleteLocalFileIsOpen}
					onCancel={confirmDeleteLocalFileClose}
					onConfirm={handleDeleteLocalFile}
					content={confirmLocalFileDeleteContent}
				/>
        <CircuitViewerModal circuit={metadata?.circuits?.[0]} isOpen={circuitIsOpen} onClose={() => setCircuitIsOpen(false)} />
        <PinoutViewerModal pinout={metadata?.pinouts?.[0]} datasheet={metadata?.datasheets?.[0].value.datasheetUrl} isOpen={pinoutIsOpen} onClose={() => setPinoutIsOpen(false)} />
				<Menu className="shortcuts">
					<Menu.Item onClick={(e) => visitAnchor(e, "#datasheets")}>{t("page.inventory.datasheets", "Datasheets")}</Menu.Item>
					<Menu.Item onClick={(e) => visitAnchor(e, "#pinout")}>{t("page.inventory.pinout", "Pinout")}</Menu.Item>
					<Menu.Item onClick={(e) => visitAnchor(e, "#circuits")}>{t("page.inventory.circuits", "Circuits")}</Menu.Item>
				</Menu>
	
				{/* Product Images Carousel */}
				<Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ProductImage)}>
					<Dimmer.Dimmable as={Card} id="productImages" color="blue">
						<Dimmer active={uploading} inverted><Loader /></Dimmer>
						{metadata && metadata.productImages && metadata.productImages.length > 0 ? (
							<Carousel variant="dark" interval={ProductImageIntervalMs} className="centered product-images">
								{metadata.productImages
									?.filter((x) => x.value.length > 0)
									?.map((productImage, imageKey) => (
                    <Carousel.Item key={imageKey} onClick={(e) => handleVisitLink(e, productImage)}>
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
                <span>
								  <Icon name="images" />
								  {t("page.inventory.productImages", "Product Images")}
                </span>
							</Header>
						</Card.Content>
					</Dimmer.Dimmable>
				</Dropzone>
	
				{/* DATASHEETS */}
				<Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Datasheet)}>
					<Dimmer.Dimmable as={Card} id="datasheets" color="green">
						<Dimmer active={uploading} inverted><Loader /></Dimmer>
						{metadata && metadata.datasheets && metadata.datasheets.length > 0 ? (
							<div>
								<Carousel variant="dark" interval={null} onSelect={onCurrentDatasheetChanged} className="datasheets">
									{metadata.datasheets.map((datasheet, datasheetKey) => (
										<Carousel.Item key={datasheetKey} onClick={(e) => handleVisitLink(e, datasheet)} {...getDatasheetAttributes(datasheet)}>
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
                <span>
								  <Icon name="file pdf" />
								  {t("page.inventory.datasheets", "Datasheets")}
                </span>
							</Header>
						</Card.Content>
					</Dimmer.Dimmable>
				</Dropzone>
	
				{/* PINOUT */}
	
				<Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.Pinout)}>
					<Dimmer.Dimmable as={Card} id="pinout" color="purple">
						<Dimmer active={uploading} inverted><Loader /></Dimmer>
            {metadata && metadata.pinouts && metadata.pinouts.length > 0 ? (
							<div>
								<Carousel variant="dark" interval={null} className="pinout-images">
									{metadata.pinouts.map((pinout, pinoutKey) => (
                    <Carousel.Item key={pinoutKey} onClick={(e) => handleViewPinout(e, pinout)} style={{position: 'relative'}}>
                      <Image src={getUrlForResource(pinout.exportImage)} size="large" />
                      <div style={{ position: "absolute", top: '4px', right: "2px", zIndex: '9999' }}>
                        <Icon name="clone outline" style={{ cursor: "pointer" }} />
                        {pinout.id && (
                          <Popup
                            position="top left"
                            content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                            trigger={
                              <Button
                                onClick={(e) => confirmDeleteLocalFileOpen(e, pinout, "pinouts")}
                                type="button"
                                size="tiny"
                                color="red"
                              >
                                <Icon name="delete" style={{ margin: 0 }} />
                              </Button>
                            }
                          />
                        )}
                      </div>
										</Carousel.Item>
									))}
								</Carousel>
                <Card.Content style={{ textAlign: "left" }}>
                  <Card.Header>{pinoutTitle}</Card.Header>
                  <Card.Meta>
                    {pinoutPartName}, {pinoutManufacturer}
                  </Card.Meta>
                  <Card.Description>{pinoutDescription}</Card.Description>
                </Card.Content>
							</div>
						) : (
							<Placeholder>
								<img src="/image/pinout.png" className="square" alt="" />
							</Placeholder>
						)}
						<Card.Content extra>
							<Header as="h4">
                <span>
								  <Icon name="microchip" />
								  {t("page.inventory.pinout", "Pinout")}
                </span>
							</Header>
						</Card.Content>
					</Dimmer.Dimmable>
				</Dropzone>
	
				{/* CIRCUITS */}
	
				<Dropzone onUpload={onUploadSubmit} onError={onUploadError} type={GetTypeName(StoredFileType, StoredFileType.ReferenceDesign)}>
					<Dimmer.Dimmable as={Card} id="circuits" color="violet">
						<Dimmer active={uploading} inverted><Loader /></Dimmer>
            {metadata && metadata.circuits && metadata.circuits.length > 0 ? (
							<div>
								<Carousel variant="dark" interval={null} className="circuit-images">
                  {metadata.circuits.map((circuit, circuitKey) => (
                    <Carousel.Item key={circuitKey} onClick={(e) => handleViewCircuit(e, circuit)}>
                      <Image src={getUrlForResource(circuit.outputImage)} size="large" />
                      <div style={{ position: "absolute", top: '4px', right: "2px", zIndex: '9999', color: 'white' }}>
                        <Icon name="clone outline" style={{ cursor: "pointer" }} />
                        {circuit.id && (
                          <Popup
                            position="top left"
                            content={t("page.inventory.popup.deleteLocalFile", "Delete this local file")}
                            trigger={
                              <Button
                                onClick={(e) => confirmDeleteLocalFileOpen(e, circuit, "circuits")}
                                type="button"
                                size="tiny"
                                color="red"
                              >
                                <Icon name="delete" style={{ margin: 0 }} />
                              </Button>
                            }
                          />
                        )}
                      </div>
										</Carousel.Item>
									))}
								</Carousel>
                <Card.Content style={{ textAlign: "left" }}>
                  <Card.Header>{circuitTitle}</Card.Header>
                  <Card.Meta>
                    {circuitPartName}, {circuitManufacturer}
                  </Card.Meta>
                  <Card.Description>{circuitDescription}</Card.Description>
                </Card.Content>
							</div>
						) : (
							<Placeholder>
								<img src="/image/referencedesign.png" className="square" alt="" />
							</Placeholder>
						)}
						<Card.Content extra>
							<Header as="h4">
                <span>
                  <i className="pcb-icon small" />
								  {t("page.inventory.circuits", "Circuits")}
                </span>
							</Header>
						</Card.Content>
					</Dimmer.Dimmable>
				</Dropzone>
			</>
		);
  }, [metadata, datasheetTitle, datasheetPartName, datasheetDescription, datasheetManufacturer, thePart, loadingPartMetadata, uploading, setConfirmDeleteLocalFileIsOpen, confirmDeleteLocalFileIsOpen, confirmLocalFileDeleteContent, handleDeleteLocalFile]);

	return (
    <>
			{renderPartMedia}
    </>
  );
}

PartMediaMemoized.propTypes = {
  /** part metadata info */
  infoResponse: PropTypes.object,
  part: PropTypes.object,
  datasheet: PropTypes.object,
  circuit: PropTypes.object,
  pinout: PropTypes.object,
	loadingPartMetadata: PropTypes.bool.isRequired
};
