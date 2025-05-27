import React, { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import BeenhereIcon from '@mui/icons-material/Beenhere';
import { Table, Modal, Popup, Form, Button, Icon } from "semantic-ui-react";
import ClearableInput from "../ClearableInput";
import _ from "underscore";

/**
 * Displays available part compliance values
 */
export function PartComplianceModal({ part, isOpen, onClose, onChange }) {
  const { t } = useTranslation();
  const [partModalOpen, setPartModalOpen] = useState(isOpen);

  useEffect(() => {
    setPartModalOpen(isOpen);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setPartModalOpen(false);
    if (onClose) onClose(e, part);
  };

  return (<div style={{ display: 'inline-block' }}>
    <Modal
      centered
      open={partModalOpen}
      onClose={handleModalClose}
    >
      <Modal.Header><BeenhereIcon className="compliance" /> {t('comp.partCompliance.title', "Part Compliance")}</Modal.Header>
      <Modal.Description><p>{t('comp.partCompliance.description', "Listed below are all of the compliance standards for the part.")}</p></Modal.Description>
      <Modal.Content scrolling>
        <Modal.Description>
          <Form>
            <Form.Group>
              <Form.Field width={3}>
                <label>{t('label.rohsStatus', "RoHS Status")}</label>
                <Popup
                  wide
                  content={<p>Restriction of Hazardous Substances (RoHS) compliance refers to an EU directive that restricts the use of hazardous substances. Typically includes <i>lead, mercury, cadmium, hexavalent chromium, polybrominated biphenyls and polybrominated diphenyl ethers</i>.</p>}
                  trigger={<div><ClearableInput placeholder='Compliant' value={part.rohsStatus || ''} onChange={onChange} name='rohsStatus' /></div>}
                />
              </Form.Field>
              <Form.Field width={3}>
                <label>{t('label.reachStatus', "Reach Status")}</label>
                <Popup
                  wide
                  content={<p>For products shipping to the EU, indicates which substance of very high concern (SVHC) a product has and may affect its export. Possible values: REACH Unaffected (no SVHC), REACH Affected (contains SVHC), Vendor Undefined (not yet classified).</p>}
                  trigger={<div><ClearableInput placeholder='Unaffected' value={part.reachStatus || ''} onChange={onChange} name='reachStatus' /></div>}
                />
              </Form.Field>
              <Form.Field width={5}>
                <label>{t('label.exportControlClassNumber', "Export Control Class (ECCN)")}</label>
                <Popup
                  wide
                  content={<p>DigiKey uses the Export Control Classification Number (ECCN) to categorize products for export control purposes. This number helps determine if an item requires a license for export. For example, some products, like high-end FPGAs and microwave RF devices, may have ECCNs other than EAR99.</p>}
                  trigger={<div><ClearableInput placeholder='EAR99' value={part.exportControlClassNumber || ''} onChange={onChange} name='exportControlClassNumber' /></div>}
                />
              </Form.Field>
              <Form.Field width={5}>
                <label>{t('label.htsusCode', "Harmonized Tariff (HTS)")}</label>
                <Popup
                  wide
                  content={<p>Harmonized Tariff Schedule (HTS) is a 10 digit code used to classify imported goods in the US, determing the applicable customs duties. It's based on the Harmonized System (HS) developed by the WCO. First six digits is the HS code, followed by two digits for the US subheading and two digits for statistical suffix.</p>}
                  trigger={<div><ClearableInput placeholder='8542.33.0001' value={part.htsusCode || ''} onChange={onChange} name='htsusCode' /></div>}
                />
              </Form.Field>
            </Form.Group>
          </Form>
        </Modal.Description>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
      </Modal.Actions>
    </Modal>
  </div>);
};

PartComplianceModal.propTypes = {
  /** Event handler to set if the modal is open */
  isOpen: PropTypes.bool,
  /** The part object */
  part: PropTypes.object,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  onChange: PropTypes.func
};