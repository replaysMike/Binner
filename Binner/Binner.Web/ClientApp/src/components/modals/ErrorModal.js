import { Modal } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
import React, { useState, useEffect } from "react";
import reactStringReplace from "react-string-replace";
import "./ErrorModal.css";

export default function ErrorModal(props) {
  const { t } = useTranslation();
  const [context, setContext] = useState(null);

  useEffect(() => {
    if (props.context) {
      setContext(props.context);
    }
  }, [props.context]);

  const handleCloseErrorModal = () => {
    window.showErrorWindow();
  };

  const hasErrorMessage = (message) => {
    return message !== undefined && message.length > 0;
  };

  if (context) {
		const errorMessages = [];
		if (context.errorMessage && context.errorMessage.includes("\n")){
			const errorMessagesArray = context.errorMessage.split('\n');
			for(let i = 0; i < errorMessagesArray.length; i++)
				errorMessages.push(errorMessagesArray[i]);
		} else {
			errorMessages.push(context.errorMessage);
		}
	
    return (
      <Modal centered open={hasErrorMessage(context.errorMessage)} onClose={handleCloseErrorModal} className="error-modal" dimmer="blurring">
        <Modal.Header>{context.modalTitle}</Modal.Header>
        <Modal.Content>
          <Modal.Description>
            <div>
              {context.header && <h1>{context.header}</h1>}
              {errorMessages && errorMessages.length > 0 && <div className="message">
								{errorMessages.map((msg, key) => (
									<div key={key}>{msg}</div>
								))}
							</div>}
            </div>
            <hr />
            <footer>
              {context.url && (
                <h2>
                  {t("label.apiEndpoint", "Api Endpoint")}: {context.url}
                </h2>
              )}
              {context.stackTrace && <p>{context.stackTrace}</p>}
            </footer>
          </Modal.Description>
        </Modal.Content>
      </Modal>
    );
  }
  return <></>;
}
