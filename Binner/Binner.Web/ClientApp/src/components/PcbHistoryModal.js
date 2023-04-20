import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Button, Form, Modal, Image, Header, Confirm, Input, Table, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import { getImagesToken } from "../common/authentication";
import { getCurrencySymbol } from "../common/Utils";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import _ from "underscore";
import "./PcbHistoryModal.css";

export function PcbHistoryModal(props) {
  const { t } = useTranslation();
  PcbHistoryModal.abortController = new AbortController();
  const defaultForm = { pcbs: [], quantity: 1 };
  const [isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [history, setHistory] = useState({ pcbs: [] });
  const [confirmDeletePcbIsOpen, setConfirmDeletePcbIsOpen] = useState(false);
  const [confirmDeletePcbContext, setConfirmDeletePcbContext] = useState(null);
  const [confirmDeletePcbContent, setConfirmDeletePcbContent] = useState(null);
  const [imageIndex, setImageIndex] = useState(null);

  useEffect(() => {
    setIsOpen(props.isOpen);
  }, [props.isOpen]);

  useEffect(() => {
    setHistory({...props.history, pcbs: props.history?.pcbs?.map(p => ({...p, pcbCost: p.pcbCost.toFixed(2) })) });
  }, [props.history]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (props.onClose) props.onClose();
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    setForm({ ...form });
  };

  const inlineSave = async () => {
    const request = {
			...history,
      pcbs: history.pcbs.map((p, i) => ({
        ...p,
        pcbCost: parseFloat(p.pcbCost),
        pcbQuantity: parseInt(p.pcbQuantity)
      }))
    };
    const response = await fetchApi('api/bom/produce/history', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      // todo: update history
    }
    else {
      toast.error(t('error.failedSaveHistory', "Failed to save history record!"));
    }
  };

  const handleDeletePcb = async (e) => {
    const request = {...confirmDeletePcbContext };
    const response = await fetchApi('api/bom/produce/history/pcb', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    }).catch(() => {
		});
    if (response && response.responseObject.status === 200) {
			// remove pcb from list
			setHistory({...history, pcbs: _.filter(history.pcbs, i => i.projectPcbProduceHistoryId !== request.projectPcbProduceHistoryId )})
      toast.success(t('success.deletedRecord', 'Record was deleted!'));
    }
    else
      toast.error(t('error.failedDeleteRecord', "Failed to remove record!"));
    setConfirmDeletePcbIsOpen(false);
		setConfirmDeletePcbContext(null);
  };

  const focusColumn = (e) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const saveColumn = async (e, entity) => {
    switch(e.target.name) {
      case "pcbCost":
        entity.pcbCost = parseFloat(entity.pcbCost).toFixed(2);
        updatePcb(entity);
        break;
      default:
        break;
    }
    await inlineSave();
  };

	const handleInlineChange = (e, control, pcb) => {   
    pcb[control.name] = control.value;
    updatePcb(pcb);
  };

  const updatePcb = (pcb) => {
    const newPcbs = history.pcbs.map((item, i) => {
      if (item.pcbId === pcb.pcbId)
        return pcb;
      else
        return item;
    });

    setHistory({...history, pcbs: newPcbs });
  }

  const confirmDeletePcbClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeletePcbIsOpen(false);
		setConfirmDeletePcbContext(null);
  };

  const confirmDeletePcbOpen = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
		setConfirmDeletePcbContext(p);
    setConfirmDeletePcbIsOpen(true);
    setConfirmDeletePcbContent(
      <p>
        {t('confirm.deleteHistory', "Are you sure you want to delete this history record?")}
        <br />
        <br />
        <Trans i18nKey='confirm.partsWillBeBackInStock'>
        All consumed parts will be placed <b>back in stock</b>.
        </Trans>
        <br />
        <br />
        <Trans i18nKey='confirm.permanent'>
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const getImage = () => {
    if (imageIndex) {
      if (history.pcbs && history.pcbs.length >= imageIndex && history.pcbs[imageIndex]?.pcb?.storedFile?.fileName)
        return (<Image src={`api/storedFile/preview?fileName=${history.pcbs[imageIndex].pcb.storedFile.fileName}&token=${getImagesToken()}`} size="medium" />);
    } else if (history.pcbs && history.pcbs.length > 0 && history.pcbs[0]?.pcb?.storedFile?.fileName) {
      return (<Image src={`api/storedFile/preview?fileName=${history.pcbs[0]?.pcb.storedFile.fileName}&token=${getImagesToken()}`} size="medium" />);
    }
    return (<Image size="medium" src="/image/pcb.png" wrapped />);
  };

  
  return (
    <div>
      <Confirm
        className="confirm"
        header={t('page.project.confirm.deletePcbHeader', "Delete Pcb")}
        open={confirmDeletePcbIsOpen}
        onCancel={confirmDeletePcbClose}
        onConfirm={handleDeletePcb}
        content={confirmDeletePcbContent}
      />
      <Modal centered open={isOpen || false} onClose={handleModalClose} className="pcbHistoryModal">
        <Modal.Header>{t('comp.pcbHistoryModal.title', "BOM Management")}</Modal.Header>
        <Modal.Content scrolling>
          <div className="image square">
            {getImage()}
          </div>
          <Modal.Description style={{ width: "100%" }}>
            <Header>{t('comp.pcbHistoryModal.header', "PCB Produce History")}</Header>
            <Form style={{ marginBottom: "10px" }}>
              <Table className="history">
                <Table.Header>
                  <Table.Row>
                    <Table.HeaderCell>{t('label.pcb', "PCB")}</Table.HeaderCell>
                    <Table.HeaderCell width={1}>
                      <Popup 
                        wide
                        content={<p>
                          <Trans i18nKey="comp.addPcbModal.popup.quantity">
                          Enter a quantity (multiplier) of PCB's produced each time you create a PCB. This should normally be 1, unless you require several copies of the same PCB for producing your BOM project.<br/><br/><i>Example:</i> An audio amplifier may require 2 of the same PCB's, one for each left/right channel each time you produce the entire assembly.
                          </Trans>
                        </p>} 
                        trigger={<div>{t('label.quantity', "Quantity")}</div>} 
                      />
                    </Table.HeaderCell>
                    <Table.HeaderCell width={1}><Popup wide content={<p>{t('page.project.popup.totalConsumed', "The total number of parts consumed")}</p>} trigger={<div>{t('label.consumed', "Consumed")}</div>} /></Table.HeaderCell>
                    <Table.HeaderCell width={2}>{t('label.cost', "Cost")}</Table.HeaderCell>
                    <Table.HeaderCell>{t('label.serial', "Serial")}</Table.HeaderCell>
                    <Table.HeaderCell width={1}></Table.HeaderCell>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {history && history.pcbs && history.pcbs.map((hpcb, key) => (
                    <Table.Row key={key} onMouseOver={() => setImageIndex(key)}>
                      <Table.Cell>{hpcb.pcb.name}</Table.Cell>
                      <Table.Cell><Input className="inline-editable" transparent type='text' name='pcbQuantity' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, hpcb)} onChange={(e, control) => handleInlineChange(e, control, hpcb)} value={hpcb.pcbQuantity || ''} fluid /></Table.Cell>
                      <Table.Cell>{hpcb.partsConsumed}</Table.Cell>
                      <Table.Cell><Input label={getCurrencySymbol(hpcb.currency || "USD")} className="inline-editable" transparent type='text' name='pcbCost' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, hpcb)} onChange={(e, control) => handleInlineChange(e, control, hpcb)} value={hpcb.pcbCost || ''} fluid /></Table.Cell>
                      <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='serialNumber' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, hpcb)} onChange={(e, control) => handleInlineChange(e, control, hpcb)} value={hpcb.serialNumber || ''} fluid /></Table.Cell>
                      <Table.Cell><Button circular size='mini' icon='delete' title='Delete history record' onClick={e => confirmDeletePcbOpen(e, hpcb)} /></Table.Cell>
                    </Table.Row>
                  ))}
                </Table.Body>
              </Table>
            </Form>
          </Modal.Description>
        </Modal.Content>
        <Modal.Actions>
          <Button primary onClick={handleModalClose}>{t('button.close', "Close")}</Button>
        </Modal.Actions>
      </Modal>
    </div>
  );
}

PcbHistoryModal.propTypes = {
  /** The history object */
  history: PropTypes.object,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

PcbHistoryModal.defaultProps = {
  isOpen: false
};
