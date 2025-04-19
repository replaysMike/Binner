import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from "react-i18next";
import { Icon, Button, Modal } from "semantic-ui-react";
import PropTypes from "prop-types";
import { Converter } from "showdown";
import "./VersionBanner.css";

/**
 * Displays a top banner when a new Binner version is available
 */
export function VersionBanner({ isOpen = false, version }) {
	const { t } = useTranslation();
	const [_isOpen, setIsOpen] = useState(false);
	const [_version, setVersion] = useState('');
	const [description, setDescription] = useState('');
	const [descriptionObject, setDescriptionObject] = useState(null);
	const [url, setUrl] = useState('');
	const [isReleaseNotesOpen, setIsReleaseNotesOpen] = useState(false);

	useEffect(() => {
		setIsOpen(isOpen);
	}, [isOpen]);

	useEffect(() => {
		setVersion(version?.version);
		setDescription(version?.description);
		setUrl(version?.url);
		var converter = new Converter({optionKey: 'value', completeHTMLDocument: false, ghMentions: true, ghCompatibleHeaderId: true, ghCodeBlocks: true, tasklists: true, tables: true, strikethrough: true, emoji: true, openLinksInNewWindow: true, simplifiedAutoLink: true });
		let desc = version?.description;
		var html = converter.makeHtml(desc);
		setDescriptionObject(html);
	}, [version]);

	const handleViewReleaseNotesModalOpen = (e) => {
		setIsReleaseNotesOpen(true);
	};

	const handleReleaseNotesModalClose = (e) => {
		setIsReleaseNotesOpen(false);
	};

	const handleView = (e) => {
		e.preventDefault();
		e.stopPropagation();
		window.open(url, "_blank");
	};

	const handleSkip = (e) => {
		e.preventDefault();
		e.stopPropagation();
		localStorage.setItem("skipVersion", _version);
		setIsOpen(false);
	};

	return (
		<div className={`version-banner ${_isOpen ? 'open' : ''}`}>
			<div>
				<Icon name="close" style={{marginRight: '20px', cursor: 'pointer'}} onClick={handleSkip} />
				<Trans i18nKey="notification.versionBanner.newVersion" version={_version}>
					A new version of Binner <b>{{version: _version}}</b> is available!
				</Trans>
			</div>
			<div>
        <Button primary onClick={handleViewReleaseNotesModalOpen} size="tiny">{t('notification.versionBanner.releaseNotes', "Release Notes")}</Button>
        <Button primary onClick={handleView} size="tiny">{t('notification.versionBanner.view', "View")}</Button>
        <Button onClick={handleSkip} size="tiny">{t('notification.versionBanner.skip', "Skip")}</Button>
      </div>
			<Modal centered open={isReleaseNotesOpen || false} onClose={handleReleaseNotesModalClose} className="release-notes">
        <Modal.Header>{t('notification.versionBanner.releaseNotes', "Release Notes")}</Modal.Header>
				<Modal.Description><p style={{padding: '0 10px', marginLeft: '10px'}}>{_version}</p></Modal.Description>
        <Modal.Content scrolling>
					<div dangerouslySetInnerHTML={{__html: descriptionObject}} />
        </Modal.Content>
        <Modal.Actions>
					<Button primary onClick={handleView}>{t('notification.versionBanner.view', "View")} {_version}</Button>
					<Button onClick={handleReleaseNotesModalClose}>{t('notification.versionBanner.close', "Close")}</Button>
        </Modal.Actions>
      </Modal>
		</div>
		);
};

VersionBanner.propTypes = {
  /** Version info */
  version: PropTypes.object,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
