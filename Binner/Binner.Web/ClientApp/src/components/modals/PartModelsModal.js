import React, { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import ViewInArIcon from '@mui/icons-material/ViewInAr';
import { Table, Modal, Button, Icon } from "semantic-ui-react";
import _ from "underscore";
import { GetTypeName } from "../../common/Types";
import { PartModelSources } from "../../common/PartModelSources";
import { PartModelTypes } from "../../common/PartModelTypes";

/**
 * Displays part CAD models available
 */
export function PartModelsModal({part, isOpen, onClose}) {
  const { t } = useTranslation();
  const [partModalOpen, setPartModalOpen] = useState(isOpen);

  useEffect(() => {
    setPartModalOpen(isOpen);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setPartModalOpen(false);
    if (onClose) onClose(e, part);
  };

  const handleDownload = (e, model) => {
    e.preventDefault();
  };

  const render = (part) => {
    return (
      <Table compact celled selectable striped size="small">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.name', "Name")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.modelType', "ModelType")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.source', "Source")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.download', "Download")}</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {part?.models?.length > 0 
            ? part?.models?.map((model, key) => (<Table.Row key={key}>
              <Table.Cell>{model.name}</Table.Cell>
              <Table.Cell width={2}>{GetTypeName(PartModelTypes, model.modelType)}</Table.Cell>
              <Table.Cell width={2}>{GetTypeName(PartModelSources, model.source)}</Table.Cell>
              <Table.Cell width={3} textAlign="center"><Button size="tiny" onClick={e => handleDownload(e, model)}><Icon name="download" /> {t('button.download', "Download")}</Button></Table.Cell>
            </Table.Row>)) 
            : <Table.Row><Table.Cell colSpan={4} textAlign="center">{t('comp.partModels.noResults', "No CAD models available.")}</Table.Cell></Table.Row>}
        </Table.Body>
      </Table>
    );
  };

  const modelsList = render(part);

  return (<div style={{ display: 'inline-block' }}>
    <Modal
      centered
      open={partModalOpen}
      onClose={handleModalClose}
    >
      <Modal.Header><ViewInArIcon className="cadModels" /> {t('comp.partModels.title', "Part Models")}</Modal.Header>
      <Modal.Description><p>{t('comp.partModels.description', "Listed below are the CAD model downloads available for this part.")}</p></Modal.Description>
      <Modal.Content scrolling>
        <Modal.Description>{modelsList}</Modal.Description>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
      </Modal.Actions>
    </Modal>
  </div>);
};

PartModelsModal.propTypes = {
  /** Event handler to set if the modal is open */
  isOpen: PropTypes.bool,
  /** The part object */
  part: PropTypes.object,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
};