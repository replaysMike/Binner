import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Icon, Button, Form, Modal, Popup, Grid } from "semantic-ui-react";
import PropTypes from "prop-types";
import { UserTokenType } from "../../common/UserTokenType";
import { GetTypeName, GetTypeProperty } from "../../common/Types";

/**
 * Add a new user token
 */
export function AddTokenModal({ isOpen = false, onAdd, onClose, ...rest }) {
  const { t } = useTranslation();
  AddTokenModal.abortController = new AbortController();
  const defaultForm = { 
    tokenType: "",
    partsTimeout: 5,
    categoriesTimeout: 10
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
    const data = {
      tokenType: form.tokenType,
      tokenConfig: JSON.stringify({
        timeout_parts_seconds: parseInt(form.partsTimeout),
        timeout_categories_seconds: parseInt(form.categoriesTimeout),
      })
    }
    if (onAdd) {
      onAdd(e, data);
    } else {
      console.error("No onAdd handler defined!");
    }
  };

  const renderOptions = () => {
    switch(form.tokenType){
      case UserTokenType.KiCadApiToken.value:
        return (
          <div style={{padding: '20px'}}>
            <Form.Field>
              <Popup  
                content={<p>Enter the timeout (in seconds) for the parts endpoint.<br /><i>Default: 5</i></p>}
                trigger={<Form.Input
                  label="Parts Timeout (seconds)"
                  placeholder="5"
                  value={form.partsTimeout || ''}
                  name="partsTimeout"
                  onChange={handleChange}
                />}
              />
            </Form.Field>
            <Form.Field>
              <Popup
                content={<p>Enter the timeout (in seconds) for the categories endpoint.<br /><i>Default: 10</i></p>}
                trigger={<Form.Input
                  label="Categories Timeout (seconds)"
                  placeholder="10"
                  value={form.categoriesTimeout || ''}
                  name="categoriesTimeout"
                  onChange={handleChange}
                />}
              />
            </Form.Field>
          </div>
        );
    }
    return (<></>);
  };

  return (
    <div>
      <Modal centered open={_isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.addTokenModal.createToken', "Create Token")}</Modal.Header>
        <Modal.Content>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '100%' }}>
            <Grid columns={2}>
              <Grid.Row>
                <Grid.Column width={8}>
                  <Form.Field>
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
                </Grid.Column>
                <Grid.Column>
                  {renderOptions()}
                </Grid.Column>
              </Grid.Row>
            </Grid>
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
