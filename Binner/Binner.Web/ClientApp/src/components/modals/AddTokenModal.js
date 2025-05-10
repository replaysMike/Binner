import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Icon, Button, Form, Modal, Dropdown, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import { UserTokenType } from "../../common/UserTokenType";
import { GetTypeName, GetTypeProperty } from "../../common/Types";
import { toast } from "react-toastify";

/**
 * Add a new user token
 */
export function AddTokenModal({ isOpen = false, onAdd, onClose, ...rest }) {
  const { t } = useTranslation();
  AddTokenModal.abortController = new AbortController();
  const defaultForm = { 
    tokenType: ""
  };
  const [_isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);

  const tokenOptions = [
    { key: 1, text: GetTypeName(UserTokenType, UserTokenType.KiCadApiToken.value), description: GetTypeProperty(UserTokenType, UserTokenType.KiCadApiToken.value, "description"), value: UserTokenType.KiCadApiToken.value },
  ];

  useEffect(() => {
    setIsOpen(isOpen);
    setForm(defaultForm);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    console.log('closing...');
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

  const handleAdd = (e) => {
    if (onAdd) {
      onAdd(e, form);
    } else {
      console.error("No onAdd handler defined!");
    }
  };

  return (
    <div>
      <Modal centered open={_isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.addTokenModal.createToken', "Create Token")}</Modal.Header>
        <Modal.Content>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '100%' }}>
            <Form.Field width={8}>
              <Popup
                wide
                content={t('comp.addTokenModal.popup.tokenType', "Select the type of token you would like to create.")}
                trigger={
                  <Form.Dropdown 
                    label={t('comp.addTokenModal.tokenType', "Select token type")} 
                    selection 
                    value={form.tokenType || ''} 
                    options={tokenOptions} 
                    onChange={handleChange} 
                    name='tokenType' 
                  />
                }
              />
            </Form.Field>
          </Form>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleAdd}>
            <Icon name="plus" /> {t('button.add', "Add")}
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

AddTokenModal.propTypes = {
  /** Event handler when adding a new part */
  onAdd: PropTypes.func.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
