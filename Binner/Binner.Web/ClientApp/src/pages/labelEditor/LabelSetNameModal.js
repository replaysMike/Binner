import React, { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Icon, Button, Form, Modal, Header, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";

/**
 * Set the label name
 */
export function LabelSetNameModal(props) {
  const { t } = useTranslation();
  LabelSetNameModal.abortController = new AbortController();
  const defaultForm = { name: props.name || "", isDefaultPartLabel: props.isDefaultPartLabel || false };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [isDirty, setIsDirty] = useState(false);

  useEffect(() => {
    setIsOpen(props.isOpen);
    setForm(defaultForm);
  }, [props.isOpen]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleChange = (e, control) => {
    switch(control.name) {
      default:
        form[control.name] = control.value;
        break;
    }
    switch(control.type) {
      default:
        break;
      case 'checkbox':
        form[control.name] = control.checked;
        break;
    }
    setForm({ ...form });
    setIsDirty(true);
  };

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.labelSelectionModal.title', "Label Editor")}</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
          <p>{t('comp.labelSetNameModal.description', "Enter a name for your new label.")}</p>
        </Modal.Description>
        <Modal.Content style={{width: "100%"}}>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '50%' }}>
            <Form.Field>
							<label>Label Name</label>
							<Form.Input name="name" placeholder="Parts Label" fluid required value={form.name || props.name} onChange={handleChange} />
						</Form.Field>
            <Form.Field>
							<label>Make default part label?</label>
							<Form.Checkbox toggle name="isDefaultPartLabel" checked={form.isDefaultPartLabel || props.isDefaultPartLabel} onChange={handleChange} />
						</Form.Field>
          </Form>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={e => props.onSave(e, form)} disabled={form.name.length === 0}>
            <Icon name="save" /> {t('button.save', "Save")}
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

LabelSetNameModal.propTypes = {
  /** Event handler when selecting an existing label */
  onSave: PropTypes.func.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool,
  /** Value for name field */
  name: PropTypes.string,
  isDefaultPartLabel: PropTypes.bool,
};

LabelSetNameModal.defaultProps = {
  isOpen: false,
  name: ""
};
