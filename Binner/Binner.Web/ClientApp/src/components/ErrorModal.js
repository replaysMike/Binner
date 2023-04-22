
import { Modal } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import React, { useContext } from 'react';
import { ErrorContext } from '../common/ErrorContext';

export default function ErrorModal (props) {
	const { t } = useTranslation();
	const context = useContext(ErrorContext)

  const handleCloseErrorModal = () => {
		window.showErrorWindow();
	};

	const hasErrorMessage = (message) => {
		return message !== undefined && message.length > 0;
	};

	return (
	<ErrorContext.Consumer>
		{({modalTitle, url, header, errorMessage, stackTrace}) => (
		<Modal centered open={hasErrorMessage(errorMessage)} onClose={handleCloseErrorModal} className="error-modal">
		<Modal.Header>{modalTitle}</Modal.Header>
		<Modal.Content scrolling dimmer='blurring'>
			<Modal.Description>
				<div>
					{header && <h1>{header}</h1>}
					{errorMessage && <h3>{errorMessage}</h3>}
				</div>
				<hr />
				<footer>
					{url && <h2>{t('label.apiEndpoint', "Api Endpoint")}: {url}</h2>}
					{stackTrace && <p>{stackTrace}</p>}
				</footer>
			</Modal.Description>
		</Modal.Content>
		</Modal>
		)}
	</ErrorContext.Consumer>);
}