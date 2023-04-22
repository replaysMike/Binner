
import { Modal } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import React, { useContext } from 'react';
import { ErrorContext } from '../common/ErrorContext';
import { Image, Icon } from "semantic-ui-react";
import "./LicensingErrorModal.css";

export default function LicenseErrorModal (props) {
	const { t } = useTranslation();
	const context = useContext(ErrorContext)

  const handleCloseErrorModal = () => {
		console.log('closing license error');
		window.showLicenseErrorWindow();
	};

	const hasErrorMessage = (message) => {
		return message !== undefined && message.length > 0;
	};

	return (
	<ErrorContext.Consumer>
		{({modalTitle, url, header, errorMessage, stackTrace}) => (
		<Modal centered open={hasErrorMessage(errorMessage)} onClose={handleCloseErrorModal} className="licensing-modal">
		<Modal.Header>{modalTitle}</Modal.Header>
		<Modal.Content scrolling image dimmer='blurring'>
			<Image size="medium" src="/image/limited.png" wrapped />
			<Modal.Description>
				<div>
					{header && <h1>{header}</h1>}
					{errorMessage && <h3>{errorMessage}</h3>}
				</div>
				<footer>
					<hr />
					<p>
						Binner open-source includes most features for free, but some are limited.<br/>
						If you require higher limits please visit <Icon name="cloud" color="blue" /><a href="https://binner.io/licensing" target="_blank" rel="noreferrer">Binner Cloud</a> and obtain a license that meets your needs.
					</p>
				</footer>
			</Modal.Description>
		</Modal.Content>
		</Modal>
		)}
	</ErrorContext.Consumer>);
}