import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import PropTypes from "prop-types";
import { Modal, Button } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
import "./BarcodeTroubleshootingModal.css";

export default function BarcodeTroubleshootingModal(props) {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(props.open);

  useEffect(() => {
    setIsOpen(props.open);
  }, [props.open]);

  const handleClose = (e) => {
    if (props.onClose) props.onClose(e);
  };

  return (
    <Modal centered open={isOpen} onClose={handleClose} className="barcodetroubleshooting-modal" closeOnDimmerClick={true} closeOnDocumentClick={true}>
      <Modal.Header>Barcode Scanner Troubleshooting</Modal.Header>
      <Modal.Content scrolling>
        <h3>Overview</h3>
        <p>
          Barcode scanners can be troublesome sometimes and even more so when using them in a web browser.<br/>
          They act as general keyboard input devices and are hard to distinguish from a regular keyboard depending on the type of barcode being scanned.
        </p>

        <h3>Common Troubleshooting Steps</h3>
        <p>
          Here are some common <i>troubleshooting steps</i> to try if you are having issues with barcode scanning:
        </p>

        <div>
          <ul>
            <li> Make sure the scanner is outputting the same value every time a barcode is scanned (use the <Link to="/tools/barcodescanner">Barcode Scanner Test Tool</Link>)</li>
            <li> Ensure that your input focus is on the correct window so the keyboard input received (i.e. current focus isn&apos;t on the browser url or inspector)</li>
            <li> Try resetting your barcode scanner to factory default settings</li>
            <li> Check the symbology settings and disable any barcode types that you may not need. 2D DotMatrix barcodes work best, 1D barcodes only contain a part number.</li>
            <li> Try cleaning the lens as it may be smudged</li>
            <li> If you are having communication issues with a wireless scanner, try using it connected via USB.</li>
            <li> If using a wireless scanner ensure your barcode scanner is fully charged.</li>
          </ul>
        </div>

        <h3>Tested Barcode Scanners</h3>

        <div>
          <ul>
            <li>Tera HW0009 (most recommended budget option)</li>
            <li>Tera D5100</li>
            <li>Netum NT-1228BL</li>
            <li>Zebra Symbol DS2278-SR</li>
            <li>Zebra DS2208</li>
            <li>Trohestar NS-8103 (1-D barcodes only)</li>
          </ul>
        </div>

        <h3>Helpful links</h3>

        <ul>
          <li><a href="https://github.com/replaysMike/Binner/wiki/Barcode-Support" target="_blank" rel="noopener noreferrer">Binner Wiki - Barcode Support</a></li>
          <li><a href="https://tera-digital.com/blogs/barcodes/barcode-scanner-calibration" target="_blank" rel="noopener noreferrer">Tera Barcode Scanner Calibration</a></li>
          <li><a href="https://www.idprt.com/Blog/Barcode-Not-Scanning-Troubleshooting-Guide.html" target="_blank" rel="noopener noreferrer">IDPRT Barcode Troubleshooting Guide</a></li>
        </ul>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handleClose}>{t('button.ok', "Ok")}</Button>
      </Modal.Actions>
    </Modal>);
}

BarcodeTroubleshootingModal.propTypes = {
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  open: PropTypes.bool,
};