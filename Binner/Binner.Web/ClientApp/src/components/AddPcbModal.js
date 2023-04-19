import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Icon, Button, Form, Modal, TextArea, Input, Image, Header, Popup } from "semantic-ui-react";
import Dropzone from "./Dropzone";
import PropTypes from "prop-types";
import { toast } from "react-toastify";
import NumberPicker from "./NumberPicker";

export function AddPcbModal(props) {
  const { t } = useTranslation();
  AddPcbModal.abortController = new AbortController();
  const defaultForm = { name: "", description: "", quantity: "1", cost: 0, serialNumberFormat: 'SN00000000', lastSerialNumber: null, image: null, imageBlob: null };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [image, setImage] = useState(null);

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
      case 'cost':
        // apply formatting

        form[control.name] = control.value;

        break;
      default:
        form[control.name] = control.value;
        break;
    }
    setForm({ ...form });
  };

  const handleFormatCost = (e, control) => {
    form.cost = Number(form.cost).toFixed(2);
    if (isNaN(form.cost)) form.cost = Number(0).toFixed(2);
    setForm({ ...form });
  };

  const updateNumberPicker = (e) => {
    setForm({ ...form, quantity: e.value.toString() });
  };

  const handleAdd = (e) => {
    if (props.onAdd) {
      props.onAdd(e, form);
    } else {
      console.error("No onAdd handler defined!");
    }
  };

  const onUploadSubmit = async (uploadFiles, type) => {
  };

  const onUploadError = (errors) => {
    for (let i = 0; i < errors.length; i++) toast.error(errors[i], { autoClose: 10000 });
  };

  const onDrop = (files) => {
    setForm({...form, image: files[files.length - 1], imageBlob: URL.createObjectURL(files[files.length - 1])});
  };

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>{t('comp.addPcbModal.title', "BOM Management")}</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
        <Header style={{marginBottom: '2px'}}>{t('comp.addPcbModal.header', "Add PCB")}</Header>
          <p>{t('comp.addPcbModal.description', "Adding a PCB allows you to associate your parts with a specific PCB, and even multiple PCBs within a project.")}</p>
        </Modal.Description>
        <Modal.Content scrolling image style={{width: "100%"}}>
          <Dropzone onUpload={onUploadSubmit} onError={onUploadError} onDrop={onDrop}>
          {form.imageBlob !== null ? (<Image src={form.imageBlob} className="medium" />)
           : (<Image size="medium" src="/image/pcb.png" wrapped />)}
            <div style={{ fontSize: "0.6em", textAlign: 'center' }}>Drag an image to upload</div>
          </Dropzone>
          <Form style={{ marginBottom: "10px", marginLeft: '50px', width: '100%' }}>
            <Form.Field width={8}>
              <Popup
                wide
                content={t('comp.addPcbModal.popup.name', "Enter the name of your pcb board or module")}
                trigger={
                  <Form.Input required label={t('label.name', "Name")} placeholder={t('comp.addPcbModal.placeholder.name', "Main Board or module name")} name="name" value={form.name || ''} onChange={handleChange} />
                }
              />
            </Form.Field>
            <Form.Field width={8}>
              <Popup
                wide
                content={<p>
                  <Trans i18nKey="comp.addPcbModal.popup.description">
                  Enter a description describing what your pcb does. (<i>optional</i>)
                  </Trans>
                  </p>}
                trigger={
                  <Form.Field
                    type="text"
                    control={TextArea}
                    style={{ height: "50px", minHeight: "50px", padding: "5px" }}
                    label={t('label.description', "Description")}
                    placeholder={t('comp.addPcbModal.placeholder.description', "Description of pcb")}
                    name="description"
                    value={form.description || ''}
                    onChange={handleChange}
                  />
                }
              />              
            </Form.Field>
            <Form.Group>
              <Form.Field width={4}>
                <Popup
                  wide
                  content={<p>
                    <Trans i18nKey="comp.addPcbModal.popup.quantity">
                    Enter a quantity (multiplier) of PCB's produced each time you create a PCB. This should normally be 1, unless you require several copies of the same PCB for producing your BOM project.<br/><br/><i>Example:</i> An audio amplifier may require 2 of the same PCB's, one for each left/right channel each time you produce the entire assembly.
                    </Trans>
                  </p>}
                  trigger={
                    <Form.Field
                      control={NumberPicker}
                      label={t('label.quantity', "Quantity")}
                      placeholder="1"
                      min={0}
                      value={form.quantity || ""}
                      onChange={updateNumberPicker}
                      name="quantity"
                      autoComplete="off"
                    />
                  }
                />
              </Form.Field>
              <Form.Field width={4}>
                <Popup
                  wide
                  content={<p>{t('comp.addPcbModal.popup.name', "The cost to produce a single PCB board (without components). If using quantity, only specify the cost for a single board as quantity will be taken into consideration.")}</p>}
                  trigger={<Form.Field>
                    <label>{t('label.cost', "Cost")}</label>
                    <Input
                      label="$"
                      placeholder="0.00"
                      value={form.cost || "0.00"}
                      type="text"
                      onChange={handleChange}
                      name="cost"
                      onBlur={handleFormatCost}
                    />
                  </Form.Field>}
                />
              </Form.Field>
            </Form.Group>
            <Form.Field width={8}>
              <Popup
                wide
                content={<p>
                  <Trans i18nKey="comp.addPcbModal.popup.serialNumberFormat">
                  Enter your serial number format as a string. The left-most portion of the string will be incremented by 1 each time you produce a PCB. (<i>optional</i>)<br/>Example: <i>SN00000000</i>
                  </Trans>
                </p>}
                trigger={
                  <Form.Input
                    label={t('label.serialNumberFormat', "Serial Number Format")}
                    placeholder="SN00000000"
                    name="serialNumberFormat"
                    value={form.serialNumberFormat || ''}
                    onChange={handleChange}
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

AddPcbModal.propTypes = {
  /** Event handler when adding a new part */
  onAdd: PropTypes.func.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

AddPcbModal.defaultProps = {
  isOpen: false
};
