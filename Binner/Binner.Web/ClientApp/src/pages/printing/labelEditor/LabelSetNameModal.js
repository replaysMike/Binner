import { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Icon, Button, Form, Modal, Checkbox } from "semantic-ui-react";
import PropTypes from "prop-types";

/**
 * Set the label name
 */
export function LabelSetNameModal({ ...rest }) {
  const { t } = useTranslation();
  const defaultForm = {
    name: rest.name || "",
    isDefaultPartLabel: rest.isDefaultPartLabel || false
  };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);

  useEffect(() => {
    setIsOpen(rest.isOpen);
  }, [rest.isOpen]);

  useEffect(() => {
    setForm(prev => ({ ...prev, name: rest.name }));
  }, [rest.name]);

  useEffect(() => {
    setForm(prev => ({ ...prev, isDefaultPartLabel: rest.isDefaultPartLabel }));
  }, [rest.isDefaultPartLabel]);

  const handleModalClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsOpen(false);
    if (rest.onClose) rest.onClose();
  };

  const handleChange = (e, control) => {
    switch (control.name) {
      case "isDefaultPartLabel":
        form[control.name] = control.checked;
        break;
      default:
        form[control.name] = control.value;
        break;
    }
    setForm({ ...form });
  };

  const handleSave = (e) => {
    if (rest.onSave) rest.onSave(e, form);
  };

  return (
    <div>
      <Modal centered open={isOpen} onClose={handleModalClose}>
        <Modal.Header>{t('comp.labelSelectionModal.title', "Label Editor")}</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
          <p>{t('comp.labelSetNameModal.description', "Enter a name for your label.")}</p>
        </Modal.Description>
        <Modal.Content style={{ width: "100%" }}>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '50%' }}>
            <Form.Field>
              <label>{t('comp.labelSetNameModal.labelName', "Label Name")}</label>
              <Form.Input name="name" placeholder="Parts Label" fluid required value={form.name} onChange={handleChange} />
            </Form.Field>
            <Form.Field>
              <label>{t('comp.labelSetNameModal.makeDefault', "Make default part label?")}</label>
              <Checkbox toggle name="isDefaultPartLabel" checked={form.isDefaultPartLabel} onChange={handleChange} />
            </Form.Field>
          </Form>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleSave}>
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
  /** True if this should be the default part label */
  isDefaultPartLabel: PropTypes.bool,
};
