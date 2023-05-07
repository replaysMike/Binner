import React, { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation, Trans } from "react-i18next";
import { HTML5Backend } from 'react-dnd-html5-backend'
import _ from "underscore";
import { FormHeader } from "../../components/FormHeader";
import { Popup, Dropdown, Table, Breadcrumb, Input, Button, Icon } from "semantic-ui-react";
import { LabelDropContainer } from "./LabelDropContainer";
import { DndProvider } from 'react-dnd'
import { DraggableBox } from './DraggableBox';
import { QRCodeIcon, DataMatrixIcon, Code128Icon, Code128NoTextIcon } from "./icons";
import { updateStateItem } from "../../common/reactHelpers";
import "./labelEditor.css";

export function LabelEditor(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
	const defaultItemProperties = {
		align: 0,
		fontSize: 1,
		fontWeight: 1,
		color: 0,
		width: "",
		height: "",
		value: ''
	};
	const [selectedItem, setSelectedItem] = useState(null);
	const [labelTemplate, setLabelTemplate] = useState(0);
	const [itemProperties, setItemProperties] = useState([]);
	const [selectedItemProperties, setSelectedItemProperties] = useState({});

	const labelTemplateOptions = [
		{ key: -1, text: "Custom", value: -1, printwidth: 1.875, printheight: 0.5625, printdpi: 300, printmargin: 0, printpadding: 0 },
		{ key: 0, text: "30277 (Dual 9/16\" x 3 7/16\")", value: 0, printwidth: 3.4375, printheight: 0.5625, printdpi: 300, printmargin: 0, printpadding: 0 },
		{ key: 1, text: "30346 (1/2\" x 1 7/8\")", value: 1, printwidth: 1.875, printheight: 0.5, printdpi: 300, printmargin: 0, printpadding: 0 },
	];
	const [labelTemplateWidth, setLabelTemplateWidth] = useState(labelTemplateOptions[0].printwidth);
	const [labelTemplateHeight, setLabelTemplateHeight] = useState(labelTemplateOptions[0].printheight);
	const [labelTemplateDpi, setLabelTemplateDpi] = useState(labelTemplateOptions[0].printdpi);
	const [labelTemplateMargin, setLabelTemplateMargin] = useState(labelTemplateOptions[0].printmargin);
	const [labelTemplatePadding, setLabelTemplatePadding] = useState(labelTemplateOptions[0].printpadding);
	const [labelTemplateNew, setLabelTemplateNew] = useState("");

	const alignOptions = [
		{ key: 0, text: "Center", value: 0 },
		{ key: 1, text: "Left", value: 1 },
		{ key: 2, text: "Right", value: 2 },
	];

	const rotateOptions = [
		{ key: 0, text: "0 Degrees", value: 0 },
		{ key: 1, text: "45 Degrees", value: 1 },
		{ key: 2, text: "90 Degrees", value: 2 },
		{ key: 3, text: "135 Degrees", value: 3 },
		{ key: 4, text: "180 Degrees", value: 4 },
		{ key: 5, text: "225 Degrees", value: 5 },
		{ key: 6, text: "270 Degrees", value: 6 },
		{ key: 7, text: "315 Degrees", value: 7 },
	];

	const fontSizeOptions = [
		{ key: 0, text: "12pt", value: 0 },
		{ key: 1, text: "14pt", value: 1 },
		{ key: 2, text: "16pt", value: 2 },
		{ key: 3, text: "18pt", value: 3 },
		{ key: 4, text: "24pt", value: 4 },
		{ key: 5, text: "32pt", value: 5 },
		{ key: 6, text: "48pt", value: 6 },
	];

	const fontWeightOptions = [
		{ key: 0, text: "Light", value: 0 },
		{ key: 1, text: "Normal", value: 1 },
		{ key: 2, text: "Bold", value: 2 },
		{ key: 3, text: "Heavy Bold", value: 3 },
	];

	const colorOptions = [
		{ key: 0, text: "Black", value: 0 },
		{ key: 1, text: "Blue", value: 1, color: 'blue' },
		{ key: 2, text: "Gray", value: 2, color: 'gray' },
		{ key: 3, text: "Green", value: 3, color: 'green' },
		{ key: 4, text: "Orange", value: 4, color: 'orange' },
		{ key: 5, text: "Purple", value: 5, color: 'purple' },
		{ key: 6, text: "Red", value: 6, color: 'red' },
		{ key: 7, text: "Yellow", value: 7, color: 'yellow' },
	];

	const convertInchesToPixels = (inches) => {
		return inches * 96;
	};

	const handleLabelTemplateChange = (e, control) => {
		setLabelTemplate(control.value);
		// resize the canvas
		const templateOption = _.find(labelTemplateOptions, i => i.value === control.value);
		setLabelTemplateWidth(templateOption.printwidth);
		setLabelTemplateHeight(templateOption.printheight);
	};

	const handleItemPropertyChange = (e, control) => {
		if (selectedItemProperties) {
			selectedItemProperties[control.name] = control.value;
			setSelectedItemProperties({...selectedItemProperties});
			const newItemProperties = updateStateItem(itemProperties, selectedItemProperties, "name", selectedItem.name);
			setItemProperties(newItemProperties);
		}
	};

	const handleSelectedItemChanged = (e, selectedItem) => {
		setSelectedItem(selectedItem);
		let itemProp = getItemProperties(selectedItem);
		if (itemProp === null) {
			itemProp = {...defaultItemProperties, name: selectedItem.name};
			const newItemProperties = [...itemProperties];
			newItemProperties.push(itemProp);
			setItemProperties(newItemProperties);
		}		
		setSelectedItemProperties(itemProp);
	};

	const getItemProperties = (item) => {
		let properties = _.find(itemProperties, i => i.name === item.name);
		if (properties) {
			return properties;
		}
		return null;
	};

	const getItemProperty = (item, name) => {
		const properties = getItemProperties(item);
		return properties[name];
	};

	console.log('labelTemplate', labelTemplate);

	return (
		<div className="labelEditor">
			<DndProvider backend={HTML5Backend}>
				<div className="tools left">
					<div style={{display: 'flex', flexDirection: 'column', flex: '1', alignItems: 'start'}}>
						<div className="header" style={{flex: '0'}}>
							<label>Barcode Labels</label>
						</div>
						<div className="wrapper" style={{flex: '1', padding: '0 20px'}}>
							<DraggableBox name="qrCode" resize="both" acceptsValue={true}><Popup wide content="QR Code" trigger={<QRCodeIcon />} /></DraggableBox>
							<DraggableBox name="dataMatrix2d" resize="both" acceptsValue={true}><Popup wide content="Data Matrix (2D) Barcode" trigger={<DataMatrixIcon />} /></DraggableBox>
							<DraggableBox name="code128" resize="both" acceptsValue={true}><Popup wide content="Code-128 Barcode (with text)" trigger={<Code128Icon style={{width: '160px'}} />} /></DraggableBox>
							<DraggableBox name="code128NoText" resize="both" acceptsValue={true}><Popup wide content="Code-128 Barcode (without text)" trigger={<Code128NoTextIcon style={{width: '160px'}} />} /></DraggableBox>
						</div>
						<div className="header" style={{flex: '0'}}>
							<label>Custom</label>
						</div>
						<div className="wrapper" style={{flex: '1', padding: '0 20px'}}>
							<DraggableBox name="text" acceptsValue={true} displaysValue={true}><Popup wide content="Custom text string" trigger={<span>Regular Text</span>} /></DraggableBox>
						</div>
					</div>
				</div>
				<div className="layout">
					<div style={{display: 'flex', flexDirection: 'column', flex: '1', alignItems: 'center'}}>
						<div className="header" style={{flex: '0'}}>
							<label>Label Editor</label>
						</div>
						<div className="wrapper" style={{flex: '1' }}>
							<LabelDropContainer 
								onSelectedItemChanged={handleSelectedItemChanged} 
								width={convertInchesToPixels(labelTemplateWidth)} 
								height={convertInchesToPixels(labelTemplateHeight)}
								margin={parseInt(labelTemplateMargin)}
								padding={parseInt(labelTemplatePadding)}
								itemProperties={itemProperties}
							/>
							<div style={{fontSize: '0.9em', fontWeight: '700', color: '#333'}}>{_.find(labelTemplateOptions, i => i.value === labelTemplate)?.text}</div>
							<div style={{fontSize: '0.7em', color: '#999'}}>Drag and drop components onto the label surface above</div>
						</div>
						<div className="header" style={{flex: '0'}}>
							<label>Label Properties</label>
							<div>
								<Table celled>
									<Table.Body>
										<Table.Row>
											<Table.Cell><b>Label Template:</b></Table.Cell>
											<Table.Cell colSpan={5}><Dropdown fluid selection name="align" options={labelTemplateOptions} value={labelTemplate || 0} onChange={handleLabelTemplateChange} /></Table.Cell>
										</Table.Row>
										<Table.Row>
											<Table.Cell><b>Width:</b></Table.Cell>
											<Table.Cell><Input style={{width: '60px'}} name="labelTemplateWidth" value={labelTemplateWidth} onChange={(e, control) => { setLabelTemplateWidth(control.value); setLabelTemplate(2); }} /> in.</Table.Cell>
											<Table.Cell><b>Height:</b></Table.Cell>
											<Table.Cell><Input style={{width: '60px'}} name="labelTemplateWidth" value={labelTemplateHeight} onChange={(e, control) => { setLabelTemplateHeight(control.value); setLabelTemplate(2); }} /> in.</Table.Cell>
											<Table.Cell><b>Dpi:</b></Table.Cell>
											<Table.Cell><Input style={{width: '60px'}} name="labelTemplateDpi" value={labelTemplateDpi} onChange={(e, control) => setLabelTemplateDpi(control.value)} /></Table.Cell>
										</Table.Row>
										<Table.Row>
											<Table.Cell><b>Margin:</b></Table.Cell>
											<Table.Cell><Input style={{width: '60px'}} name="labelTemplateMargin" value={labelTemplateMargin} onChange={(e, control) => setLabelTemplateMargin(control.value)} /></Table.Cell>
											<Table.Cell><b>Padding:</b></Table.Cell>
											<Table.Cell><Input style={{width: '60px'}} name="labelTemplatePadding" value={labelTemplatePadding} onChange={(e, control) => setLabelTemplatePadding(control.value)} /></Table.Cell>
											<Table.Cell colSpan={2}></Table.Cell>
										</Table.Row>
										{labelTemplate === -1 && <Table.Row>
											<Table.Cell><b>Template Name:</b></Table.Cell>
											<Table.Cell colSpan={5}>
												<Input icon="file" style={{ width: '400px'}} name="labelTemplateNew" placeholder="Enter a template name" value={labelTemplateNew} onChange={(e, control) => setLabelTemplateNew(control.value)} />
												<Button size="mini" style={{height: '26px', padding: '8px 10px', marginLeft: '5px' }}><Icon name="save" /> Save</Button>
											</Table.Cell>
										</Table.Row>}
									</Table.Body>
								</Table>
							</div>
						</div>
						<div className="header" style={{flex: '0'}}>
							<label>Item Properties - {selectedItem?.name}</label>
							<div>
								<Table celled>
									<Table.Body>
										<Table.Row>
											<Table.Cell><b>Align:</b></Table.Cell>
											<Table.Cell style={{width: '100px'}}><Dropdown selection name="align" options={alignOptions} value={selectedItemProperties?.align || 0} onChange={handleItemPropertyChange} /></Table.Cell>
											<Table.Cell><b>Rotate:</b></Table.Cell>
											<Table.Cell colSpan={3}><Dropdown style={{width: '150px'}} selection name="rotate" options={rotateOptions} value={selectedItemProperties?.rotate || 0} onChange={handleItemPropertyChange} /></Table.Cell>
										</Table.Row>
										<Table.Row>
											<Table.Cell><b>Font Size:</b></Table.Cell>
											<Table.Cell style={{width: '100px'}}><Dropdown selection name="fontSize" options={fontSizeOptions} value={selectedItemProperties?.fontSize || 0} onChange={handleItemPropertyChange} /></Table.Cell>
											<Table.Cell><b>Font Weight:</b></Table.Cell>
											<Table.Cell style={{width: '100px'}}><Dropdown selection name="fontWeight" options={fontWeightOptions} value={selectedItemProperties?.fontWeight || 0} onChange={handleItemPropertyChange} /></Table.Cell>
											<Table.Cell><b>Color:</b></Table.Cell>
											<Table.Cell style={{width: '100px'}}><Dropdown selection name="color" options={colorOptions} value={selectedItemProperties?.color || 0} onChange={handleItemPropertyChange} /></Table.Cell>
										</Table.Row>
										<Table.Row>
											<Table.Cell><b>Text Value:</b></Table.Cell>
											<Table.Cell colSpan={5}><Input style={{width: '400px'}} name="value" placeholder="Enter some text" value={selectedItemProperties?.value || ""} disabled={!(selectedItem?.acceptsValue)} onChange={handleItemPropertyChange} /></Table.Cell>
										</Table.Row>
									</Table.Body>
								</Table>
							</div>
						</div>
					</div>
				</div>
				<div className="tools right">
					<div style={{display: 'flex', flexDirection: 'column', flex: '1', alignItems: 'start'}}>
						<div className="header" style={{flex: '0'}}>
							<label>Part Information</label>
						</div>
						<div className="wrapper" style={{flex: '1', padding: '0 20px'}}>
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
							<DraggableBox name="quantity"><span style={{color: '#f00'}}>Quantity</span></DraggableBox>
							<DraggableBox name="location"><span style={{color: '#08c'}}>Location</span></DraggableBox>
							<DraggableBox name="binNumber"><span style={{color: '#08c'}}>Bin Number</span></DraggableBox>
							<DraggableBox name="binNumber2"><span style={{color: '#08c'}}>Bin Number2</span></DraggableBox>
						</div>
					</div>
				</div>
			</DndProvider>
		</div>
	);
};