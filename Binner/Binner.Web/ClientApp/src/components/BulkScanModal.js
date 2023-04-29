import React, { useState, useEffect, useRef } from "react";
import { useTranslation, Trans } from 'react-i18next';
import _ from "underscore";
import { Confirm, Icon, Button, Checkbox, Form, Modal, Popup, Table, Flag, Dimmer, Loader } from "semantic-ui-react";
import { getLocalData, setLocalData, removeLocalData } from "../common/storage";
import ProtectedInput from "./ProtectedInput";
import PropTypes from "prop-types";
import "./BulkScanModal.css";

export function BulkScanModal(props) {
  const { t } = useTranslation();
	const LocalStorageKey = "scannedPartsSerialized";
	const SettingsContainer = "BulkScanModal";
	const {onBarcodeLookup, onGetPartMetadata, onInventoryPartSearch} = props;

	const getFromLocalStorage = () => {
		const val = getLocalData(LocalStorageKey, { settingsName: SettingsContainer })
		// on load, force all items to be editable
		if (val) {
			for(var i = 0; i < val.length; i++) {
				val[i].isEditable = true;
			}
		}
		return val;
	};

	const saveToLocalStorage = (items) => {
		setLocalData(LocalStorageKey, items, { settingsName: SettingsContainer } );
	};

	const scannedPartsSerialized = getFromLocalStorage() || [];
	const [scannedParts, setScannedParts] = useState(scannedPartsSerialized);
	const scannedPartsRef = useRef(null);
	const [highlightScannedPart, setHighlightScannedPart] = useState(null);
	const [isDirty, setIsDirty] = useState(false);
	const [isOpen, setIsOpen] = useState(props.isOpen);
	const [bulkScanSaving, setBulkScanSaving] = useState(true);
	const [showBarcodeBeingScanned, setShowBarcodeBeingScanned] = useState(false);
	const [barcodeInput, setBarcodeInput] = useState(props.barcodeInput);
	const [autoIncrement, setAutoIncrement] = useState(true);
	const [rememberLocation, setRememberLocation] = useState(true);
	const [confirmClearIsOpen, setConfirmClearIsOpen] = useState(false);
	const [confirmClearContent, setConfirmClearContent] = useState(null);

	useEffect(() => {
		setIsOpen(props.isOpen);
		if (props.isOpen) {
			const scannedPartsSerialized = getFromLocalStorage() || [];
			setScannedParts(scannedPartsSerialized);
		}
	}, [props.isOpen]);

	useEffect(() => {
		setBulkScanSaving(props.isBulkScanSaving);
	}, [props.isBulkScanSaving]);

	useEffect(() => {
		const doAutoIncrement = (str) => {
			let parsedNum = parseInt(str);
			if (!isNaN(parsedNum)) {
				return parsedNum + 1;
			}
			return str;
		};
		const handleBarcodeInput = (barcodeparams) => {
			// process barcode input data
			const {cleanPartNumber, input} = barcodeparams;
	
			// bulk scan add part
			const lastPart = _.last(scannedParts);
			const scannedPart = {
				id: scannedParts.length + 1,
				partNumber: cleanPartNumber,
				quantity: parseInt(input.value.quantity || "1"),
				scannedQuantity: parseInt(input.value.quantity || "1"),
				location: (rememberLocation && lastPart && lastPart.location) || "",
				binNumber: (rememberLocation && lastPart && lastPart.binNumber) || "",
				binNumber2: (rememberLocation && lastPart && lastPart.binNumber2) || "",
				origin: (input.value.countryOfOrigin && input.value.countryOfOrigin.toLowerCase()) || "",
				description: input.value.description || "",
				barcode: input.correctedValue,
				isMetadataFound: false,
				isEditable: false,
				existsInInventory: false,
				dateAdded: new Date().getTime()
			};
			// stupid hack
			const els = document.getElementsByClassName('noMoreAnimation');
			for(var i = 0; i < els.length; i++)
				els[i].classList.remove('noMoreAnimation');

			if (scannedPart.binNumber2 && autoIncrement)
				scannedPart.binNumber2 = doAutoIncrement(scannedPart.binNumber2);

			const existingPartNumber = _.find(scannedParts, { partNumber: cleanPartNumber });
			if (existingPartNumber) {
				//existingPartNumber.quantity += existingPartNumber.scannedQuantity || 1;
				existingPartNumber.quantity = existingPartNumber.scannedQuantity || 1;
				existingPartNumber.isEditable = true;
				saveToLocalStorage(scannedParts);
				setShowBarcodeBeingScanned(false);
				setHighlightScannedPart(existingPartNumber);
				setIsDirty(!isDirty);
			} else {
				// fetch metadata on the barcode, don't await, do a background update
				const newScannedParts = [...scannedParts, scannedPart];
				saveToLocalStorage(newScannedParts);
				setShowBarcodeBeingScanned(false);
				setHighlightScannedPart(scannedPart);
				setScannedParts(newScannedParts);
				setIsDirty(!isDirty);
				scannedPartsRef.current = newScannedParts;
				
				onBarcodeLookup(scannedPart, async (partInfo) => {
					// barcode found
					// does it already exist in inventory?
					const localInventoryResponse = await onInventoryPartSearch(partInfo.basePartNumber);

					const newScannedParts = [...scannedPartsRef.current];
					const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === partInfo.manufacturerPartNumber || i.barcode === scannedPart.barcode);
					if (scannedPartIndex >= 0) {
						const scannedPart = newScannedParts[scannedPartIndex];
						if (localInventoryResponse.exists) {
							// part exists in inventory
							scannedPart.existsInInventory = localInventoryResponse.exists;
							scannedPart.description = localInventoryResponse.data[0].description;
							scannedPart.location = localInventoryResponse.data[0].location;
							scannedPart.binNumber = localInventoryResponse.data[0].binNumber;
							scannedPart.binNumber2 = localInventoryResponse.data[0].binNumber2;
							scannedPart.quantity = localInventoryResponse.data[0].quantity;
						} else {
							scannedPart.description = partInfo.description;
							if (partInfo.basePartNumber && partInfo.basePartNumber.length > 0)
								scannedPart.partNumber = partInfo.basePartNumber;
						}
						scannedPart.isMetadataFound = true;
						scannedPart.isEditable = true;
						newScannedParts[scannedPartIndex] = scannedPart;
						setScannedParts(newScannedParts);
						setIsDirty(!isDirty);
						saveToLocalStorage(newScannedParts);
					}
				}, (scannedPart) => {
					// no barcode info found, try searching the part number
					const includeInventorySearch = true;
					onGetPartMetadata(scannedPart.partNumber, scannedPart, includeInventorySearch).then((data) => {
						if (data.response.parts.length > 0) {
							const firstPart = data.response.parts[0];
							// console.log('adding part', firstPart);
							const newScannedParts = [...scannedPartsRef.current];
							const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === firstPart.manufacturerPartNumber || i.barcode === scannedPart.barcode);
							if (scannedPartIndex >= 0) {
								const scannedPart = newScannedParts[scannedPartIndex];
								scannedPart.isMetadataFound = true;
								scannedPart.isEditable = true;
								scannedPart.description = firstPart.description;
								if (firstPart.basePartNumber && firstPart.basePartNumber.length > 0)
									scannedPart.partNumber = firstPart.basePartNumber;
								newScannedParts[scannedPartIndex] = scannedPart;
								setScannedParts(newScannedParts);
								setIsDirty(!isDirty);
								saveToLocalStorage(newScannedParts);
							}
						} else {
							const newScannedParts = [...scannedPartsRef.current];
							const scannedPartIndex = _.findIndex(newScannedParts, i => i.partNumber === scannedPart.partNumber);
							if (scannedPartIndex >= 0) {
								const scannedPart = newScannedParts[scannedPartIndex];
								scannedPart.isEditable = true;
								setScannedParts(newScannedParts);
							}
						}
					});
				});
			}
		};
		setBarcodeInput(props.barcodeInput);
		if (props.barcodeInput) handleBarcodeInput(props.barcodeInput);
	}, [props.barcodeInput]);

  const handleScannedPartChange = (e, control, id) => {
		const newScannedParts = scannedParts.map((c, i) => {
			if (c.id === id) {
				c[control.name] = control.value;
			}
			return c;
		});
		setScannedParts(newScannedParts);
    setIsDirty(!isDirty);
  };

	const ensureNumeric = (e, name, part) => {
		const newScannedParts = scannedParts.map((c, i) => {
			if (c.id === part.id) {
				var val = parseInt(part[name]);
				if (isNaN(val)) c[name] = 0;
				else c[name] = val;
			}
			return c;
		});
		setScannedParts(newScannedParts);
    setIsDirty(!isDirty);
  };

  const deleteScannedPart = (e, scannedPart) => {
    e.preventDefault();
    e.stopPropagation();
    const scannedPartsExceptDeleted = _.without(scannedParts, _.findWhere(scannedParts, { partNumber: scannedPart.partNumber }));
		saveToLocalStorage(scannedPartsExceptDeleted);
		setScannedParts(scannedPartsExceptDeleted);
		setIsDirty(!isDirty);
  };

  const handleAddBulkScanRow = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setScannedParts([...scannedParts, {
			id: scannedParts.length + 1,
      basePartNumber: '',
      partNumber: '',
      quantity: 1,
      description: '',
      origin: '',
      location: '',
      binNumber: '',
      binNumber2: '',
			isMetadataFound: false,
			isEditable: true,
			existsInInventory: false,
			dateAdded: new Date().getTime(),
     }]);
		 setIsDirty(!isDirty);
  };

	const handleBulkScanClose = (e) => {
    if (props.onClose) props.onClose(e);
		setScannedParts([]);
		setIsDirty(!isDirty);
  };

  const handleBulkScanClear = () => {
		localStorage.removeItem(LocalStorageKey);
		removeLocalData(LocalStorageKey, { settingsName: SettingsContainer } );
		setScannedParts([]);
		setIsDirty(!isDirty);
		setConfirmClearIsOpen(false);
  };

	const handleRememberLocationChange = (e, control) => {
		setRememberLocation(control.checked);
		if (!control.checked) setAutoIncrement(false);
	};

	const handleAutoIncrementChange = (e, control) => {
		setAutoIncrement(control.checked);
		if (control.checked) setRememberLocation(true);
	};

	const handleOnSave = async (e) => {
		if (props.onSave) {
			const isSuccess = await props.onSave(e, scannedParts);
			if (isSuccess) {
				handleBulkScanClear();
			}
		}
	};

	const confirmClearOpen = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmClearIsOpen(true);
    setConfirmClearContent(
      <p>
        <Trans i18nKey="confirm.clearScan">
        Are you sure you want to clear your scanned results?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>        
      </p>
    );
  };

	const confirmClearClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmClearIsOpen(false);
  };

	const beginAnimation = (highlightScannedPart) => {
		const highlightElement = document.querySelectorAll(`[data-partnumber="${highlightScannedPart.partNumber}"]`);
		if (highlightElement.length > 0) {
			if (!highlightElement[0].classList.contains('noMoreAnimation')) {
				highlightElement[0].classList.add('lastScannedPart');
				highlightElement[0].classList.add('scannedPartAnimation');
				setTimeout(() => {
					highlightElement[0].classList.remove("scannedPartAnimation");
					highlightElement[0].classList.add("noMoreAnimation");
				}, 1000);
			}
		}
	};

	const renderScannedParts = (highlightScannedPart) => {
    if (highlightScannedPart) {
      // reset the css highlight animation
			if (highlightScannedPart.isEditable) {
				beginAnimation(highlightScannedPart);
			}
    }
    return (
			<Table compact celled striped size="small">
				<Table.Header>
					<Table.Row>
						<Table.HeaderCell></Table.HeaderCell>
						<Table.HeaderCell>{t('label.part', "Part")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.quantity', "Quantity")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.description', "Description")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.origin', "Origin")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.location', "Location")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.binNumber', "Bin Number")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.binNumber2', "Bin Number 2")}</Table.HeaderCell>
						<Table.HeaderCell></Table.HeaderCell>
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{_.sortBy(scannedParts, (num) => num).reverse().map((p, index) => (
						<Table.Row
							key={index}
							data-partnumber={p.partNumber}
							className={`${p.existsInInventory ? 'exists' : ''} `}
						>
							<Table.Cell textAlign="center" style={{verticalAlign: 'middle', width: '50px'}}>
								{!p.isEditable 
									? <Loader active inline />
									: p.existsInInventory 
										? <Popup content={<p>{t('message.scanPartExists', "Part exists in inventory. You can edit it's details or remove it from your scan.")}</p>} trigger={<Icon name="warning circle" color="red" size="big" />} />
										: p.isMetadataFound 
											? <Icon name="check circle" color="green" size="big" /> 
											: <Popup content={<p>{t('message.noPartMetadata', "No part metadata found!")}</p>} trigger={<Icon name="warning sign" color="yellow" size="big" />} />
								}									
							</Table.Cell>
							<Table.Cell collapsing>
								<ProtectedInput 
									name="partNumber" 
									value={p.partNumber || ''} 
									onChange={(e, c) => handleScannedPartChange(e, c, p.id)} 
									disabled={!p.isEditable}
								/>
							</Table.Cell>
							<Table.Cell collapsing>
								<ProtectedInput
									hideIcon
									value={p.quantity || '1'}
									onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
									name="quantity"
									disabled={!p.isEditable}
									style={{width: '75px'}}
									onBlur={e => ensureNumeric(e, "quantity", p)}
								/>
							</Table.Cell>
							<Table.Cell collapsing>
								<Popup 
									wide='very'
									hoverable
									content={p.description || ''}
									trigger={<ProtectedInput 
										name="description" 
										value={p.description || ''} 
										onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
										disabled={!p.isEditable}
										/>
									}
								/>									
							</Table.Cell>
							<Table.Cell collapsing textAlign="center" verticalAlign="middle">
								<Flag name={p.origin || ""} />
							</Table.Cell>
							<Table.Cell collapsing>
								<ProtectedInput
									hideIcon
									placeholder={t('page.inventory.placeholder.location', "Home lab")}
									value={p.location || ''}
									onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
									name="location"
									disabled={!p.isEditable}
									style={{width: '120px'}}
								/>
							</Table.Cell>
							<Table.Cell collapsing>
								<ProtectedInput
									hideIcon
									placeholder=""
									value={p.binNumber || ''}
									onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
									name="binNumber"
									disabled={!p.isEditable}
									style={{width: '120px'}}
								/>
							</Table.Cell>
							<Table.Cell collapsing>
								<ProtectedInput
									hideIcon
									placeholder=""
									value={p.binNumber2 || ''}
									onChange={(e, c) => handleScannedPartChange(e, c, p.id)}
									name="binNumber2"
									disabled={!p.isEditable}
									style={{width: '120px'}}
								/>
							</Table.Cell>
							<Table.Cell collapsing textAlign="center" verticalAlign="middle">
								<Button type="button" circular size="mini" icon="delete" title="Delete" onClick={(e) => deleteScannedPart(e, p)} />
							</Table.Cell>
						</Table.Row>
					))}
				</Table.Body>
			</Table>
    );
  };

	return (
		<>
		<Confirm
			className="confirm"
			header={t('confirm.header.clearScan', "Clear Scanned Parts")}
			open={confirmClearIsOpen}
			onCancel={confirmClearClose}
			onConfirm={handleBulkScanClear}
			content={confirmClearContent}
		/>
		<Modal centered open={isOpen} onClose={handleBulkScanClose} dimmer="blurring">
			<Modal.Header>{t('page.inventory.bulkScan', "Bulk Scan")}</Modal.Header>
			<Dimmer.Dimmable as={Modal.Content} scrolling>
				<Dimmer active={bulkScanSaving} inverted><Loader /></Dimmer>
				<div style={{ width: "200px", height: "100px", margin: "auto" }}>
					<div className="anim-box">
						<div className="scanner animated" />
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-md"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-md"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-md"></div>
						<div className="anim-item anim-item-md"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-md"></div>
						<div className="anim-item anim-item-lg"></div>
						<div className="anim-item anim-item-sm"></div>
						<div className="anim-item anim-item-md"></div>
					</div>
				</div>
				<div style={{ textAlign: "center" }}>
					<Form>
						<p>{t('page.inventory.startScanning', "Start scanning parts...")}</p>
						<div style={{textAlign: 'right', height: '35px', width: '100%', marginBottom: '2px'}}>
							<Form.Group style={{justifyContent: 'end'}}>
								<Form.Field style={{margin: 'auto 0'}}><Popup hoverable content={<p>{t('comp.bulkScanModal.popup.autoIncrement', "Auto increment of Bin Number 2")}</p>} trigger={<Checkbox toggle label={t('comp.bulkScanModal.autoIncrement', "Auto Increment")} checked={autoIncrement} onChange={handleAutoIncrementChange} style={{scale: '0.8' }} />}/></Form.Field>
								<Form.Field style={{margin: 'auto 0'}}><Popup content={<p>{t('comp.bulkScanModal.popup.rememberLocation', "Repeat the location of each added part")}</p>} trigger={<Checkbox toggle label={t('comp.bulkScanModal.rememberLocation', "Remember Location")} checked={rememberLocation} onChange={handleRememberLocationChange} style={{scale: '0.8' }} />}/></Form.Field>
								<Form.Field>
								<Button size='mini' onClick={handleAddBulkScanRow}><Icon name="plus" color="green" /> {t('button.manualAdd', "Manual Add")}</Button>
							</Form.Field>
							</Form.Group>
						</div>
						{isOpen && renderScannedParts(highlightScannedPart)}
					</Form>
				</div>
			</Dimmer.Dimmable>
			<Modal.Actions>
				<Button onClick={confirmClearOpen} disabled={scannedParts.length === 0}>{t('button.clear', "Clear")}</Button>
				<Button onClick={handleBulkScanClose}>{t('button.cancel', "Cancel")}</Button>
				<Button primary onClick={handleOnSave} disabled={bulkScanSaving}>
					{t('button.save', "Save")}
				</Button>
			</Modal.Actions>
		</Modal>
		</>);
};

BulkScanModal.propTypes = {
	/** Event handler to call when saving scanned parts */
	onSave: PropTypes.func.isRequired,
	/** Event handler to call when needing part metadata */
	onBarcodeLookup: PropTypes.func.isRequired,
	/** Event handler to call when needing part info */
	onGetPartMetadata: PropTypes.func.isRequired,
	/** Event handler to call when needing local inventory lookup */
	onInventoryPartSearch: PropTypes.func.isRequired,
	/** Event handler when a part is scanned */
  onAdd: PropTypes.func,
	/** Event handler when a part is removed */
	onRemove: PropTypes.func,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
	/** Set the barcode input when received */
	barcodeInput: PropTypes.object,
  /** Set this to true to open model */
  isOpen: PropTypes.bool,
	isBulkScanSaving: PropTypes.bool,
};

BulkScanModal.defaultProps = {
  isOpen: false
};
