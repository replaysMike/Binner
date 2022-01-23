
import { Modal } from 'semantic-ui-react';
import React, { Component } from 'react';
import { ErrorContext } from '../common/ErrorContext';

export default class ErrorModal extends Component {
	static contextType = ErrorContext;
	constructor(props) {
		super(props);
    	this.handleCloseErrorModal = this.handleCloseErrorModal.bind(this);
	}

  	handleCloseErrorModal() {
		window.showErrorWindow();
	}

	hasErrorMessage(message){
		return message !== undefined && message.length > 0;
	}

	render() {
		return (
		<ErrorContext.Consumer>
			{({modalTitle, url, header, errorMessage, stackTrace}) => (
			<Modal centered open={this.hasErrorMessage(errorMessage)} onClose={this.handleCloseErrorModal}>
			<Modal.Header>{modalTitle}</Modal.Header>
			<Modal.Content scrolling dimmer='blurring'>
				<Modal.Description>
				{header && <h1>{header}</h1>}
				{errorMessage && <h3>{errorMessage}</h3>}
				<hr/>
				{url && <h2>Api Endpoint: {url}</h2>}
				{stackTrace && <p>{stackTrace}</p>}
				</Modal.Description>
			</Modal.Content>
			</Modal>
			)}
		</ErrorContext.Consumer>);
	}
};
ErrorModal.contextType = ErrorContext;