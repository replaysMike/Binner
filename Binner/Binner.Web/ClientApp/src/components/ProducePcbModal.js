import React, { useState, useEffect, useCallback } from "react";
import { Button, Form, Modal, Image, Header, Popup, Input, Table, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import NumberPicker from "./NumberPicker";
import _ from "underscore";
import { getProduciblePcbCount, getProducibleUnassociatedCount, getOutOfStockParts } from "../common/bomTools";

export function ProducePcbModal(props) {
  ProducePcbModal.abortController = new AbortController();
  const defaultForm = { pcbs: [], quantity: 1 };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [project, setProject] = useState({ parts:[], pcbs: []});
  const [pcbOptions, setPcbOptions] = useState([]);

  const canProducePcb = (pcb) => {
    const formQuantity = parseInt(form.quantity) || 1;
    if (pcb && pcb.pcbId > 0) {
      const pcbParts = _.filter(project.parts, p => p.pcbId === pcb.pcbId);
      const pcbCount = getProduciblePcbCount(pcbParts, pcb);
      return pcbCount.count >= formQuantity;
    }
    // unassociated
    const pcbCount = getProducibleUnassociatedCount(project.parts);
    return pcbCount >= formQuantity;
  };

  const createPcbOptions = (project) => {
    const options = [
      { key: -1, text: 'All', description: 'Produce the entire BOM', value: -1 },
      { key: 0, text: 'Unassociated', description: 'Produce parts not associated to a PCB', value: 0, icon: !canProducePcb() && "warning circle", disabled: getProducibleUnassociatedCount(project.parts) === 0 },
    ];
  
    if (project && project.pcbs && project.pcbs.length > 0) {
      for(let i = 0; i < project.pcbs.length; i++) {
        options.push({ key: i + 1, text: project.pcbs[i].name, description: project.pcbs[i].description, value: project.pcbs[i].pcbId, icon: !canProducePcb(project.pcbs[i]) && "warning circle",  disabled: !canProducePcb(project.pcbs[i])});
      }
    }
    return options;
  };

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
    const newPcbs = props.project.pcbs.map(p => ( {...p, serialNumber: generateSerialNumber(p)}));
    const newProject = {...props.project, pcbs: newPcbs};
    setProject(newProject);
    setPcbOptions(createPcbOptions(newProject));
  }, [props.project]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    setForm({ ...form });
  };

  const handleSerialNumberChange = (e, control) => {
    const p = _.find(project.pcbs, x => x.pcbId === control.id);
    p[control.name] = control.value;
    setProject({...project});
  };

  const handleSubmit = (e) => {
    if (props.onSubmit) {
      let pcbsToProcess = _.filter(project.pcbs, x => form.pcbs.includes(x.pcbId));
      if (form.pcbs.includes(-1))
        pcbsToProcess = project.pcbs;
      
      // ensure we don't pass any pcb's to produce that don't have enough parts
      const validPcbsToProcess = [];
      for(let i = 0; i < pcbsToProcess.length; i++){
        if (canProducePcb(pcbsToProcess[i])){
          validPcbsToProcess.push(pcbsToProcess[i]);
        }
      }

      // only process unassociated if parts are available
      let processUnassociated = false;
      if ((form.pcbs.includes(0) || form.pcbs.includes(-1)) && canProducePcb())
        processUnassociated = true;

      props.onSubmit(e, { ...form,
        unassociated: processUnassociated,
        pcbs: validPcbsToProcess 
      });
    } else {
      console.error("No onSubmit handler defined!");
    }
  };

  const updateNumberPicker = (e) => {
    setForm({ ...form, quantity: e.value.toString() });
  };

  const getTotalPartsOutOfStock = (pcb) => {
    return getOutOfStockParts(project.parts, pcb).length;
  };

  const getPcbsToDisplay = useCallback((project) => {
    if (!project) return [];
    let pcbsToDisplay = [..._.filter(project.pcbs, x => form.pcbs.includes(x.pcbId))];
    if (form.pcbs.includes(-1)) {
      // include all pcbs
      pcbsToDisplay = [...project.pcbs];
    }
  
    if (form.pcbs.includes(-1) || form.pcbs.includes(0)) {
      // include unassociated
      pcbsToDisplay.push({ name: 'Unassociated' });
    }
    return pcbsToDisplay;
  }, [form]);

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose} className="producePcbModal">
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
                      min={1}
                      value={form.quantity || ""}
                      onChange={updateNumberPicker}
                      name="quantity"
                      autoComplete="off"
                    />
                  }
                />
              </Form.Field>
              {getPcbsToDisplay(project).length > 0 &&
                <Table className="small">
                  <Table.Header>
                    <Table.Row>
                      <Table.HeaderCell width={3}>PCB</Table.HeaderCell>
                      <Table.HeaderCell width={3}><Popup position="top left" content="The next serial number assigned to the board" trigger={<div>Next Serial Number</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup wide position="top center" content="The maximum number of boards you can produce" trigger={<div>Max Qty</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup position="top center" content="The number of parts on the board" trigger={<div>Parts</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup wide position="top center" content="The number of parts on the board that are out of stock" trigger={<div>Out of Stock</div>}/></Table.HeaderCell>
                      <Table.HeaderCell width={3}></Table.HeaderCell>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                  {getPcbsToDisplay(project).map((p, key) => (
                    <Table.Row key={key} className={!canProducePcb(p) ? "disabled" : ""}>
                      <Table.Cell>{p.name}</Table.Cell>
                      <Table.Cell>
                        {p.pcbId && <Form.Field>
                          <Popup
                            wide
                            content="The next PCB will have it's serial number started at this value."
                            trigger={
                              <Input disabled={!canProducePcb(p)} style={{display: 'inline-block', width: '200px'}} placeholder="SN00000000" name="serialNumber" value={p.serialNumber || ''} id={p.pcbId} onChange={handleSerialNumberChange} />
                            }
                          />
                        </Form.Field>}
                      </Table.Cell>
                      <Table.Cell textAlign="center">{p.pcbId > 0 ? getProduciblePcbCount(project.parts, p).count : getProducibleUnassociatedCount(project.parts)}</Table.Cell>
                      <Table.Cell textAlign="center">{p.pcbId > 0 ? _.filter(project.parts, x => x.pcbId === p.pcbId).length : _.filter(project.parts, x => x.pcbId === null).length}</Table.Cell>
                      <Table.Cell textAlign="center">{getTotalPartsOutOfStock(p.pcbId && p.pcbId > 0 ? p : null)}</Table.Cell>
                      <Table.Cell>{!canProducePcb(p) && <span><Icon name="warning circle" color="red" /> Not enough parts</span>}</Table.Cell>
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
            <i className="pcb-icon tiny" /> Produce
          </Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

ProducePcbModal.propTypes = {
  /** Event handler when adding a new part */
  onSubmit: PropTypes.func.isRequired,
  /** The project */
  project: PropTypes.object.isRequired,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

ProducePcbModal.defaultProps = {
  isOpen: false
};
