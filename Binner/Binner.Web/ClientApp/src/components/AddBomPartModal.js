import React, { useState, useEffect, useMemo } from "react";
import { Icon, Button, Form, Modal, Popup, TextArea, Header, Confirm } from "semantic-ui-react";
import PropTypes from "prop-types";
import PartsGrid from "./PartsGrid";
import NumberPicker from "./NumberPicker";
import debounce from "lodash.debounce";

export function AddBomPartModal(props) {
  AddBomPartModal.abortController = new AbortController();
	const defaultForm = { 
    keyword: "", 
    pcbId: null, 
    quantity: '1', 
    referenceId: '', 
    notes: '' 
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

	const pcbOptions = [{key: 0, text: 'None', value: 0}];
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
        part: selectedPart, 
				pcbId: form.pcbId,
				quantity: form.quantity,
				notes: form.notes,
				referenceId: form.referenceId,
        partName: selectedPart?.partNumber || form.keyword
			});
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
    <div>
			<Confirm
        className="confirm"
        header="Add Part"
        open={confirmAddPartIsOpen}
        onCancel={confirmAddPartClose}
        onConfirm={handleAddPart}
        content={<p>You have not selected a part from your inventory.<br/>Are you sure you want to add this part without associating it to a part in your inventory?<br/><br/><span className="small">Note: You will still be able to manage it's quantity if you choose to proceed, but it will not appear in your inventory.</span></p>}
      />
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>BOM Management</Modal.Header>
				<Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
					<Header style={{marginBottom: '2px'}}>Add Part</Header>
					<p>Add a part to your BOM, optionally associating it with a particular PCB.</p>
				</Modal.Description>
        <Modal.Content scrolling style={{paddingTop: '0'}}>
          <Form style={{ marginBottom: "10px", width: '100%' }}>
						<Form.Group>
							<Form.Field width={8}>
								<Popup
									wide
									content="Select the pcb you would like to add parts to. If you choose not to select a PCB, the part will be added to your BOM without PCB associations."
									trigger={
											<Form.Dropdown label='Select PCB' placeholder="Choose PCB" selection value={form.pcbId || ''} options={pcbOptions} onChange={handleChange} name='pcbId' />
									}
								/>
							</Form.Field>
							<Form.Field width={4}>
									<Popup
										wide
										content={<p>Enter the quantity of this part required to produce a single PCB.</p>}
										trigger={
											<Form.Field
												control={NumberPicker}
												label="Quantity"
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
									content={<p>Enter a custom referenceId you can use for identifying this part.<br/>Examples: <i>D1</i>, <i>C2</i>, <i>Safety Fuse</i></p>}
									trigger={<Form.Input label="ReferenceId(s)" placeholder="D1,D2" name="referenceId" value={form.referenceId || ''} onChange={handleChange} icon="hashtag" />}
								/>
							</Form.Field>
							<Form.Field width={4}>
								<Popup 
										wide
										content={<p>Enter any custom notes for this part.</p>}
										trigger={<Form.Field label="Notes" placeholder="Used for full bridge rectifier" type='text' control={TextArea} style={{height: '43px', minHeight: '43px', padding: '5px'}} name='notes' onChange={handleChange} value={form.notes || ''} />}
									/>								
							</Form.Field>
						</Form.Group>
            <Form.Field width={8}>
							<Popup 
								wide
								content={<p>Search for a part in your inventory</p>}
								trigger={<Form.Input label="Part Number" placeholder="LM358" name="keyword" value={form.keyword} onChange={handleChange} icon="microchip" />}
							/>
            </Form.Field>
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
            No matching results.
          </PartsGrid>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>Cancel</Button>
          <Button primary onClick={handleConfirmPartSelection}>
            <Icon name="plus" /> Add
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
