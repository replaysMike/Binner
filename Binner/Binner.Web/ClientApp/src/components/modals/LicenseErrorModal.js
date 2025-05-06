import { Modal } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
import React, { useState, useEffect } from "react";
import { Image, Icon } from "semantic-ui-react";
import "./LicensingErrorModal.css";

export default function LicenseErrorModal(props) {
  const { t } = useTranslation();
  const [context, setContext] = useState(null);

  useEffect(() => {
    if (props.context) {
      setContext(props.context);
    }
  }, [props.context]);

  const handleCloseErrorModal = () => {
    if (window?.showLicenseErrorWindow) window.showLicenseErrorWindow();
  };

  const hasErrorMessage = (message) => {
    return message !== undefined && message.length > 0;
  };

  if (context) {
    return (
      <Modal centered open={hasErrorMessage(context.errorMessage)} onClose={handleCloseErrorModal} className="licensing-modal" dimmer="blurring">
        <Modal.Header>{context.modalTitle}</Modal.Header>
        <Modal.Content scrolling image>
          <Image size="medium" src="/image/limited.png" wrapped />
          <Modal.Description>
            <div>
              {context.header && <h1>{context.header}</h1>}
              {context.errorMessage && <h3>{context.errorMessage}</h3>}
            </div>
            <footer>
              <hr />
              <p>
                Binner open-source includes most features for free, but some are limited.
                <br />
                If you require higher limits please visit <Icon name="cloud" color="blue" />
                <a href="https://binner.io/licensing" target="_blank" rel="noreferrer">
                  Binner Cloud
                </a> and obtain a license that meets your needs.
              </p>
            </footer>
          </Modal.Description>
        </Modal.Content>
      </Modal>
    );
  }
  return <></>;
}
