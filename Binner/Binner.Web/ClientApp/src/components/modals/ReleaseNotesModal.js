import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from "react-i18next";
import { Icon, Button, Modal } from "semantic-ui-react";
import PropTypes from "prop-types";
import { Converter } from "showdown";
import "./ReleaseNotesModal.css";

/**
 * Displays release notes with markdown support
 */
export function ReleaseNotesModal({ isOpen, text, description, header, actionText, onAction, onClose }) {
  const { t } = useTranslation();
  const [html, setHtml] = useState('');

  useEffect(() => {
    var converter = new Converter({optionKey: 'value', completeHTMLDocument: false, ghMentions: true, ghCompatibleHeaderId: true, ghCodeBlocks: true, tasklists: true, tables: true, strikethrough: true, emoji: true, openLinksInNewWindow: true, simplifiedAutoLink: true });
    var html = converter.makeHtml(text);
    setHtml(html);
  }, [text]);

  const handleClose = (e, control) => {
    if (onClose) onClose(e, control);
  };

  const handleAction = (e, control) => {
    if (onAction) onAction(e, control);
  };

  return (
  <Modal centered open={isOpen || false} onClose={handleClose} className="release-notes">
    <Modal.Header>{header}</Modal.Header>
    <Modal.Description><p style={{padding: '0 10px', marginLeft: '10px'}}>{description}</p></Modal.Description>
    <Modal.Content scrolling>
        <div dangerouslySetInnerHTML={{ __html: html }} />
    </Modal.Content>
    <Modal.Actions>
      {actionText && <Button primary onClick={handleAction}>{actionText}</Button>}
      <Button onClick={handleClose}>{t('button.close', "Close")}</Button>
    </Modal.Actions>
  </Modal>);
};

ReleaseNotesModal.propTypes = {
  /** The content to display */
  text: PropTypes.any,
  description: PropTypes.string,
  header: PropTypes.string,
  actionText: PropTypes.any,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  onAction: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
