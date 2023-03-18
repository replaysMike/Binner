import React, { useState, useEffect } from "react";
import { Icon, Button, Form, Modal, Image, Header, Popup, Input, Table, Tab } from "semantic-ui-react";
import PropTypes from "prop-types";
import NumberPicker from "./NumberPicker";
import _ from "underscore";

export function ProducePcbModal(props) {
  ProducePcbModal.abortController = new AbortController();
  const defaultForm = { pcbs: [], quantity: 1 };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [pcbs, setPcbs] = useState([]);

  const pcbOptions = [
    { key: -1, text: 'All', description: 'Produce the entire BOM', value: -1 },
    { key: 0, text: 'Unassociated', description: 'Produce parts not associated to a PCB', value: 0 },
  ];
  if (pcbs && pcbs.length > 0) {
    for(let i = 0; i < pcbs.length; i++)
      pcbOptions.push({ key: i + 1, text: pcbs[i].name, description: pcbs[i].description, value: pcbs[i].pcbId});
  }

  const generateSerialNumber = (p) => {
    const format = p.serialNumberFormat;
    const lastSerialNumber = p.lastSerialNumber || p.serialNumberFormat;
    if (format) {
      // find the index of the last non-numeric character
      let lastNonNumericIndex = 0;
      for(let i = 0; i < lastSerialNumber.length; i++) {
        let charcode = lastSerialNumber.charCodeAt(i);
        if (charcode < 48 || charcode > 57)
          lastNonNumericIndex = i;
      }
      // parse the remainder as an integer
      const numericLabel = lastSerialNumber.substring(lastNonNumericIndex + 1);
      const parsedNumber = parseInt(numericLabel);
      if (!isNaN(parsedNumber)) {
        // increment it
        const nextSerialNumberInt = parsedNumber + 1;
        const labelPortion = lastSerialNumber.substring(0, lastNonNumericIndex + 1);
        const nextSerialNumber = labelPortion.padEnd(labelPortion.length + numericLabel.length - numDigits(nextSerialNumberInt), '0') + nextSerialNumberInt;
        return nextSerialNumber;
      }

    }
    return '';
  };

  const numDigits = (x) => {
    return (Math.log10((x ^ (x >> 31)) - (x >> 31)) | 0) + 1;
  };

  useEffect(() => {
    setIsOpen(props.isOpen);
    setForm(defaultForm);
  }, [props.isOpen]);

  useEffect(() => {
    const newPcbs = props.pcbs.map(p => ( {...p, serialNumber: generateSerialNumber(p)}));
    setPcbs(newPcbs);
  }, [props.pcbs]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    setForm({ ...form });
  };

  const handleSerialNumberChange = (e, control) => {
    const p = _.find(pcbs, x => x.pcbId === control.id);
    p[control.name] = control.value;
    setPcbs([...pcbs]);
  };

  const handleSubmit = (e) => {
    if (props.onSubmit) {
      let pcbsToProcess = _.filter(pcbs, x => form.pcbs.includes(x.pcbId));
      if (form.pcbs.includes(-1))
        pcbsToProcess = pcbs;
      props.onSubmit(e, { ...form,
        unassociated: form.pcbs.includes(0) || form.pcbs.includes(-1),
        pcbs: pcbsToProcess 
      });
    } else {
      console.error("No onSubmit handler defined!");
    }
  };

  const updateNumberPicker = (e) => {
    setForm({ ...form, quantity: e.value.toString() });
  };

  let pcbsToDisplay = _.filter(pcbs, x => form.pcbs.includes(x.pcbId));
  if (form.pcbs.includes(-1))
    pcbsToDisplay = pcbs;

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose}>
        <Modal.Header>BOM Management</Modal.Header>
        <Modal.Content scrolling image>
          <Image size="medium" src="/image/pcb.png" wrapped />
          <Modal.Description style={{ width: "100%" }}>
            <Header>Produce Pcb</Header>
            <Form style={{ marginBottom: "10px" }}>
              <Form.Field width={10}>
                <Popup
                  wide
                  content="Select the pcb(s) you would like to produce. If you don't define PCB's, choose Unassociated or All."
                  trigger={
                    <Form.Dropdown label='Select PCB(s)' placeholder="Choose PCB(s) to produce" multiple selection value={form.pcbs || []} options={pcbOptions} onChange={handleChange} name='pcbs' />
                  }
                />
              </Form.Field>
              <Form.Field width={4}>
                <Popup
                  wide
                  content={<p>Enter the quantity of PCBs you are producing.</p>}
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
              {pcbsToDisplay.length > 0 &&
                <Table className="small">
                  <Table.Header>
                    <Table.Row>
                      <Table.HeaderCell>PCB</Table.HeaderCell>
                      <Table.HeaderCell>Next Serial Number</Table.HeaderCell>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                  {pcbsToDisplay.map((p, key) => (
                    <Table.Row key={key}>
                      <Table.Cell>{p.name}</Table.Cell>
                      <Table.Cell>
                        <Form.Field>
                          <Popup
                            wide
                            content="The next PCB will have it's serial number started at this value."
                            trigger={
                              <Input style={{display: 'inline-block', width: '200px'}} placeholder="SN00000000" name="serialNumber" value={p.serialNumber || ''} id={p.pcbId} onChange={handleSerialNumberChange} />
                            }
                          />
                        </Form.Field>
                      </Table.Cell>
                    </Table.Row>
                  ))}
                  </Table.Body>
                </Table>
              }
            </Form>
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>Cancel</Button>
          <Button primary onClick={handleSubmit} disabled={form.pcbs.length === 0}>
            <Icon name="microchip" /> Produce
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

ProducePcbModal.propTypes = {
  /** Event handler when adding a new part */
  onSubmit: PropTypes.func.isRequired,
  /** The array of pcb's for the project */
  pcbs: PropTypes.array.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

ProducePcbModal.defaultProps = {
  isOpen: false
};
