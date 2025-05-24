import React, { useState, useEffect, useMemo } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Icon, Button, Form, Modal, Popup, TextArea, Header, Confirm } from "semantic-ui-react";
import PropTypes from "prop-types";
import { fetchApi } from "../../common/fetchApi";
import PartsGrid2Memoized from "../PartsGrid2Memoized";
import NumberPicker from "../NumberPicker";
import debounce from "lodash.debounce";
import _ from "underscore";

/**
 * Add a part to a BOM project
 */
export function AddBomPartModal({ isOpen = false, ...rest }) {
  const { t } = useTranslation();
  AddBomPartModal.abortController = new AbortController();
  const defaultForm = {
    keyword: "",
    pcbId: rest.defaultPcb?.pcbId,
    quantity: '1',
    referenceId: '',
    notes: '',
    schematicReferenceId: '',
    customDescription: ''
  };
  const [form, setForm] = useState(defaultForm);
  const [_isOpen, setIsOpen] = useState(false);
  const [addPartSearchResults, setAddPartSearchResults] = useState([]);
  const [projectPartIds, setProjectPartIds] = useState(_.filter(rest.parts, i => form.pcbId > 0 ? i.pcbId === form.pcbId : true).map(i => (i.partId)) || []);
  const [pcbs, setPcbs] = useState([]);
  const [selectedPart, setSelectedPart] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [confirmAddPartIsOpen, setConfirmAddPartIsOpen] = useState(false);
  const [filterBy, setFilterBy] = useState([]);
  const [filterByValue, setFilterByValue] = useState([]);
  const [sortBy, setSortBy] = useState("DateCreatedUtc");
  const [sortDirection, setSortDirection] = useState("Descending");

  const pcbOptions = [{ key: 0, text: t('comp.addBomPartModal.none', "None"), value: 0 }];
  if (pcbs && pcbs.length > 0) {
    for (let i = 0; i < pcbs.length; i++)
      pcbOptions.push({ key: i + 1, text: pcbs[i].name, description: pcbs[i].description, value: pcbs[i].pcbId });
  }

  const loadParts = async (page, reset = false, by = filterBy, byValue = filterByValue, results = pageSize, orderBy = sortBy, orderDirection = sortDirection, keyword = null) => {
    setLoading(true);  
    const response = await fetchApi(
        `/api/part/list?orderBy=${orderBy || ""}&direction=${orderDirection || ""}&results=${results}&page=${page}&keyword=${keyword || ""}&by=${by?.join(',')}&value=${byValue?.join(',')}`
      );
    const { data } = response;
    const pageOfData = data.items;
    let newData = [];
    if (pageOfData) {
      if (reset) 
        newData = [...pageOfData];
      else 
        newData = [...addPartSearchResults, ...pageOfData];
    }
    setAddPartSearchResults(newData);
    setPage(page);
    setTotalPages(data.totalPages);
    setTotalRecords(data.totalItems);
    setLoading(false);
    return response;
  };

  const search = async (keyword) => {
    AddBomPartModal.abortController.abort(); // Cancel the previous request
    AddBomPartModal.abortController = new AbortController();

    setLoading(true);
    setPage(1);

    try {
      const response = await fetchApi(`/api/part/search?keywords=${encodeURIComponent(keyword.trim())}`, {
        signal: AddBomPartModal.abortController.signal
      }).catch(() => {
        setLoading(false);
        setAddPartSearchResults([]);
      });

      if (response && response.responseObject.ok) {
        const { data } = response;
        setTotalRecords(data.length);
        setTotalPages(1);
        setPage(1);
        setLoading(false);
        setAddPartSearchResults(data || []);
      } else {
        setLoading(false);
        setAddPartSearchResults([]);
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
    setIsOpen(isOpen);
    setForm(defaultForm);
    setAddPartSearchResults([]);
    if (isOpen) loadParts(page);
  }, [isOpen]);

  useEffect(() => {
    setPcbs(rest.pcbs);
  }, [rest.pcbs]);

  useEffect(() => {
    setProjectPartIds(_.filter(rest.parts, i => form.pcbId > 0 ? i.pcbId === form.pcbId : true ).map(i => (i.partId)) || []);
  }, [rest.parts, form.pcbId]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (rest.onClose) rest.onClose();
  };

  const handleAddPartsNextPage = async (e, page) => { 
    await loadParts(page, true, filterBy, filterByValue, pageSize, sortBy, sortDirection, form.keyword || "");
  };

  const handleAddPartSelectPart = (e, part) => {
    if (selectedPart === part)
      setSelectedPart(null);
    else
      setSelectedPart(part);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPage(1);
    setPageSize(pageSize);
    setAddPartSearchResults([]);
    if (form.keyword)
      await search(form.keyword);
    else
      await loadParts(1, true);
  };

  const updateNumberPicker = (e) => {
    setForm({ ...form, quantity: e.value.toString() });
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    switch (control.name) {
      case "keyword":
        if (control.value && control.value.length > 0) {
          searchDebounced(control.value);
        } else {
          // reset and ensure parts grid is empty
          setLoading(false);
          setAddPartSearchResults([]);
        }
        break;
      default:
        break;
    }
    setForm({ ...form });
  };

  const handleAddPart = (e) => {
    confirmAddPartClose();
    if (rest.onAdd) {
      rest.onAdd(e, {
        part: { ...selectedPart },
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
            You have not selected a part from your inventory.<br />
            Are you sure you want to add this part <b>{{ keyword: form.keyword }}</b> without associating it to a part in your inventory?<br /><br />
            <span className="small">Note: You will still be able to manage it's quantity if you choose to proceed, but it will not appear in your inventory.</span>
          </Trans>
        </p>}
      />
      <Modal centered open={_isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.addBomPartModal.title', "BOM Management")}</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
          <Header style={{ marginBottom: '2px' }}>{t('comp.addBomPartModal.confirmHeader', "Add Part")}</Header>
          <p>{t('comp.addBomPartModal.description', "Add a part to your BOM, optionally associating it with a particular PCB.")}</p>
        </Modal.Description>
        <Modal.Content scrolling style={{ paddingTop: '0' }}>
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
                      Enter a custom Reference Id you can use for identifying this part.<br />Examples: <i>Optoisolator1</i>, <i>Capacitor Array</i>
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
                      Enter one or more Schematic Reference Ids you can use for identifying this part on the PCB silkscreen.<br />Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                    </Trans>
                  </p>}
                  trigger={<Form.Input label={t('label.schematicReferenceIds', "Schematic Reference Id(s)")} placeholder="D1,D2" name="schematicReferenceId" value={form.schematicReferenceId || ''} onChange={handleChange} icon="hashtag" />}
                />
              </Form.Field>
              <Form.Field width={4}>
                <Popup
                  wide
                  content={<p>{t('comp.addBomPartModal.popup.notes', "Enter any custom notes for this part.")}</p>}
                  trigger={<Form.Field label={t('label.notes', "Notes")} type='text' control={TextArea} style={{ height: '43px', minHeight: '43px', padding: '5px' }} name='notes' onChange={handleChange} value={form.notes || ''} />}
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
                  trigger={<Form.Field label={t('label.customDescription', "Custom Description")} type='text' control={TextArea} style={{ height: '43px', minHeight: '43px', padding: '5px' }} name='customDescription' onChange={handleChange} value={form.customDescription || ''} />}
                />
              </Form.Field>
            </Form.Group>
          </Form>

          <PartsGrid2Memoized
            parts={addPartSearchResults}
            page={page}
            totalPages={totalPages}
            loading={loading}
            loadPage={handleAddPartsNextPage}
            onPartClick={handleAddPartSelectPart}
            onPageSizeChange={handlePageSizeChange}
            selectedPart={selectedPart}
            defaultVisibleColumns="partNumber,quantity,manufacturerPartNumber,description,cost"
            editable={false}
            visitable={false}
            disabledPartIds={projectPartIds || []}
            settingsName="bomAddPartModal"
            name="partsGrid"
          >
            {t('message.noMatchingResults', "No matching results.")}
          </PartsGrid2Memoized>
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
  isOpen: PropTypes.bool,
  /** The pcb to select when the modal is opened */
  defaultPcb: PropTypes.object,
  /** The list of parts in the BOM */
  parts: PropTypes.array
};
