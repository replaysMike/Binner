import React, { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import TextSnippet from "@mui/icons-material/TextSnippet";
import { Table, Modal, Popup, Button, Icon } from "semantic-ui-react";
import _ from "underscore";

/**
 * Displays available part parametric values
 */
export function PartParametricsModal({part, isOpen, onClose}) {
  const { t } = useTranslation();
  const [partModalOpen, setPartModalOpen] = useState(isOpen);

  useEffect(() => {
    setPartModalOpen(isOpen);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setPartModalOpen(false);
    if (onClose) onClose(e, part);
  };

  const render = (part) => {
    return (
      <Table compact celled selectable striped size="small">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.name', "Name")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.value', "Value")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.units', "Units")}</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {part?.parametrics?.length > 0 
            ? part?.parametrics?.map((p, key) => (<Table.Row key={key}>
              <Table.Cell>{p.name}</Table.Cell>
              <Table.Cell>{p.value}</Table.Cell>
              <Table.Cell>{p.units}</Table.Cell>
            </Table.Row>)) 
            : <Table.Row><Table.Cell colSpan={3} textAlign="center">{t('comp.partParametrics.noResults', "No parametrics available.")}</Table.Cell></Table.Row>}
        </Table.Body>
      </Table>
    );
  };

  const parametricsList = render(part);

  return (<div style={{ display: 'inline-block' }}>
    <Modal
      centered
      open={partModalOpen}
      onClose={handleModalClose}
    >
      <Modal.Header><TextSnippet className="technical-specs" /> {t('comp.partParametrics.title', "Part Parametrics")}</Modal.Header>
      <Modal.Description><p>{t('comp.partParametrics.description', "Listed below are all of the electrical characteristics (parametrics) for the part.")}</p></Modal.Description>
      <Modal.Content scrolling>
        <Modal.Description>{parametricsList}</Modal.Description>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
      </Modal.Actions>
    </Modal>
  </div>);
};

PartParametricsModal.propTypes = {
  /** Event handler to set if the modal is open */
  isOpen: PropTypes.bool,
  /** The part object */
  part: PropTypes.object,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
};