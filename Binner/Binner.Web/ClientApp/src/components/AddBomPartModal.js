import React, { useState, useEffect, useMemo } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Icon, Button, Form, Modal, Popup, TextArea, Header, Confirm } from "semantic-ui-react";
import PropTypes from "prop-types";
import PartsGrid from "./PartsGrid";
import NumberPicker from "./NumberPicker";
import debounce from "lodash.debounce";

export function AddBomPartModal(props) {
  const { t } = useTranslation();
  AddBomPartModal.abortController = new AbortController();
	const defaultForm = { 
    keyword: "", 
    pcbId: null, 
    quantity: '1', 
    referenceId: '', 
    notes: '',
    schematicReferenceId: '',
    customDescription: ''
  };
  const [isOpen, setIsOpen] = useState(false);
  const [addPartSearchResults, setAddPartSearchResults] = useState([]);
	const [pcbs, setPcbs] = useState([]);
  const [selectedPart, setSelectedPart] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState(defaultForm);
	const [confirmAddPartIsOpen, setConfirmAddPartIsOpen] = useState(false);

	const pcbOptions = [{key: 0, text: t('comp.addBomPartModal.none', "None"), value: 0}];
  if (pcbs && pcbs.length > 0) {
    for(let i = 0; i < pcbs.length; i++)
      pcbOptions.push({ key: i + 1, text: pcbs[i].name, description: pcbs[i].description, value: pcbs[i].pcbId});
  }

  const search = async (keyword) => {
    AddBomPartModal.abortController.abort(); // Cancel the previous request
    AddBomPartModal.abortController = new AbortController();

    setLoading(true);

    try {
      const response = await fetch(`part/search?keywords=${keyword}`, {
        signal: AddBomPartModal.abortController.signal
      });

      if (response.status === 200) {
        const data = await response.json();
        setAddPartSearchResults(data || []);
        setLoading(false);
      } else {
        setAddPartSearchResults([]);
        setLoading(false);
      }
    } catch (ex) {
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const searchDebounced = useMemo(() => debounce(search, 400), []);

  useEffect(() => {
    setIsOpen(props.isOpen);
		setForm(defaultForm);
		setAddPartSearchResults([]);
  }, [props.isOpen]);

	useEffect(() => {
    setPcbs(props.pcbs);
  }, [props.pcbs]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleAddPartsNextPage = (e) => {};

  const handleAddPartSelectPart = (e, part) => {
    if (selectedPart === part)
      setSelectedPart(null);
    else
      setSelectedPart(part);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
  };

	const updateNumberPicker = (e) => {
    setForm({ ...form, quantity: e.value.toString() });
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    switch (control.name) {
      case "keyword":
        if (control.value && control.value.length > 0) searchDebounced(control.value);
        break;
      default:
        break;
    }
    setForm({ ...form });
  };

  const handleAddPart = (e) => {
		confirmAddPartClose();
    if (props.onAdd) {
      props.onAdd(e, { 
        part: {...selectedPart}, 
				pcbId: form.pcbId,
				quantity: form.quantity,
				notes: form.notes,
				referenceId: form.referenceId,
        partName: selectedPart?.partNumber || form.keyword,
        schematicReferenceId: form.schematicReferenceId,
        customDescription: form.customDescription
			});
      setSelectedPart(null);
    } else {
      console.error("No onAdd handler defined!");
    }
  };

	const handleConfirmPartSelection = (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (!selectedPart) {
      setConfirmAddPartIsOpen(true);
    } else {
      handleAddPart(e);
    }
  };

  const confirmAddPartClose = () => {
    setConfirmAddPartIsOpen(false);
  };

  return (
    <div className="addBomPartModal">
			<Confirm
        className="confirm"
        header={t('comp.addBomPartModal.confirmHeader', "Add Part")}
        open={confirmAddPartIsOpen}
        onCancel={confirmAddPartClose}
        onConfirm={handleAddPart}
        content={<p>
          <Trans i18nKey="comp.addBomPartModal.confirmAddUnassociated" keyword={form.keyword}>
          You have not selected a part from your inventory.<br/>
          Are you sure you want to add this part <b>{{keyword: form.keyword}}</b> without associating it to a part in your inventory?<br/><br/>
          <span className="small">Note: You will still be able to manage it's quantity if you choose to proceed, but it will not appear in your inventory.</span>
          </Trans>
          </p>}            
      />
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.addBomPartModal.title', "BOM Management")}</Modal.Header>
				<Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
					<Header style={{marginBottom: '2px'}}>{t('comp.addBomPartModal.confirmHeader', "Add Part")}</Header>
					<p>{t('comp.addBomPartModal.description', "Add a part to your BOM, optionally associating it with a particular PCB.")}</p>
				</Modal.Description>
        <Modal.Content scrolling style={{paddingTop: '0'}}>
          <Form style={{ marginBottom: "10px", width: '100%' }}>
						<Form.Group>
							<Form.Field width={8}>
								<Popup
									wide
									content={t('comp.addBomPartModal.popup.selectPcb', "Select the pcb you would like to add parts to. If you choose not to select a PCB, the part will be added to your BOM without PCB associations.")}
									trigger={
											<Form.Dropdown label={t('comp.addBomPartModal.selectPcb', "Select PCB")} placeholder={t('comp.addBomPartModal.choosePcb', "Choose PCB")} selection value={form.pcbId || ''} options={pcbOptions} onChange={handleChange} name='pcbId' />
									}
								/>
							</Form.Field>
							<Form.Field width={4}>
									<Popup
										wide
										content={<p>{t('comp.addBomPartModal.popup.quantity', "Enter the quantity of this part required to produce a single PCB.")}</p>}
										trigger={
											<Form.Field
												control={NumberPicker}
												label={t('label.quantity', "Quantity")}
												placeholder="10"
												min={0}
												value={form.quantity || ""}
												onChange={updateNumberPicker}
												name="quantity"
												autoComplete="off"
											/>
										}
									/>
								</Form.Field>
						</Form.Group>
						<Form.Group>
							<Form.Field width={4}>
								<Popup 
									wide
									content={<p>
                    <Trans i18nKey="comp.addBomPartModal.popup.referenceIds">
                    Enter a custom Reference Id you can use for identifying this part.<br/>Examples: <i>Optoisolator1</i>, <i>Capacitor Array</i>
                    </Trans>
                  </p>}
									trigger={<Form.Input label={t('label.referenceIds', "Reference Id(s)")} placeholder="Optoisolator1" name="referenceId" value={form.referenceId || ''} onChange={handleChange} icon="hashtag" />}
								/>
							</Form.Field>
              <Form.Field width={4}>
								<Popup 
									wide
									content={<p>
                    <Trans i18nKey="comp.addBomPartModal.popup.schematicReferenceIds">
                    Enter one or more Schematic Reference Ids you can use for identifying this part on the PCB silkscreen.<br/>Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                    </Trans>
                  </p>}
									trigger={<Form.Input label={t('label.schematicReferenceIds', "Schematic Reference Id(s)")} placeholder="D1,D2" name="schematicReferenceId" value={form.schematicReferenceId || ''} onChange={handleChange} icon="hashtag" />}
								/>
							</Form.Field>
							<Form.Field width={4}>
								<Popup 
										wide
										content={<p>{t('comp.addBomPartModal.popup.notes', "Enter any custom notes for this part.")}</p>}
										trigger={<Form.Field label={t('label.notes', "Notes")} type='text' control={TextArea} style={{height: '43px', minHeight: '43px', padding: '5px'}} name='notes' onChange={handleChange} value={form.notes || ''} />}
									/>								
							</Form.Field>
						</Form.Group>
            <Form.Group>
              <Form.Field width={4}>
                <Popup 
                  wide
                  content={<p>{t('comp.addBomPartModal.popup.partNumber', "Search for a part in your inventory")}</p>}
                  trigger={<Form.Input label={t('label.partNumber', "Part Number")} placeholder="LM358" name="keyword" value={form.keyword} onChange={handleChange} icon="microchip" />}
                />
              </Form.Field>
              <Form.Field width={4}>
								<Popup 
										wide
										content={<p>{t('comp.addBomPartModal.popup.customDescription', "Enter your own custom description for this part.")}</p>}
										trigger={<Form.Field label={t('label.customDescription', "Custom Description")} type='text' control={TextArea} style={{height: '43px', minHeight: '43px', padding: '5px'}} name='customDescription' onChange={handleChange} value={form.customDescription || ''} />}
									/>								
							</Form.Field>
            </Form.Group>
          </Form>

          <PartsGrid
            parts={addPartSearchResults}
            page={page}
            totalPages={totalPages}
            loading={loading}
            loadPage={handleAddPartsNextPage}
            onPartClick={handleAddPartSelectPart}
            onPageSizeChange={handlePageSizeChange}
            selectedPart={selectedPart}
            columns="partNumber,quantity,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl"
            editable={false}
            visitable={false}
            name="partsGrid"
          >
            {t('message.noMatchingResults', "No matching results.")}
          </PartsGrid>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>Cancel</Button>
          <Button primary onClick={handleConfirmPartSelection}>
            <Icon name="plus" /> {t('button.add', "Add")}
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

AddBomPartModal.propTypes = {
  /** Event handler when adding a new part */
  onAdd: PropTypes.func.isRequired,
	/** The array of pcb's for the project */
  pcbs: PropTypes.array.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

AddBomPartModal.defaultProps = {
  isOpen: false
};
