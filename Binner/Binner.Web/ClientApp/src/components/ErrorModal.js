import { Modal } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
import React, { useState, useEffect } from "react";
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
    return (
      <Modal centered open={hasErrorMessage(context.errorMessage)} onClose={handleCloseErrorModal} className="error-modal">
        <Modal.Header>{context.modalTitle}</Modal.Header>
        <Modal.Content dimmer="blurring">
          <Modal.Description>
            <div>
              {context.header && <h1>{context.header}</h1>}
              {context.errorMessage && <h3>{context.errorMessage}</h3>}
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
