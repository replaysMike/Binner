import React, { useState, useEffect, useMemo } from "react";
import ReactDOMServer from "react-dom/server";
import { Icon, Button, Form, Modal, Popup, TextArea, Header, Confirm } from "semantic-ui-react";
import reactStringReplace from "react-string-replace";
import PropTypes from "prop-types";

export function VersionBanner(props) {
	const [isOpen, setIsOpen] = useState(false);
	const [version, setVersion] = useState('');
	const [description, setDescription] = useState('');
	const [descriptionObject, setDescriptionObject] = useState(null);
	const [url, setUrl] = useState('');
	const [isReleaseNotesOpen, setIsReleaseNotesOpen] = useState(false);

	useEffect(() => {
		setIsOpen(props.isOpen);
	}, [props.isOpen]);

	useEffect(() => {
		setVersion(props.version?.version);
		setDescription(props.version?.description);
		setUrl(props.version?.url);
		let desc = props.version?.description;
		desc = reactStringReplace(desc, /(\r\n|\r|\n)/g, (match, i) => (<br key={i}/>))
		desc = reactStringReplace(desc, "* ", (match, i) => (<span className="bullet" key={i}>&bull;</span>))
		desc = reactStringReplace(desc, "## What's Changed", (match, i) => (<h2 key={i}>What's Changed</h2>))
		setDescriptionObject(desc);
	}, [props.version]);

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
		localStorage.setItem("skipVersion", version);
		setIsOpen(false);
	};

	return (
		<>
		{isOpen && <div className="version-banner">
			<span>
				<Icon name="close" style={{marginRight: '20px', cursor: 'pointer'}} onClick={handleSkip} />
				A new version of Binner <b>{version}</b> is available!
			</span>
			<div style={{float: 'right'}}>
				<Button primary onClick={handleViewReleaseNotesModalOpen} size="tiny">Release Notes</Button>
				<Button primary onClick={handleView} size="tiny">View</Button>
				<Button onClick={handleSkip} size="tiny">Skip</Button>
			</div>
			<Modal centered open={isReleaseNotesOpen || false} onClose={handleReleaseNotesModalClose} className="release-notes">
        <Modal.Header>Release Notes</Modal.Header>
				<Modal.Description><p style={{padding: '0 10px', marginLeft: '10px'}}>{version}</p></Modal.Description>
        <Modal.Content scrolling>
					{descriptionObject}
        </Modal.Content>
        <Modal.Actions>
					<Button primary onClick={handleView}>View {version}</Button>
					<Button onClick={handleReleaseNotesModalClose}>Close</Button>
        </Modal.Actions>
      </Modal>
		</div>}
		</>);

};

VersionBanner.propTypes = {
  /** Version info */
  version: PropTypes.object,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};

VersionBanner.defaultProps = {
  isOpen: false
};