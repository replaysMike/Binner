import React, { useState, useEffect } from "react";
import { Icon, Button, Form, Modal, TextArea, Image, Header, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";

export function AddPcbModal(props) {
  AddPcbModal.abortController = new AbortController();
  const defaultForm = { name: "", description: "", serialNumberFormat: 'SN00000000' };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);

  useEffect(() => {
    setIsOpen(props.isOpen);
    setForm(defaultForm);
  }, [props.isOpen]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    setForm({ ...form });
  };

  const handleAdd = (e) => {
    if (props.onAdd) {
      props.onAdd(e, form);
    } else {
      console.error("No onAdd handler defined!");
    }
  };

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>BOM Management</Modal.Header>
        <Modal.Description style={{ width: "100%", padding: '5px 25px' }}>
        <Header style={{marginBottom: '2px'}}>Add PCB</Header>
          <p>Adding a PCB allows you to associate your parts with a specific PCB, and even multiple PCBs within a project.</p>
        </Modal.Description>
        <Modal.Content scrolling image style={{width: "100%"}}>
          <Image size="medium" src="/image/pcb.png" wrapped />
          <Form style={{ marginBottom: "10px", width: '100%' }}>
            <Form.Field width={8}>
              <Popup
                wide
                content="Enter the name of your pcb board or module"
                trigger={
                  <Form.Input required label="Name" placeholder="Main Board or module name" name="name" value={form.name || ''} onChange={handleChange} />
                }
              />
            </Form.Field>
            <Form.Field width={8}>
              <Popup
                wide
                content={<p>Enter a description describing what your pcb does. (<i>optional</i>)</p>}
                trigger={
                  <Form.Field
                    type="text"
                    control={TextArea}
                    style={{ height: "50px", minHeight: "50px", padding: "5px" }}
                    label="Description"
                    placeholder="Description of pcb"
                    name="description"
                    value={form.description || ''}
                    onChange={handleChange}
                  />
                }
              />
              
            </Form.Field>
            <Form.Field width={8}>
              <Popup
                wide
                content={<p>Enter your serial number format as a string. The left-most portion of the string will be incremented by 1 each time you produce a PCB. (<i>optional</i>)<br/>Example: <i>SN00000000</i></p>}
                trigger={
                  <Form.Input
                    label="Serial Number Format"
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
          <Button onClick={handleModalClose}>Cancel</Button>
          <Button primary onClick={handleAdd}>
            <Icon name="plus" /> Add
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
