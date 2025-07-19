import React, { useState, useEffect } from "react";
import { useTranslation, Trans } from "react-i18next";
import { Icon, Button, Modal } from "semantic-ui-react";
import PropTypes from "prop-types";
import { Converter } from "showdown";
import "./VersionBanner.css";
import { ReleaseNotesModal } from "./modals/ReleaseNotesModal";

/**
 * Displays a top banner when a new Binner version is available
 */
export function VersionBanner({ isOpen = false, version }) {
	const { t } = useTranslation();
	const [_isOpen, setIsOpen] = useState(false);
	const [_version, setVersion] = useState('');
	const [html, setHtml] = useState('');
	const [url, setUrl] = useState('');
	const [isReleaseNotesOpen, setIsReleaseNotesOpen] = useState(false);

	useEffect(() => {
		setIsOpen(isOpen);
	}, [isOpen]);

	useEffect(() => {
		setVersion(version?.version);
		setHtml(version?.description);
		setUrl(version?.url);
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
      <ReleaseNotesModal 
        header={t('notification.versionBanner.releaseNotes', "Release Notes")}
        description={_version}
        text={html}
        actionText={<span>{t('notification.versionBanner.view', "View")} {_version}</span>}
        isOpen={isReleaseNotesOpen || false}
        onClose={handleReleaseNotesModalClose}
        onAction={handleView}
      />
		</div>
		);
};

VersionBanner.propTypes = {
  /** Version info */
  version: PropTypes.object,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
