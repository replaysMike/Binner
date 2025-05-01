import React, { useState, useEffect, useCallback } from "react";
import { useTranslation } from 'react-i18next';
import { Button, Form, Modal, Image, Header, Popup, Input, Table, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import NumberPicker from "../NumberPicker";
import _ from "underscore";
import { cloneDeep } from "lodash";
import { getProduciblePcbCount, getProducibleBomCount, getProducibleUnassociatedCount, getOutOfStockParts, consumeFromPartList } from "../../common/bomTools";
import { toast } from "react-toastify";

export function ProducePcbModal({ isOpen = false, ...rest }) {
  const { t } = useTranslation();
  ProducePcbModal.abortController = new AbortController();
  const defaultForm = { pcbs: [], quantity: 1 };
  const [_isOpen, setIsOpen] = useState(isOpen);
  const [form, setForm] = useState(defaultForm);
  const [project, setProject] = useState({ parts:[], pcbs: []});
  const [pcbOptions, setPcbOptions] = useState([]);

  const canProducePcb = (parts, pcb = null) => {
    const formQuantity = parseInt(form.quantity) || 1;
    if (pcb && pcb.pcbId > 0) {
      const pcbParts = _.filter(parts, p => p.pcbId === pcb.pcbId);
      const pcbCount = getProduciblePcbCount(pcbParts, pcb);

      return pcbCount.count >= formQuantity;
    }
    // unassociated
    const pcbCount = getProducibleUnassociatedCount(parts);
    return pcbCount >= formQuantity;
  };

  const createPcbOptions = (project) => {
    const options = [
      { key: -1, text: t('comp.producePcbModal.options.all', "All"), description: t('comp.producePcbModal.options.allDescription', "Produce the entire BOM"), value: -1, icon: getProducibleBomCount(project.parts, project.pcbs) === 0 && "warning circle", disabled: getProducibleBomCount(project.parts, project.pcbs).count === 0 },
      { key: 0, text: t('comp.producePcbModal.options.unassociated', "Unassociated"), description: t('comp.producePcbModal.options.unassociatedDescription', "Produce parts not associated to a PCB"), value: 0, icon: !canProducePcb(project.parts) && "warning circle", disabled: getProducibleUnassociatedCount(project.parts) === 0 },
    ];
  
    if (project && project.pcbs && project.pcbs.length > 0) {
      for(let i = 0; i < project.pcbs.length; i++) {
        options.push({ key: i + 1, text: project.pcbs[i].name, description: project.pcbs[i].description, value: project.pcbs[i].pcbId, icon: !canProducePcb(project.parts, project.pcbs[i]) && "warning circle",  disabled: !canProducePcb(project.parts, project.pcbs[i])});
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
    setIsOpen(_isOpen);
    setForm(defaultForm);
  }, [_isOpen]);

  useEffect(() => {
    const newPcbs = rest.project.pcbs.map(p => ( {...p, serialNumber: generateSerialNumber(p)}));
    const newProject = {...rest.project, pcbs: newPcbs};
    setProject(newProject);
    setPcbOptions(createPcbOptions(newProject));
  }, [rest.project]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (rest.onClose) rest.onClose();
  };

  const handleChange = (e, control) => {

    // re-evaluate the list and set as disabled if no inventory is left to produce the pcb
    // value = array of pcb ids
    let pcbIds = [];

    // keep a running part consumed count and only allow selection if there are enough parts left.
    // clone the inventory first, as we don't want to modify the original part quantities
    let assignedParts = cloneDeep(project.parts);

    const inventory = _.unique(_.map(assignedParts, x => x.part), x => x.partId);
    for(let i = 0; i < control.value.length; i++) {
      let allowSelect = false;

      const targetPcb = _.find(project.pcbs, x => x.pcbId === control.value[i]);

      const canProduce = consumeFromPartList(inventory, assignedParts, targetPcb, form.quantity);
      if (!canProduce) {
        toast.info("You don't have enough inventory to produce this PCB.");
      }

      if (canProduce)
        allowSelect = true;

      if (allowSelect)
        pcbIds.push(control.value[i]);
    }

    form[control.name] = pcbIds;
    setForm({ ...form });
  };

  const getCanProduce = (assignedParts, pcb) => {
    const inventory = _.unique(_.map(assignedParts, x => x.part), x => x.partId);
    return consumeFromPartList(inventory, assignedParts, pcb, form.quantity);
  };

  const handleSerialNumberChange = (e, control) => {
    const p = _.find(project.pcbs, x => x.pcbId === control.id);
    p[control.name] = control.value;
    setProject({...project});
  };

  const handleSubmit = (e) => {
    if (rest.onSubmit) {
      let pcbsToProcess = _.filter(project.pcbs, x => form.pcbs.includes(x.pcbId));
      if (form.pcbs.includes(-1))
        pcbsToProcess = project.pcbs;
      
      // ensure we don't pass any pcb's to produce that don't have enough parts
      const validPcbsToProcess = [];
      for(let i = 0; i < pcbsToProcess.length; i++){
        if (canProducePcb(project, pcbsToProcess[i])){
          validPcbsToProcess.push(pcbsToProcess[i]);
        }
      }

      // only process unassociated if parts are available
      let processUnassociated = false;
      if ((form.pcbs.includes(0) || form.pcbs.includes(-1)) && canProducePcb(project))
        processUnassociated = true;

      rest.onSubmit(e, { ...form,
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

  // keep a running part consumed count and only allow selection if there are enough parts left.
  // clone the inventory first, as we don't want to modify the original part quantities
  let assignedParts = cloneDeep(project.parts);
  let allowProduce = form.pcbs.length > 0;

  return (
    <div>
      <Modal centered open={isOpen || false} onClose={handleModalClose} className="producePcbModal">
        <Modal.Header>{t('comp.producePcbModal.title', "BOM Management")}</Modal.Header>
        <Modal.Content scrolling image>
          <Image size="medium" src="/image/pcb.png" wrapped />
          <Modal.Description style={{ width: "100%" }}>
            <Header>{t('comp.producePcbModal.header', "Produce Pcb")}</Header>
            <Form style={{ marginBottom: "10px" }}>
              <Form.Field width={10}>
                <Popup
                  wide
                  content={t('comp.producePcbModal.popup.pcbs', "Select the pcb(s) you would like to produce. If you don't define PCB's, choose Unassociated or All.")}
                  trigger={
                    <Form.Dropdown label={t('comp.producePcbModal.label.pcbs', "Select PCB(s)")} placeholder={t('comp.producePcbModal.placeholder.pcbs', "Choose PCB(s) to produce")} 
                      multiple 
                      selection 
                      value={form.pcbs || []} 
                      options={pcbOptions} 
                      onChange={handleChange}
                      name='pcbs' 
                    />
                  }
                />
              </Form.Field>
              <Form.Field width={4}>
                <Popup
                  wide
                  content={<p>{t('comp.producePcbModal.popup.quantity', "Enter the quantity of PCBs you are producing.")}</p>}
                  trigger={
                    <Form.Field
                      control={NumberPicker}
                      label={t('label.quantity', "Quantity")}
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
                      <Table.HeaderCell width={3}><Popup position="top left" content={t('comp.producePcbModal.popup.nextSerialNumber', "The next serial number assigned to the board")} trigger={<div>{t('comp.producePcbModal.nextSerialNumber', "Next Serial Number")}</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup wide position="top center" content={t('comp.producePcbModal.popup.maxQty', "The maximum number of boards you can produce")} trigger={<div>{t('comp.producePcbModal.maxQty', "Max Qty")}</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup position="top center" content={t('comp.producePcbModal.popup.parts', "The number of parts on the board")} trigger={<div>{t('label.parts', "Parts")}</div>}/></Table.HeaderCell>
                      <Table.HeaderCell textAlign="center"><Popup wide position="top center" content={t('comp.producePcbModal.popup.outOfStock', "The number of parts on the board that are out of stock")} trigger={<div>{t('label.outOfStock', "Out of Stock")}</div>}/></Table.HeaderCell>
                      <Table.HeaderCell width={3}></Table.HeaderCell>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                  {getPcbsToDisplay(project).map((pcb, key) => {
                    const canProduce = getCanProduce(assignedParts, pcb);
                    if (!canProduce) allowProduce = false; // disable produce form

                    return (
                    <Table.Row key={key} className={!canProduce ? "disabled" : ""}>
                      <Table.Cell>{pcb.name}</Table.Cell>
                      <Table.Cell>
                        {pcb.pcbId && <Form.Field>
                          <Popup
                            wide
                            content={t('comp.producePcbModal.popup.serialNumber', "The next PCB will have it's serial number started at this value.")}
                            trigger={
                              <Input disabled={!canProduce} style={{display: 'inline-block', width: '200px'}} placeholder="SN00000000" name="serialNumber" value={pcb.serialNumber || ''} id={pcb.pcbId} onChange={handleSerialNumberChange} />
                            }
                          />
                        </Form.Field>}
                      </Table.Cell>
                      <Table.Cell textAlign="center">{pcb.pcbId > 0 ? getProduciblePcbCount(project.parts, pcb).count : getProducibleUnassociatedCount(project.parts)}</Table.Cell>
                      <Table.Cell textAlign="center">{pcb.pcbId > 0 ? _.filter(project.parts, x => x.pcbId === pcb.pcbId).length : _.filter(project.parts, x => x.pcbId === null).length}</Table.Cell>
                      <Table.Cell textAlign="center">{getTotalPartsOutOfStock(pcb.pcbId && pcb.pcbId > 0 ? pcb : null)}</Table.Cell>
                      <Table.Cell>{!canProduce && (pcb.pcbId > 0 ? _.filter(project.parts, x => x.pcbId === pcb.pcbId).length : _.filter(project.parts, x => x.pcbId === null).length > 0) && <span><Icon name="warning circle" color="red" /> {t('message.notEnoughParts', "Not enough parts")}</span>}</Table.Cell>
                    </Table.Row>
                  );
                  })}
                  </Table.Body>
                </Table>
              }
            </Form>
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button onClick={handleModalClose}>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleSubmit} disabled={!allowProduce}>
            <i className="pcb-icon tiny" /> {t('button.produce', "Produce")}
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
