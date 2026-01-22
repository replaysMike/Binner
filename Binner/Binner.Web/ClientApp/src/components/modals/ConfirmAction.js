import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import { Modal, Button, Checkbox } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
const soundDiscard = new Audio('/audio/discard.mp3');
import "./ConfirmAction.css";

export default function ConfirmAction(props) {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(props.open);
  const [doNotAskAgain, setDoNotAskAgain] = useState(false);

  useEffect(() => {
    const isDoNotAskAgain = localStorage.getItem(`doNotAskAgain-${props.name}`);
    if (isDoNotAskAgain === true || isDoNotAskAgain === 'true') {
      // confirm the choice automatically
      setIsOpen(false);
      if (props.open && props.onConfirm) props.onConfirm();
    } else {
      if (props.open && props.enableSound) soundDiscard.play();
      setIsOpen(props.open);
    }
  }, [props.open]);

  const handleChange = (e, control) => {
    setDoNotAskAgain(control.checked);
  };

  const handleClose = (e) => {
    if (props.onClose) props.onClose(e);
  };

  const handleCancel = (e) => {
    setDoNotAskAgain(false);
    if (props.onCancel) props.onCancel(e);
  };

  const handleConfirm = (e) => {
    if (doNotAskAgain) {
      localStorage.setItem(`doNotAskAgain-${props.name}`, doNotAskAgain);
    }
    if (props.onConfirm) props.onConfirm(e);
  };

  return (
    <Modal centered open={isOpen} onClose={handleClose} className="confirm-modal" closeOnDimmerClick={true} closeOnDocumentClick={true}>
      <Modal.Header>{props.header}</Modal.Header>
      <Modal.Content>
        {props.content}
      </Modal.Content>
      <Modal.Actions>
        {props.dontAskAgain ? <Checkbox toggle name="doNotAskAgain" label={t('confirm.doNotAskAgain', "Do not ask again")} className="left small" checked={doNotAskAgain} onChange={handleChange}></Checkbox> : <></>}
        {props.cancelButton ? <Button onClick={handleCancel}>{props.cancelButton}</Button> : <Button onClick={handleCancel}>{t('button.cancel', "Cancel")}</Button>}
        {props.confirmButton ? <Button primary onClick={handleConfirm}>{props.confirmButton}</Button> : <Button primary onClick={handleConfirm}>{t('button.ok', "Ok")}</Button>}
      </Modal.Actions>
    </Modal>);
}

ConfirmAction.propTypes = {
  /** Event handler when cancel is clicked */
  onCancel: PropTypes.func,
  /** Event handler when confirm is clicked */
  onConfirm: PropTypes.func,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  open: PropTypes.bool,
  /** Cancel button */
  cancelButton: PropTypes.string,
  /** Confirm button */
  confirmButton: PropTypes.string,
  /** True to offer choice to not show again */
  dontAskAgain: PropTypes.bool,
  /** True to enable sounds */
  enableSound: PropTypes.bool,
  header: PropTypes.any,
  content: PropTypes.any,
  name: PropTypes.string,
};