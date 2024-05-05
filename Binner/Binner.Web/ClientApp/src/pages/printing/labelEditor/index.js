import React, { useState, useEffect, useMemo, useCallback } from "react";
import { useTranslation, Trans } from "react-i18next";
import { HTML5Backend } from "react-dnd-html5-backend";
import _ from "underscore";
import { Popup, Dropdown, Table, Input, Button, Icon, Image, Checkbox } from "semantic-ui-react";
import debounce from "lodash.debounce";
import AlignVerticalTopIcon from '@mui/icons-material/AlignVerticalTop';
import AlignVerticalCenterIcon from '@mui/icons-material/AlignVerticalCenter';
import AlignVerticalBottomIcon from '@mui/icons-material/AlignVerticalBottom';
import AlignHorizontalLeft from '@mui/icons-material/AlignHorizontalLeft';
import AlignHorizontalCenter from '@mui/icons-material/AlignHorizontalCenter';
import AlignHorizontalRight from '@mui/icons-material/AlignHorizontalRight';
import { LabelDropContainer } from "./LabelDropContainer";
import { DndProvider } from "react-dnd";
import { DraggableBox } from "./DraggableBox";
import { DEFAULT_FONT } from '../../../common/Types';
import { HandleBinaryResponse } from "../../../common/handleResponse.js";
import { fetchApi } from "../../../common/fetchApi";
import { getImagesToken } from "../../../common/authentication";
import { updateStateItem } from "../../../common/reactHelpers";
import { LabelSelectionModal } from "./LabelSelectionModal";
import { LabelSetNameModal } from "./LabelSetNameModal";
import { getChildrenByName } from './labelEditorComponents';
import { toast } from "react-toastify";
import "./labelEditor.css";

export function LabelEditor(props) {
  const PreviewDebounceTimeMs = 500;
  const { t } = useTranslation();
  const defaultItemProperties = {
    align: 0,
    fontSize: 2,
    fontWeight: 0,
    color: 0,
    rotate: 0,
    font: DEFAULT_FONT,
    name: "",
    value: ""
  };
  const [zoomLevel, setZoomLevel] = useState(1);
  const [labelSelectionModalIsOpen, setLabelSelectionModalIsOpen] = useState(false);
  const [labelSetNameModalIsOpen, setLabelSetNameModalIsOpen] = useState(false);
  const [boxes, setBoxes] = useState([]);
  const [label, setLabel] = useState({ labelId: -1, name: "" });
  const [selectedItem, setSelectedItem] = useState(null);
  const [itemProperties, setItemProperties] = useState([]);
  const [selectedItemProperties, setSelectedItemProperties] = useState({});
  const [imgBase64, setImgBase64] = useState("");
  const [clearSelectedItem, setClearSelectedItem] = useState(null);
  const [fontOptions, setFontOptions] = useState([]);
  const [fontsLoading, setFontsLoading] = useState(true);
  const [labelTemplates, setLabelTemplates] = useState([]);
  const [labelTemplatesLoading, setLabelTemplatesLoading] = useState(true);
  const [labelTemplateOptions, setLabelTemplateOptions] = useState([]);
  const [labelTemplate, setLabelTemplate] = useState({});
  const [labelTemplateIsDirty, setLabelTemplateIsDirty] = useState(false);
  const [isDirty, setIsDirty] = useState(false);

  const alignOptions = [
    { key: 0, text: "Center", value: 0 },
    { key: 1, text: "Left", value: 1 },
    { key: 2, text: "Right", value: 2 }
  ];

  const rotateOptions = [
    { key: 0, text: "0 Degrees", value: 0 },
    { key: 1, text: "45 Degrees", value: 1 },
    { key: 2, text: "90 Degrees", value: 2 },
    { key: 3, text: "135 Degrees", value: 3 },
    { key: 4, text: "180 Degrees", value: 4 },
    { key: 5, text: "225 Degrees", value: 5 },
    { key: 6, text: "270 Degrees", value: 6 },
    { key: 7, text: "315 Degrees", value: 7 }
  ];

  const fontSizeOptions = [
    { key: 0, text: "Tiny", value: 0 },
    { key: 1, text: "Small", value: 1 },
    { key: 2, text: "Normal", value: 2 },
    { key: 3, text: "Medium", value: 3 },
    { key: 4, text: "Large", value: 4 },
    { key: 5, text: "ExtraLarge", value: 5 },
    { key: 6, text: "VeryLarge", value: 6 }
  ];

  const fontWeightOptions = [
    { key: 0, text: "Normal", value: 0 },
    { key: 1, text: "Bold", value: 1 },
  ];

  const colorOptions = [
    { key: 0, text: "Black", value: 0 },
    { key: 1, text: "Blue", value: 1, color: "blue" },
    { key: 2, text: "Gray", value: 2, color: "gray" },
    { key: 3, text: "Green", value: 3, color: "green" },
    { key: 4, text: "Orange", value: 4, color: "orange" },
    { key: 5, text: "Purple", value: 5, color: "purple" },
    { key: 6, text: "Red", value: 6, color: "red" },
    { key: 7, text: "Yellow", value: 7, color: "yellow" }
  ];

  const convertInchesToPixels = (inches) => {
    return inches * 96;
  };
  const getItemProperties = useCallback((item, itemProperties) => {
    let properties = _.find(itemProperties, (i) => i.name === item.name);
    if (properties) {
      return properties;
    }
    return null;
  }, []);

  const handlePreview = useCallback(async (boxes, labelTemplate, allItemProperties = null, generateImageOnly = true) => {
    if (!allItemProperties) {
      allItemProperties = itemProperties;
    }
    let zoomAdjust = 1.0 - (zoomLevel - 1.0);
    zoomAdjust = 1;

    if (boxes.length > 0) {
      if (!document.getElementById(boxes[0].id)){
        // loaded boxes haven't been added to the canvas yet.
        // load preview in a little bit when the canvas items are added to the DOM
        setTimeout(async () => {
          await handlePreview(boxes, labelTemplate, allItemProperties, generateImageOnly);
        }, 100);
        return;
      }
    }

    const request = {
      token: getImagesToken(),
      label: { ...labelTemplate },
      boxes: boxes.map((box) => ({
        acceptsValue: box.acceptsValue,
        displaysValue: box.displaysValue,
        id: box.id,
        left: Math.trunc(box.left * zoomAdjust),
        name: box.name,
        resize: box.resize,
        top: Math.trunc(box.top * zoomAdjust),
        properties: getItemProperties(box, allItemProperties) || defaultItemProperties,
        width: (Math.trunc(document.getElementById(box.id)?.clientWidth * zoomAdjust) || 99),
        height: (Math.trunc(document.getElementById(box.id)?.clientHeight * zoomAdjust) || 99),
      })),
      generateImageOnly: generateImageOnly
    };

    return await fetchApi("api/print/beta", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then(async (response) => {
      return await HandleBinaryResponse(response.responseObject).then((b) => {
        const base64 = arrayBufferToBase64(b);
        setImgBase64(base64);
        return base64;
      });
    });
  }, [defaultItemProperties, getItemProperties, zoomLevel, itemProperties]);

  const previewDebounced = useMemo(() => debounce(handlePreview, PreviewDebounceTimeMs), [handlePreview]);

  const handleLabelTemplateChange = useCallback((e, control) => {
    const newValue = { ...labelTemplate, labelTemplateId: control.value };
    // resize the canvas
    const templateOption = _.find(labelTemplates, (i) => i.labelTemplateId === control.value);
    newValue.width = templateOption.width;
    newValue.height = templateOption.height;
    newValue.dpi = templateOption.dpi;
    newValue.margin = templateOption.margin;
    newValue.showBoundaries = templateOption.showBoundaries || false;
    if (templateOption.name === "Custom")
      newValue.name = "";
    else
      newValue.name = templateOption.name;

    setLabelTemplate(newValue);
    previewDebounced(boxes, newValue);
  }, [boxes, labelTemplates, labelTemplate, previewDebounced]);

  const handleItemPropertyChange = useCallback((e, control) => {
    if (selectedItemProperties) {
      selectedItemProperties[control.name] = control.value;
      setSelectedItemProperties({ ...selectedItemProperties });
      const newItemProperties = updateStateItem(itemProperties, selectedItemProperties, "name", selectedItem.name);
      setItemProperties(newItemProperties);
      previewDebounced(boxes, labelTemplate, newItemProperties);
    } else {
      previewDebounced(boxes, labelTemplate);
    }
    setIsDirty(true);
  }, [boxes, labelTemplate, previewDebounced, itemProperties, selectedItemProperties, selectedItem]);

  const handleSelectedItemChanged = useCallback((e, selectedItem) => {
    if (selectedItem === null) {
      setSelectedItem(null);
      setSelectedItemProperties(null);
      return;
    }
    setSelectedItem(selectedItem);
    let itemProp = getItemProperties(selectedItem, itemProperties);
    if (itemProp === null) {
      itemProp = { ...defaultItemProperties, name: selectedItem.name };
      const newItemProperties = [...itemProperties];
      newItemProperties.push(itemProp);
      setItemProperties(newItemProperties);
      setSelectedItemProperties(itemProp);
      previewDebounced(boxes, labelTemplate, newItemProperties);
    } else {
      setSelectedItemProperties(itemProp);
      previewDebounced(boxes, labelTemplate);
    }
  }, [boxes, labelTemplate, previewDebounced, itemProperties, defaultItemProperties, getItemProperties]);


  const handleClearSelectedItem = () => {
    setClearSelectedItem(Math.random());
  };

  const handleOnDrop = useCallback((box) => {
    try {
      const newBoxes = [...boxes];
      newBoxes.push({ ...box });
      setBoxes(newBoxes);
      setIsDirty(true);
      previewDebounced(newBoxes, labelTemplate);
    } catch { }
  }, [boxes, labelTemplate, previewDebounced]);

  const handleOnMove = useCallback((box) => {
    try {
      const newBoxes = updateStateItem(boxes, box, "id", box.id);
      setBoxes(newBoxes);
      setIsDirty(true);
      previewDebounced(newBoxes, labelTemplate);
    } catch { }
  }, [boxes, labelTemplate, previewDebounced]);

  const handleOnRemove = useCallback((box) => {
    try {
      const newBoxes = _.filter(boxes, (i) => i.id !== box.id);
      setBoxes(newBoxes);
      setIsDirty(true);
      previewDebounced(newBoxes, labelTemplate);
    } catch { }
  }, [boxes, labelTemplate, previewDebounced]);

  useEffect(() => {
    const getLabelTemplates = async () => {
      setLabelTemplatesLoading(true);
      await fetchApi("api/print/templates", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((response) => {
        const { data } = response;

        const labelTemplates = [
          { name: "Custom", description: "Create a custom label template", labelTemplateId: -1, width: "1.875", height: "0.5625", dpi: 300, margin: "0", showBoundaries: false },
        ];
        for (let i = 0; i < data.length; i++)
          labelTemplates.push(data[i]);

        setLabelTemplates(labelTemplates);
        setLabelTemplateOptions(labelTemplates.map((item, key) => ({ key, text: item.name, description: item.description, value: item.labelTemplateId })));
        // select the first template as the default value
        const defaultTemplate = _.find(labelTemplates, i => i.labelTemplateId !== -1);
        setLabelTemplate({ ...defaultTemplate, showBoundaries: false });
        setLabelTemplatesLoading(false);
      });
    };

    const loadFonts = async () => {
      setFontsLoading(true);
      await fetchApi("api/print/fonts", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((response) => {
        const { data } = response;
        const newFonts = data.map((l, k) => {
          return {
            key: k,
            value: l,
            text: l
          };
        });
        const selectedFont = _.find(newFonts, (x) => x && x.text === DEFAULT_FONT);
        setFontOptions(newFonts);
        setFontsLoading(false);
      });
    };
    if (fontOptions.length === 0)
      loadFonts();
    if (labelTemplates.length === 0)
      getLabelTemplates();
    if (label?.labelId === -1)
      setLabelSelectionModalIsOpen(true);
  }, []);

  const handleSaveTemplate = async () => {
    // save the template
    const request = {
      ...labelTemplate,
      width: labelTemplate.width + "",
      height: labelTemplate.height + ""
    };

    await fetchApi("api/print/template", {
      method: request.labelTemplateId === -1 ? "POST" : "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      if (response.responseObject.ok) {
        const { data } = response;
        // add it to the list
        const newLabelTemplates = [...labelTemplates];
        if (request.labelTemplateId === -1) {
          newLabelTemplates.push(data);
          setLabelTemplates(newLabelTemplates);
          setLabelTemplateOptions(newLabelTemplates.map((item, key) => ({ key, text: item.name, value: item.labelTemplateId })));
          setLabelTemplate({ ..._.find(newLabelTemplates, i => i.labelTemplateId === data.labelTemplateId), showBoundaries: false });
        }
      } else {
        toast.error('Failed to save label template!');
      }
      setLabelTemplateIsDirty(false);
    });
  };

  const handlePrint = useCallback(() => {
    previewDebounced(boxes, labelTemplate, itemProperties, false);
  }, [boxes, labelTemplate, itemProperties, previewDebounced]);

  const handleOpenPreviewImage = () => {
    const base64ImageData = `data:image/png;base64,${imgBase64}`;
    const contentType = 'image/png';
    const byteCharacters = atob(base64ImageData.substr(`data:${contentType};base64,`.length));
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += 1024) {
      const slice = byteCharacters.slice(offset, offset + 1024);

      const byteNumbers = new Array(slice.length);
      for (let i = 0; i < slice.length; i++) {
        byteNumbers[i] = slice.charCodeAt(i);
      }

      const byteArray = new Uint8Array(byteNumbers);

      byteArrays.push(byteArray);
    }
    const blob = new Blob(byteArrays, { type: contentType });
    const blobUrl = URL.createObjectURL(blob);

    window.open(blobUrl, '_blank');
  };

  const arrayBufferToBase64 = (buffer) => {
    let binary = "";
    let bytes = [].slice.call(new Uint8Array(buffer));

    bytes.forEach((b) => (binary += String.fromCharCode(b)));

    return btoa(binary);
  };

  const handleAlignSelected = useCallback((alignTo) => {
    if (!selectedItem) return;

    const box = _.find(boxes, i => i.id === selectedItem.id);
    const id = box.id;
    const el = document.getElementById(id);
    const margins = getMargins(labelTemplate.margin);
    // margins: top right bottom left
    switch (alignTo) {
      case 'left':
        box.left = margins[3];
        break;
      default:
      case 'center':
        box.left = (((convertInchesToPixels(labelTemplate.width) - margins[1]) / 2.0) - (el.offsetWidth / 2.0));
        break;
      case 'right':
        box.left = (convertInchesToPixels(labelTemplate.width) - el.offsetWidth - margins[1]) - 3;
        break;
      case 'top':
        box.top = margins[0];
        break;
      case 'middle':
        box.top = (((convertInchesToPixels(labelTemplate.height) - margins[0]) / 2.0) - (el.offsetHeight / 2.0));
        break;
      case 'bottom':
        box.top = (convertInchesToPixels(labelTemplate.height) - margins[2] - el.offsetHeight - 2);
        break;
    }
    setSelectedItem(box);
    const newBoxes = [...boxes];
    setBoxes(newBoxes);
    previewDebounced(newBoxes, labelTemplate);
    setIsDirty(true);
  }, [boxes, labelTemplate, previewDebounced, selectedItem]);

  const getMargins = (margin) => {
    const margins = [0, 0, 0, 0];
    let marginDef = margin?.split(' ') || [];

    for (let i = 0; i < marginDef.length; i++)
      margins[i] = parseInt(marginDef[i]);

    if (marginDef.length === 1) {
      margins[1] = margins[2] = margins[3] = margins[0];
    } else if (marginDef.length === 2) {
      margins[2] = margins[0];
      margins[3] = margins[1];
    }
    return margins;
  };

  const handleSave = async (e, labelNameForm) => {
    const request = {
      name: labelNameForm.name,
      labelId: label.labelId,
      isDefaultPartLabel: labelNameForm.isDefaultPartLabel,
      label: { ...labelTemplate },
      boxes: boxes.map((box) => ({
        acceptsValue: box.acceptsValue,
        displaysValue: box.displaysValue,
        id: box.id,
        left: Math.trunc(box.left),
        name: box.name,
        resize: box.resize,
        top: Math.trunc(box.top),
        properties: getItemProperties(box, itemProperties) || defaultItemProperties,
        width: Math.trunc(document.getElementById(box.id)?.clientWidth) || 0,
        height: Math.trunc(document.getElementById(box.id)?.clientHeight) || 0,
      }))
    };

    await fetchApi("api/print/label", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      if (response.responseObject.ok) {
        setIsDirty(false);
        toast.success("Label saved!");
      } else {
        toast.error("Failed to save label!");
      }
      setLabelSetNameModalIsOpen(false);
    });

  };

  const handleLabelTemplateValueChange = useCallback((e, control, propertyName) => {
    const value = { ...labelTemplate };
    switch (control.type) {
      case 'checkbox':
        value[propertyName] = control.checked;
        break;
      default:
        value[propertyName] = control.value;
        break;
    }
    setLabelTemplate(value);
    previewDebounced(boxes, value);
    setLabelTemplateIsDirty(true);
  }, [boxes, labelTemplate, previewDebounced]);

  const handleSelectLabel = useCallback(async (e, label) => {
    setLabelSelectionModalIsOpen(false);
    if (label?.labelId > 0) {
      setLabel(label);
      const template = JSON.parse(label.template);
      setBoxes(template.boxes);
      const newLabelTemplate = { ...template.label, showBoundaries: false };
      setLabelTemplate(newLabelTemplate);
      const newItemProperties = template.boxes.map((item, key) => ({
        ...item.properties, name: item.name
      }));
      setItemProperties(newItemProperties);
      // force a preview
      await handlePreview(template.boxes, newLabelTemplate, newItemProperties);
    } else {
      setBoxes([]);
      setItemProperties([]);
      setSelectedItem(null);
      setSelectedItemProperties({});
      setLabel({ labelId: -1, name: "" });
      setImgBase64("");
    }
  }, [handlePreview]);

  const handleLoad = () => {
    setLabelSelectionModalIsOpen(true);
  };

  const handleZoomIn = () => {
    let newZoomLevel = zoomLevel + 0.1;
    if (newZoomLevel > 2)
      newZoomLevel = 2;
    setZoomLevel(newZoomLevel);
  };

  const handleZoomOut = () => {
    let newZoomLevel = zoomLevel - 0.1;
    if (newZoomLevel < 0.5)
      newZoomLevel = 0.5;
    setZoomLevel(newZoomLevel);
  };

  const handleSelectedItemLocationChange = useCallback((e, control) => {
    // handle manual entry of selected item x/y/width/height
    if (selectedItem) {
      const el = document.getElementById(selectedItem.id);
      switch (control.name) {
        case 'itemPropertyX':
          setSelectedItem({ ...selectedItem, left: parseInt(control.value) });
          el.style.left = parseInt(control.value) + 'px';
          break;
        case 'itemPropertyY':
          setSelectedItem({ ...selectedItem, top: control.value });
          el.style.top = parseInt(control.value) + 'px';
          break;
        case 'itemPropertyWidth':
          el.style.width = parseInt(control.value) + 'px';
          setSelectedItem({ ...selectedItem });
          break;
        case 'itemPropertyHeight':
          el.style.height = parseInt(control.value) + 'px';
          setSelectedItem({ ...selectedItem });
          break;
        default:
          break;
      }
      previewDebounced(boxes, labelTemplate);
      setIsDirty(true);
    }
  }, [boxes, labelTemplate, previewDebounced, selectedItem]);

  return (
    <div className="labelEditor">
      <LabelSelectionModal isOpen={labelSelectionModalIsOpen} onSelect={handleSelectLabel} onClose={() => setLabelSelectionModalIsOpen(false)} />
      <LabelSetNameModal isOpen={labelSetNameModalIsOpen} name={label?.name || ""} isDefaultPartLabel={label?.isPartLabelTemplate || false} onSave={handleSave} onClose={() => setLabelSetNameModalIsOpen(false)} />
      <DndProvider backend={HTML5Backend}>
        <div className="tools left">
          <div style={{ display: "flex", flexDirection: "column", flex: "1", alignItems: "start" }}>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.barcodeLabels', "Barcode Labels")}</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="qrCode" resize="both" acceptsValue={true}>
                <Popup wide content="QR Code" trigger={getChildrenByName('qrCode')} />
              </DraggableBox>
              <DraggableBox name="dataMatrix2d" resize="both" acceptsValue={true}>
                <Popup wide content="Data Matrix (2D) Barcode" trigger={getChildrenByName('dataMatrix2d')} />
              </DraggableBox>
              <DraggableBox name="aztecCode" resize="both" acceptsValue={true}>
                <Popup wide content="Aztec Code" trigger={getChildrenByName('aztecCode')} />
              </DraggableBox>
              <DraggableBox name="code128" resize="both" acceptsValue={true}>
                <Popup wide content="Code-128 Barcode" trigger={getChildrenByName('code128')} />
              </DraggableBox>
              <DraggableBox name="pdf417" resize="both" acceptsValue={true}>
                <Popup wide content="PDF417 Barcode" trigger={getChildrenByName('pdf417')} />
              </DraggableBox>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.custom', "Custom")}</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="text" acceptsValue={true} displaysValue={true}>
                <Popup wide content="Custom text string" trigger={getChildrenByName('text')} />
              </DraggableBox>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.preview', "Preview")}</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px", width: '100%' }}>
              {imgBase64 && (
                <Popup
                  hoverable
                  size="huge"
                  wide="very"
                  content={<Image
                    name="previewImage"
                    src={`data:image/png;base64,${imgBase64}`}
                    style={{ border: '1px solid #000', backgroundColor: '#fff' }}
                  />}
                  trigger={<Image
                    name="previewImage"
                    src={`data:image/png;base64,${imgBase64}`}
                    size="medium"
                    style={{ marginTop: "20px", border: '1px solid #000', backgroundColor: '#fff' }}
                  />} />
              )}
              <div style={{textAlign: 'center', marginTop: '2px'}}>
                <Popup
                  wide
                  position="bottom center"
                  content={<p>Manually refresh</p>}
                  trigger={<Button size="mini" style={{ padding: '6px 8px', width: '15%', margin: '1px' }} onClick={() => previewDebounced(boxes, labelTemplate)}><Icon name="refresh" color="blue" /></Button>}
                />
                <Popup 
                  wide
                  position="bottom center"
                  content={<p>Open image in new tab</p>}
                  trigger={<Button size="mini" style={{ padding: '6px 8px', width: '15%' }} onClick={handleOpenPreviewImage}><Icon name="external" color="grey" /></Button>}
                />
              </div>
           </div>
          </div>
        </div>
        <div className="layout">
          <div style={{ display: "flex", flexDirection: "column", flex: "1", alignItems: "center" }}>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.labelEditor', "Label Editor")}</label>
            </div>
            <div className="wrapper" style={{ flex: "1" }} onClick={handleClearSelectedItem}>
              <div className="zoomContainer" style={{ transform: `scale(${zoomLevel})` }}>
                <LabelDropContainer
                  width={convertInchesToPixels(labelTemplate?.width || 0)}
                  height={convertInchesToPixels(labelTemplate?.height || 0)}
                  margin={labelTemplate.margin}
                  itemProperties={itemProperties}
                  resetSelectedItem={clearSelectedItem}
                  onSelectedItemChanged={handleSelectedItemChanged}
                  onDrop={handleOnDrop}
                  onMove={handleOnMove}
                  onRemove={handleOnRemove}
                  boxes={boxes}
                />
                <div style={{ fontSize: "0.9em", fontWeight: "700", color: "#333" }}>{_.find(labelTemplateOptions, (i) => i.value === labelTemplate)?.text}</div>
                <div style={{ fontSize: "0.7em", color: "#999" }}>Drag and drop components onto the label surface above</div>
              </div>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.manage', "Manage")}</label>
              <div>
                <Table celled>
                  <Table.Body>
                    <Table.Row>
                      <Table.Cell colSpan={2} className="alignTools">
                        <Button icon onClick={() => handleAlignSelected('left')} disabled={!selectedItem} size="tiny" title="Align left"><AlignHorizontalLeft /></Button>
                        <Button icon onClick={() => handleAlignSelected('center')} disabled={!selectedItem} size="tiny" title="Align center"><AlignHorizontalCenter /></Button>
                        <Button icon onClick={() => handleAlignSelected('right')} disabled={!selectedItem} size="tiny" title="Align right"><AlignHorizontalRight /></Button>
                        <Button icon onClick={() => handleAlignSelected('top')} disabled={!selectedItem} size="tiny" title="Align top"><AlignVerticalTopIcon /></Button>
                        <Button icon onClick={() => handleAlignSelected('middle')} disabled={!selectedItem} size="tiny" title="Align middle"><AlignVerticalCenterIcon /></Button>
                        <Button icon onClick={() => handleAlignSelected('bottom')} disabled={!selectedItem} size="tiny" title="Align bottom"><AlignVerticalBottomIcon /></Button>
                      </Table.Cell>
                      <Table.Cell className="zoomTools">
                        <Button icon="zoom out" onClick={() => handleZoomOut()} size="tiny" title="Zoom Out" disabled={zoomLevel <= 0.5} />
                        <Button icon="zoom in" onClick={() => handleZoomIn()} size="tiny" title="Zoom In" disabled={zoomLevel >= 2.0} />
                        <div>{t('page.printing.labelTemplates.zoomLevel', "Zoom level")}: {(zoomLevel * 100).toFixed(0)}%</div>
                      </Table.Cell>
                      <Table.Cell colSpan={3} className="ioTools">
                        <div style={{ display: "flex", flexDirection: "row", alignItems: "center", width: "100%" }}>
                          <div style={{ display: "flex", justifyContent: "start" }}>
                            <Popup
                              wide
                              content="Print the label"
                              trigger={
                                <Button size="mini" onClick={() => handlePrint()}>
                                  <Icon name="print" /> {t('page.printing.labelTemplates.button.print', "Print")}
                                </Button>
                              }
                            />
                          </div>
                          <div style={{ flex: "1", display: "flex", justifyContent: "end" }}>
                            <Popup
                              wide
                              content="Load another label or create a new one"
                              trigger={
                                <Button size="mini" onClick={() => handleLoad()}>
                                  <Icon name="folder open" color="blue" /> {t('page.printing.labelTemplates.button.load', "Load...")}
                                </Button>
                              }
                            />
                            <Popup
                              wide
                              content="Save your label design"
                              trigger={
                                <Button size="mini" primary onClick={e => setLabelSetNameModalIsOpen(true)} disabled={!isDirty}>
                                  <Icon name="save" /> {t('button.save', "Save")}
                                </Button>
                              }
                            />
                          </div>
                        </div>
                      </Table.Cell>
                    </Table.Row>
                  </Table.Body>
                </Table>
              </div>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.labelProperties', "Label Properties")}</label>
              <div className="labelProperties">
                <Table celled>
                  <Table.Body>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.labelTemplate', "Label Template")}:</b>
                      </Table.Cell>
                      <Table.Cell colSpan={5}>
                        <Dropdown fluid selection loading={labelTemplatesLoading} name="align" options={labelTemplateOptions} value={labelTemplate?.labelTemplateId || -1} onChange={handleLabelTemplateChange} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.width', "Width")}:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "60px" }}
                          name="width"
                          value={labelTemplate?.width || ""}
                          onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'width')}
                        />{" "}
                        in.
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.height', "Height")}:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "60px" }}
                          name="height"
                          value={labelTemplate?.height || ""}
                          onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'height')}
                        />{" "}
                        in.
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.dpi', "Dpi")}:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "60px" }}
                          name="dpi" value={labelTemplate?.dpi || ""}
                          onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'dpi')}
                        />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.margin', "Margin")}:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "120px" }}
                          placeholder="0 0 0 0"
                          name="margin"
                          value={labelTemplate?.margin || ""}
                          onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'margin')}
                        />
                      </Table.Cell>
                      <Table.Cell colSpan={4}>
                        <Popup
                          content={<p>Turn on to display the margins and bounding boxes in the rendered preview</p>}
                          trigger={<Checkbox
                            toggle
                            name="showBoundaries"
                            label="Display Boundaries"
                            checked={labelTemplate?.showBoundaries || false}
                            onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'showBoundaries')}
                          />}
                        />
                      </Table.Cell>
                    </Table.Row>
                    {(labelTemplate?.labelTemplateId === -1 || labelTemplateIsDirty) && (
                      <Table.Row>
                        <Table.Cell>
                          <b>{t('page.printing.labelTemplates.templateName', "Template Name")}:</b>
                        </Table.Cell>
                        <Table.Cell colSpan={5}>
                          <Input
                            icon="file"
                            style={{ width: "400px" }}
                            name="name"
                            placeholder="Enter a template name"
                            value={labelTemplate?.name || ""}
                            onChange={(e, control) => handleLabelTemplateValueChange(e, control, 'name')}
                            disabled={labelTemplate?.labelTemplateId !== -1}
                          />
                          <Button size="mini" onClick={handleSaveTemplate} style={{ height: "26px", padding: "8px 10px", marginLeft: "5px" }}>
                            <Icon name="save" /> {t('button.save', "Save")}
                          </Button>
                        </Table.Cell>
                      </Table.Row>
                    )}
                  </Table.Body>
                </Table>
              </div>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.itemProperties', "Item Properties")} - {selectedItem?.name}</label>
              <div className="itemProperties">
                <Table celled>
                  <Table.Body>
                    <Table.Row>
                      <Table.Cell colSpan={6}>
                        {selectedItem &&
                          <div className="itemProperties">
                            <span>X: <Input name="itemPropertyX" transparent value={selectedItem?.left.toFixed(0) || 0} onChange={handleSelectedItemLocationChange} style={{ padding: '2px', margin: '0', fontSize: '0.9em' }} /></span>
                            <span>Y: <Input name="itemPropertyY" transparent value={selectedItem?.top.toFixed(0) || 0} onChange={handleSelectedItemLocationChange} style={{ padding: '2px', margin: '0', fontSize: '0.9em' }} /></span>
                            <span>{t('page.printing.labelTemplates.width', "Width")}: <Input name="itemPropertyWidth" transparent onChange={handleSelectedItemLocationChange} value={Math.trunc(document.getElementById(selectedItem.id)?.clientWidth || 0)} style={{ padding: '2px', margin: '0', fontSize: '0.9em' }} /></span>
                            <span>{t('page.printing.labelTemplates.height', "Height")}: <Input name="itemPropertyHeight" transparent onChange={handleSelectedItemLocationChange} value={Math.trunc(document.getElementById(selectedItem.id)?.clientHeight || 0)} style={{ padding: '2px', margin: '0', fontSize: '0.9em' }} /></span>
                          </div>
                        }
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.align', "Align")}:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="align" options={alignOptions} value={selectedItemProperties?.align || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.font', "Font")}:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "150px" }}>
                        <Dropdown style={{ width: "150px" }} selection loading={fontsLoading} name="font" options={fontOptions} value={selectedItemProperties?.font || ""} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.rotate', "Rotate")}:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Dropdown style={{ width: "150px" }} selection name="rotate" options={rotateOptions} value={selectedItemProperties?.rotate || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.fontSize', "Font Size")}:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="fontSize" options={fontSizeOptions} value={selectedItemProperties?.fontSize || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.fontWeight', "Font Weight")}:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="fontWeight" options={fontWeightOptions} value={selectedItemProperties?.fontWeight || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.color', "Color")}:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="color" options={colorOptions} value={selectedItemProperties?.color || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>{t('page.printing.labelTemplates.textBarcodeValue', "Text/Barcode Value")}:</b>
                      </Table.Cell>
                      <Table.Cell colSpan={5}>
                        <Popup
                          wide='very'
                          hoverable
                          content={<p>The text to display (for a Custom text box) or override the value assigned to a barcode with static text or a part template value.
                            <br />
                            Part template values should be of the following format: <i><b>&#123;</b>partNumber<b>&#125;</b></i>
                            <br /><br />
                            Website URLs are also permitted: https://localhost:8090/inventory/<i><b>&#123;</b>partNumber<b>&#125;</b>:<b>&#123;</b>partId<b>&#125;</b></i>
                          </p>}
                          trigger={<Input
                            fluid
                            icon="code"
                            name="value"
                            placeholder="Automatic"
                            autoComplete="off"
                            value={selectedItemProperties?.value || ""}
                            disabled={!selectedItem?.acceptsValue || !selectedItem}
                            onChange={handleItemPropertyChange}
                          />}
                        />
                        <div style={{ fontSize: '0.8em', textAlign: 'right' }}>{t('page.printing.labelTemplates.length', "Length")}: {selectedItemProperties?.value?.length || 0}</div>
                      </Table.Cell>
                    </Table.Row>
                  </Table.Body>
                </Table>
              </div>
            </div>
          </div>
        </div>
        <div className="tools right">
          <div style={{ display: "flex", flexDirection: "column", flex: "1", alignItems: "start" }}>
            <div className="header" style={{ flex: "0" }}>
              <label>{t('page.printing.labelTemplates.partInformation', "Part Information")}</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="partNumber">{getChildrenByName('partNumber')}</DraggableBox>
              <DraggableBox name="partId">{getChildrenByName('partId')}</DraggableBox>
              <DraggableBox name="manufacturerPartNumber">{getChildrenByName('manufacturerPartNumber')}</DraggableBox>
              <DraggableBox name="manufacturer">{getChildrenByName('manufacturer')}</DraggableBox>
              <DraggableBox name="description" resize="both">{getChildrenByName('description')}</DraggableBox>
              <DraggableBox name="partType">{getChildrenByName('partType')}</DraggableBox>
              <DraggableBox name="partTypeId">{getChildrenByName('partTypeId')}</DraggableBox>
              <DraggableBox name="mountingType">{getChildrenByName('mountingType')}</DraggableBox>
              <DraggableBox name="packageType">{getChildrenByName('packageType')}</DraggableBox>
              <DraggableBox name="cost">{getChildrenByName('cost')}</DraggableBox>
              <DraggableBox name="keywords">{getChildrenByName('keywords')}</DraggableBox>
              <DraggableBox name="quantity">{getChildrenByName('quantity')}</DraggableBox>
              <DraggableBox name="digikeyPartNumber">{getChildrenByName('digikeyPartNumber')}</DraggableBox>
              <DraggableBox name="mouserPartNumber">{getChildrenByName('mouserPartNumber')}</DraggableBox>
              <DraggableBox name="arrowPartNumber">{getChildrenByName('arrowPartNumber')}</DraggableBox>
              <DraggableBox name="tmePartNumber">{getChildrenByName('tmePartNumber')}</DraggableBox>
              <DraggableBox name="location">{getChildrenByName('location')}</DraggableBox>
              <DraggableBox name="binNumber">{getChildrenByName('binNumber')}</DraggableBox>
              <DraggableBox name="binNumber2">{getChildrenByName('binNumber2')}</DraggableBox>
              <DraggableBox name="extensionValue1">{getChildrenByName('extensionValue1')}</DraggableBox>
              <DraggableBox name="extensionValue2">{getChildrenByName('extensionValue2')}</DraggableBox>
              <DraggableBox name="footprintName">{getChildrenByName('footprintName')}</DraggableBox>
              <DraggableBox name="symbolName">{getChildrenByName('symbolName')}</DraggableBox>
              <DraggableBox name="projectId">{getChildrenByName('projectId')}</DraggableBox>
            </div>
          </div>
        </div>
      </DndProvider>
    </div>
  );
}
