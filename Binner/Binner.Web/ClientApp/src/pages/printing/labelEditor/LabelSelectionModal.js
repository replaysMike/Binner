import React, { useState, useEffect, useCallback } from "react";
import { useTranslation } from 'react-i18next';
import { Icon, Button, Form, Modal, Header } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";
import { fetchApi } from "../../../common/fetchApi";

/**
 * Select or create a label
 */
export function LabelSelectionModal({ isOpen, onSelect, onClose }) {
  const { t } = useTranslation();
  LabelSelectionModal.abortController = new AbortController();
  const defaultForm = { labelId: -1 };
  const [open, setOpen] = useState(isOpen);
  const [form, setForm] = useState(defaultForm);
	const [labels, setLabels] = useState([]);
	const [labelOptions, setLabelOptions] = useState([]);

  useEffect(() => {
    setOpen(isOpen);
    setForm(defaultForm);
		if (isOpen && labelOptions.length === 0)
			loadLabels();
  }, [isOpen]);

  const handleModalClose = (e) => {
    setOpen(false);
    if (onClose) onClose();
  };

  const handleChange = (e, control) => {
    switch(control.name) {
      default:
        form[control.name] = control.value;
        break;
    }
    setForm({ ...form });
  };

  const handleSelect = (e) => {
    if (onSelect) {
      onSelect(e, _.find(labels, i => i.labelId === form.labelId));
		setLabels([]);
		setLabelOptions([]);
		setForm(defaultForm);
    } else {
      console.error("No onSelect handler defined!");
    }
  };

	const loadLabels = useCallback(async () => {
		fetchApi("api/print/labels", {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    }).then((response) => {
      if (response.responseObject.ok) {
				const { data } = response;
				setLabels(data);
				const newLabelOptions = [
          { key: -1, text: t('comp.labelSelectionModal.createNewLabel', "Create a new label..."), value: -1 }, 
          ...data.map((item, key) => ({ key: key, text: item.name, value: item.labelId, icon: item.isPartLabelTemplate ? "checkmark" : "", description: item.isPartLabelTemplate ? "Default Part Label" : "" }))
        ];
				setLabelOptions(newLabelOptions);
			}
    });
	}, []);

	useEffect(() => {
		loadLabels();
	}, []);

  return (
    <div>
      <Modal centered open={open || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.labelSelectionModal.title', "Label Editor")}</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
        <Header style={{marginBottom: '2px'}}>{t('comp.labelSelectionModal.header', "Select Label")}</Header>
          <p>{t('comp.labelSelectionModal.description', "Select an existing label to edit or create a new one.")}</p>
        </Modal.Description>
        <Modal.Content style={{width: "100%", minHeight: '300px'}}>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '50%' }}>
            <Form.Field>
							<label>{t('comp.labelSelectionModal.chooseLabel', "Choose the label you wish to edit")}</label>
							<Form.Dropdown name="labelId" style={{zIndex: '9999'}} placeholder="Select a label..." fluid selection value={form.labelId} options={labelOptions} onChange={handleChange} />
						</Form.Field>
          </Form>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleSelect}>
            <Icon name="check" /> {t('button.select', "Select")}
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

LabelSelectionModal.propTypes = {
  /** Event handler when selecting an existing label */
  onSelect: PropTypes.func.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

LabelSelectionModal.defaultProps = {
  isOpen: false
};
