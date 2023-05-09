import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { HTML5Backend } from "react-dnd-html5-backend";
import _ from "underscore";
import { FormHeader } from "../../components/FormHeader";
import { Popup, Dropdown, Table, Breadcrumb, Input, Button, Icon, Image, Checkbox } from "semantic-ui-react";
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
import { QRCodeIcon, DataMatrixIcon, AztecCodeIcon, Code128NoTextIcon, PDF417Icon } from "./icons";
import { DEFAULT_FONT } from '../../common/Types';
import { HandleBinaryResponse } from "../../common/handleResponse.js";
import { fetchApi } from "../../common/fetchApi";
import { getImagesToken } from "../../common/authentication";
import { updateStateItem } from "../../common/reactHelpers";
import "./labelEditor.css";

export function LabelEditor(props) {
	const PreviewDebounceTimeMs = 500;
  const { t } = useTranslation();
  const navigate = useNavigate();
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
  const [boxes, setBoxes] = useState([]);
  const [selectedItem, setSelectedItem] = useState(null);
  const [itemProperties, setItemProperties] = useState([]);
  const [selectedItemProperties, setSelectedItemProperties] = useState({});
	const [imgBase64, setImgBase64] = useState("");
	const [clearSelectedItem, setClearSelectedItem] = useState(null);
	const [fontOptions, setFontOptions] = useState([
    {
      key: 1,
      value: 0,
      text: "Loading"
    }
  ]);
	const [labelTemplates, setLabelTemplates] = useState([]);
	const [labelTemplateOptions, setLabelTemplateOptions] = useState([]);
  
	const [labelTemplate, setLabelTemplate] = useState({});

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

  const handleLabelTemplateChange = (e, control) => {
		const newValue = {...labelTemplate, id: control.value };
    // resize the canvas
    const templateOption = _.find(labelTemplates, (i) => i.id === control.value);
		newValue["width"] = templateOption.width;
		newValue["height"] = templateOption.height;
		newValue["dpi"] = templateOption.dpi;
		newValue["margin"] = templateOption.margin;
		newValue["showBoundaries"] = templateOption.showBoundaries;
    setLabelTemplate(newValue);
		previewDebounced(boxes, newValue);
  };

  const handleItemPropertyChange = (e, control) => {
    if (selectedItemProperties) {
      selectedItemProperties[control.name] = control.value;
      setSelectedItemProperties({ ...selectedItemProperties });
      const newItemProperties = updateStateItem(itemProperties, selectedItemProperties, "name", selectedItem.name);
      setItemProperties(newItemProperties);
    }
		previewDebounced(boxes, labelTemplate);
  };

  const handleSelectedItemChanged = (e, selectedItem) => {
		if (selectedItem === null) {
			setSelectedItem(null);
			setSelectedItemProperties(null);
			return;
		}
    setSelectedItem(selectedItem);
    let itemProp = getItemProperties(selectedItem);
    if (itemProp === null) {
      itemProp = { ...defaultItemProperties, name: selectedItem.name };
      const newItemProperties = [...itemProperties];
      newItemProperties.push(itemProp);
      setItemProperties(newItemProperties);
    }
    setSelectedItemProperties(itemProp);
		previewDebounced(boxes, labelTemplate);
  };

  const getItemProperties = (item) => {
    let properties = _.find(itemProperties, (i) => i.name === item.name);
    if (properties) {
      return properties;
    }
    return null;
  };

  const getItemProperty = (item, name) => {
    const properties = getItemProperties(item);
    return properties[name];
  };

	const handleClearSelectedItem = () => {
		setClearSelectedItem(Math.random());
	};

  const handleOnDrop = (box) => {
    console.log("Drop received", box);
		const newBoxes = [...boxes];
		newBoxes.push({...box});
		setBoxes(newBoxes);
  };

  const handleOnMove = (box) => {
    console.log("Move received", box);
    try {
      setBoxes(updateStateItem(boxes, box, "id", box.id));
    } catch {}
  };

  const handleOnRemove = (box) => {
    try {
      setBoxes(_.filter(boxes, (i) => i.id !== box.id));
    } catch {}
  };

	useEffect(() => {
		const getLabelTemplates = async () => {
			await fetchApi("api/print/templates", {
				method: "GET",
				headers: {
					"Content-Type": "application/json",
				},
			}).then((response) => {
				const { data } = response;

				const labelTemplates = [
					{ name: "Custom", id: -1, width: 1.875, height: 0.5625, dpi: 300, margin: "0", showBoundaries: false },
					{ name: '30277 (Dual 9/16" x 3 7/16")', id: 0, width: 3.4375, height: 0.5625, dpi: 300, margin: "0", showBoundaries: false },
					{ name: '30346 (1/2" x 1 7/8")', id: 1, width: 1.875, height: 0.5, dpi: 300, margin: "0", showBoundaries: false }
				];
				for(let i = 0; i < data.length; i++)
					labelTemplates.push(data[i]);
				
				setLabelTemplates(data);
				setLabelTemplateOptions(labelTemplates.map((item, key) => ({ key, text: item.name, value: item.id })));
				setLabelTemplate({
					width: labelTemplates[1].width,
					height: labelTemplates[1].height,
					dpi: labelTemplates[1].dpi,
					margin: labelTemplates[1].margin,
					showBoundaries: labelTemplates[1].showBoundaries,
					id: labelTemplates[1].id,
					new: "",
				});
			});
		};

    const loadFonts = async () => {
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
    });
    };
    
    loadFonts();
		getLabelTemplates();
  }, []);  

  const handlePreview = async (boxes, labelTemplate) => {
		const request = {
			token: getImagesToken(),
			label: {...labelTemplate, name: labelTemplate.new, templateId: labelTemplate.id },
			boxes: boxes.map((box) => ({
				acceptsValue: box.acceptsValue,
				displaysValue: box.displaysValue,
				id: box.id,
				left: Math.trunc(box.left),
				name: box.name,
				resize: box.resize,
				top: Math.trunc(box.top),
				properties: getItemProperties(box) || defaultItemProperties,
				width: Math.trunc(document.getElementById(box.id)?.getBoundingClientRect().width) || 0,
				height: Math.trunc(document.getElementById(box.id)?.getBoundingClientRect().height) || 0,
			})),
			generateImageOnly: true
		};

		await fetchApi("api/print/beta", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      HandleBinaryResponse(response.responseObject).then((b) => {
        const base64 = arrayBufferToBase64(b);
        setImgBase64(base64); 
      });
    });
	};

	const previewDebounced = useMemo(() => debounce(handlePreview, PreviewDebounceTimeMs), [boxes, labelTemplate]);

	const arrayBufferToBase64 = (buffer) => {
    let binary = "";
    let bytes = [].slice.call(new Uint8Array(buffer));

    bytes.forEach((b) => (binary += String.fromCharCode(b)));

    return btoa(binary);
  };

	const handleAlignSelected = (alignTo) => {
		if (!selectedItem) return;

		const box = _.find(boxes, i => i.id === selectedItem.id);
		const id = box.id;
		const el = document.getElementById(id);
		const margins = getMargins(labelTemplate.margin);
		console.log('margins', margins);
		// margins: top right bottom left
		switch(alignTo){
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
				console.log('height', el.offsetHeight);
				box.top = (((convertInchesToPixels(labelTemplate.height) - margins[0]) / 2.0) - (el.offsetHeight / 2.0));
				break;
			case 'bottom':
				box.top = (convertInchesToPixels(labelTemplate.height) - margins[2] - el.offsetHeight);
				break;
		}
		setSelectedItem(box);
		const newBoxes = [...boxes];
		setBoxes(newBoxes);
		previewDebounced(newBoxes, labelTemplate);
	};

	const getMargins = (margin) => {
		const margins = [0, 0, 0, 0];
		let marginDef = margin?.split(' ') || [];
	
		for(let i = 0; i < marginDef.length; i++)
			margins[i] = parseInt(marginDef[i]);
	
		if (marginDef.length === 1) {
			margins[1] = margins[2] = margins[3] = margins[0];
		} else if (marginDef.length === 2) {
			margins[2] = margins[0];
			margins[3] = margins[1];
		}
		return margins;
	};

  const handleSave = () => {};

  return (
    <div className="labelEditor">
      <DndProvider backend={HTML5Backend}>
        <div className="tools left">
          <div style={{ display: "flex", flexDirection: "column", flex: "1", alignItems: "start" }}>
            <div className="header" style={{ flex: "0" }}>
              <label>Barcode Labels</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="qrCode" resize="both" acceptsValue={true}>
                <Popup wide content="QR Code" trigger={<QRCodeIcon />} />
              </DraggableBox>
              <DraggableBox name="dataMatrix2d" resize="both" acceptsValue={true}>
                <Popup wide content="Data Matrix (2D) Barcode" trigger={<DataMatrixIcon />} />
              </DraggableBox>
							<DraggableBox name="aztecCode" resize="both" acceptsValue={true}>
                <Popup wide content="Aztec Code" trigger={<AztecCodeIcon />} />
              </DraggableBox>
              <DraggableBox name="code128" resize="both" acceptsValue={true}>
                <Popup wide content="Code-128 Barcode" trigger={<Code128NoTextIcon style={{ width: "160px" }} />} />
              </DraggableBox>
							<DraggableBox name="pdf417" resize="both" acceptsValue={true}>
                <Popup wide content="PDF417 Barcode" trigger={<PDF417Icon style={{ width: "160px" }} />} />
              </DraggableBox>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>Custom</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="text" acceptsValue={true} displaysValue={true}>
                <Popup wide content="Custom text string" trigger={<span>Regular Text</span>} />
              </DraggableBox>
            </div>
						<div className="header" style={{ flex: "0" }}>
              <label>Preview</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
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
            </div>
          </div>
        </div>
        <div className="layout">
          <div style={{ display: "flex", flexDirection: "column", flex: "1", alignItems: "center" }}>
            <div className="header" style={{ flex: "0" }}>
              <label>Label Editor</label>
            </div>
            <div className="wrapper" style={{ flex: "1" }} onClick={handleClearSelectedItem}>
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
            <div className="header" style={{ flex: "0" }}>
              <label>Manage</label>
              <div>
                <Table celled>
                  <Table.Body>
                    <Table.Row>
											<Table.Cell colSpan={3}>
												<Button icon onClick={() => handleAlignSelected('left')} disabled={!selectedItem} size="tiny" title="Align left"><AlignHorizontalLeft /></Button>
												<Button icon onClick={() => handleAlignSelected('center')} disabled={!selectedItem} size="tiny" title="Align center"><AlignHorizontalCenter /></Button>
												<Button icon onClick={() => handleAlignSelected('right')} disabled={!selectedItem} size="tiny" title="Align right"><AlignHorizontalRight /></Button>
												<Button icon onClick={() => handleAlignSelected('top')} disabled={!selectedItem} size="tiny" title="Align top"><AlignVerticalTopIcon /></Button>
												<Button icon onClick={() => handleAlignSelected('middle')} disabled={!selectedItem} size="tiny" title="Align middle"><AlignVerticalCenterIcon /></Button>
												<Button icon onClick={() => handleAlignSelected('bottom')} disabled={!selectedItem} size="tiny" title="Align bottom"><AlignVerticalBottomIcon /></Button>
											</Table.Cell>
                      <Table.Cell colSpan={3}>
                        <div style={{ display: "flex", flexDirection: "row", alignItems: "center", width: "100%" }}>
                          <div style={{ flex: "1", display: "flex", justifyContent: "start" }}>
                            <Popup
                              wide
                              content="Preview your label with content"
                              trigger={
                                <Button size="mini" secondary onClick={() => handlePreview(boxes, labelTemplate)}>
                                  <Icon name="eye" color="blue" /> Preview
                                </Button>
                              }
                            />
                          </div>
                          <div style={{ flex: "1", display: "flex", justifyContent: "end" }}>
                            <Popup
                              wide
                              content="Save your label design"
                              trigger={
                                <Button size="mini" primary onClick={handleSave}>
                                  <Icon name="save" /> Save
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
              <label>Label Properties</label>
              <div>
                <Table celled>
                  <Table.Body>
                    <Table.Row>
                      <Table.Cell>
                        <b>Label Template:</b>
                      </Table.Cell>
                      <Table.Cell colSpan={5}>
                        <Dropdown fluid selection name="align" options={labelTemplateOptions} value={labelTemplate?.id || 0} onChange={handleLabelTemplateChange} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>Width:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "60px" }}
                          name="labelTemplateWidth"
                          value={labelTemplate?.width || ""}
                          onChange={(e, control) => {
														const value = {...labelTemplate, width: control.value};
                            setLabelTemplate(value);
														previewDebounced(boxes, value);
                          }}
                        />{" "}
                        in.
                      </Table.Cell>
                      <Table.Cell>
                        <b>Height:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input
                          style={{ width: "60px" }}
                          name="labelTemplateWidth"
                          value={labelTemplate?.height || ""}
                          onChange={(e, control) => {
                            const value = {...labelTemplate, height: control.value};
														setLabelTemplate(value);
														previewDebounced(boxes, value);
                          }}
                        />{" "}
                        in.
                      </Table.Cell>
                      <Table.Cell>
                        <b>Dpi:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input style={{ width: "60px" }} name="labelTemplateDpi" value={labelTemplate?.dpi || ""} onChange={(e, control) => { const value = {...labelTemplate, dpi: control.value}; setLabelTemplate(value); previewDebounced(boxes, value); }} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>Margin:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Input style={{ width: "120px" }} placeholder="0 0 0 0" name="labelTemplateMargin" value={labelTemplate?.margin || ""} onChange={(e, control) => { const value = {...labelTemplate, margin: control.value}; setLabelTemplate(value); previewDebounced(boxes, value); }} />
                      </Table.Cell>
                      <Table.Cell colSpan={4}>
												<Checkbox toggle name="showBoundaries" label="Display Boundaries" checked={labelTemplate?.showBoundaries || false} onChange={(e, control) => { const value = {...labelTemplate, showBoundaries: control.checked}; setLabelTemplate(value); previewDebounced(boxes, value); }} />
											</Table.Cell>
                    </Table.Row>
                    {labelTemplate?.id === -1 && (
                      <Table.Row>
                        <Table.Cell>
                          <b>Template Name:</b>
                        </Table.Cell>
                        <Table.Cell colSpan={5}>
                          <Input
                            icon="file"
                            style={{ width: "400px" }}
                            name="labelTemplateNew"
                            placeholder="Enter a template name"
                            value={labelTemplate?.new || ""}
                            onChange={(e, control) => setLabelTemplate({...labelTemplate, new: control.value})}
                          />
                          <Button size="mini" style={{ height: "26px", padding: "8px 10px", marginLeft: "5px" }}>
                            <Icon name="save" /> Save
                          </Button>
                        </Table.Cell>
                      </Table.Row>
                    )}
                  </Table.Body>
                </Table>
              </div>
            </div>
            <div className="header" style={{ flex: "0" }}>
              <label>Item Properties - {selectedItem?.name}</label>
              <div>
                <Table celled>
                  <Table.Body>
										<Table.Row>
											<Table.Cell colSpan={6}>
												{selectedItem && 
												<div className="itemProperties">
													<span>X: {selectedItem.left}</span>
													<span>Y: {selectedItem.top}</span>
													<span>Width: {Math.trunc(document.getElementById(selectedItem.id)?.getBoundingClientRect().width)}</span>
													<span>Height: {Math.trunc(document.getElementById(selectedItem.id)?.getBoundingClientRect().height)}</span>
												</div>
												}
											</Table.Cell>
										</Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>Align:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="align" options={alignOptions} value={selectedItemProperties?.align || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
											<Table.Cell>
                        <b>Font:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "150px" }}>
                        <Dropdown style={{ width: "150px" }} selection name="font" options={fontOptions} value={selectedItemProperties?.font || ""} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>Rotate:</b>
                      </Table.Cell>
                      <Table.Cell>
                        <Dropdown style={{ width: "150px" }} selection name="rotate" options={rotateOptions} value={selectedItemProperties?.rotate || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>Font Size:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="fontSize" options={fontSizeOptions} value={selectedItemProperties?.fontSize || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>Font Weight:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="fontWeight" options={fontWeightOptions} value={selectedItemProperties?.fontWeight || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                      <Table.Cell>
                        <b>Color:</b>
                      </Table.Cell>
                      <Table.Cell style={{ width: "100px" }}>
                        <Dropdown selection name="color" options={colorOptions} value={selectedItemProperties?.color || 0} onChange={handleItemPropertyChange} disabled={!selectedItem} />
                      </Table.Cell>
                    </Table.Row>
                    <Table.Row>
                      <Table.Cell>
                        <b>Text Value:</b>
                      </Table.Cell>
                      <Table.Cell colSpan={5}>
                        <Input
                          style={{ width: "400px" }}
                          name="value"
                          placeholder="Enter some text"
                          value={selectedItemProperties?.value || ""}
                          disabled={!selectedItem?.acceptsValue || !selectedItem}
                          onChange={handleItemPropertyChange}
                        />
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
              <label>Part Information</label>
            </div>
            <div className="wrapper" style={{ flex: "1", padding: "0 20px" }}>
              <DraggableBox name="partNumber">Part Number</DraggableBox>
              <DraggableBox name="manufacturerPartNumber">Mfr Part Number</DraggableBox>
              <DraggableBox name="manufacturer">Manufacturer</DraggableBox>
              <DraggableBox name="description">Description</DraggableBox>
              <DraggableBox name="partType">Part Type</DraggableBox>
              <DraggableBox name="mountingType">Mounting Type</DraggableBox>
              <DraggableBox name="packageType">Package Type</DraggableBox>
              <DraggableBox name="cost">Cost</DraggableBox>
              <DraggableBox name="keywords">Keywords</DraggableBox>
              <DraggableBox name="image">Image</DraggableBox>
              <DraggableBox name="quantity">Quantity</DraggableBox>
              <DraggableBox name="digikeyPartNumber">DigiKey P/N</DraggableBox>
              <DraggableBox name="mouserPartNumber">Mouser P/N</DraggableBox>
              <DraggableBox name="arrowPartNumber">Arrow P/N</DraggableBox>
              <DraggableBox name="location">Location</DraggableBox>
              <DraggableBox name="binNumber">Bin Number</DraggableBox>
              <DraggableBox name="binNumber2">Bin Number2</DraggableBox>
            </div>
          </div>
        </div>
      </DndProvider>
    </div>
  );
}
