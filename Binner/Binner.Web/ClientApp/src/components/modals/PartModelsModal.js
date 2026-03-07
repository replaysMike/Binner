import { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import ViewInArIcon from '@mui/icons-material/ViewInAr';
import { Table, Modal, Button, Icon, Form, Popup } from "semantic-ui-react";
import _ from "underscore";
import { GetTypeName, GetReactSelectTypeDropdown } from "../../common/Types";
import { PartModelSources } from "../../common/PartModelSources";
import { PartModelTypes } from "../../common/PartModelTypes";
import Select from "react-select";

/**
 * Displays part CAD models available
 */
export function PartModelsModal({ part, isOpen, onClose, onAdd, onDelete, onChange }) {
  const { t } = useTranslation();
  const [partModalOpen, setPartModalOpen] = useState(isOpen);
  const [newId, setNewId] = useState(0);
  const modelTypeOptions = GetReactSelectTypeDropdown(PartModelTypes);
  const modelSourceOptions = GetReactSelectTypeDropdown(PartModelSources);

  useEffect(() => {
    setPartModalOpen(isOpen);
    setNewId(0);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setPartModalOpen(false);
    if (onClose) onClose(e, part);
  };

  const handleAdd = (e, control) => {
    const virtualId = newId - 1;
    setNewId(virtualId);
    const row = {
      partId: part?.partId || 0,
      name: '',
      modelType: PartModelTypes.Model3d,
      source: PartModelSources.UserUpload,
      edit: true,
      partModelId: virtualId,
      partNumberManufacturerModelId: virtualId
    };
    if (onAdd) onAdd(e, control, row);
  };

  const handleChange = (e, control, model) => {
    model[control.name] = control.value;
    switch(control.name) {
      case 'url':
        if (control.value.includes("snapeda.com") || control.value.includes("snapmagic.com"))
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.SnapMagic), model);
        else if (control.value.includes("ultralibrarian.com"))
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.UltraLibrarian), model);
        else if (control.value.includes("componentsearchengine.com"))
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.ComponentSearchEngine), model);
        else if (control.value.includes("3dfindit.com"))
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.ThreeDFindIt), model);
        else if (control.value.includes("binner.io"))
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.Binner), model);
        else
          handleSelectChange(e, 'source', _.find(modelSourceOptions, option => option.value === PartModelSources.Other), model);
        break;
    }
    if (onChange) onChange(e, control, model);
  };

  const handleSelectChange = (e, name, option, model) => {
    model[name] = option.value;
    if (onChange) onChange(e, { name, value: option.value }, model);
  };

  const handleEdit = (e, control, model) => {
    model.edit = !model.edit;
    if (onChange) onChange(e, control, model);
  };

  const handleDelete = (e, control, model) => {
    if (onDelete) onDelete(e, control, model);
  }

  const handleDownload = (e, model) => {
    e.preventDefault();
    if (model.url) window.open(model.url, '_blank');
  };

  const render = (part) => {
    return (
      <Form>
        <div style={{ float: 'right', marginBottom: '5px' }}>
          <Button primary type="button" onClick={handleAdd} size='tiny'><Icon name="plus" /> {t('button.add', "Add")}</Button>
        </div>
        <Table compact celled selectable striped size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>{t('label.namePackage', "Name / Package")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.modelType', "Model Type")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.source', "Source")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.download', "Download")}</Table.HeaderCell>
              <Table.HeaderCell></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {part?.models?.length > 0
              ? part?.models?.map((model, key) => (
                <Table.Row key={key}>
                  <Table.Cell>{model.edit 
                    ? <Form.Input name="name" value={model.name} onChange={(e, control) => handleChange(e, control, model)} /> 
                    : model.name}
                  </Table.Cell>
                  <Table.Cell width={2}>{model.edit 
                    ? <Popup 
                        content={<p>Specify the type of model, such as a 3d model, footprint layout or symbol object.</p>} 
                        trigger={<Select 
                          name="modelType" 
                          menuPortalTarget={document.body} 
                          options={modelTypeOptions} 
                          value={_.find(modelTypeOptions, option => option.value === model.modelType)}
                          onChange={(newVal, e) => handleSelectChange(e, 'modelType', newVal, model)} 
                          styles={{ menuPortal: base => ({ ...base, zIndex: 9999 }) }} 
                        />} 
                      /> 
                    : GetTypeName(PartModelTypes, model.modelType)}
                  </Table.Cell>
                  <Table.Cell width={2}>{model.edit 
                    ? <Popup 
                        content={<p>Specify the source where the model comes from. The source will attempt to be autodetected when available.</p>} 
                        trigger={<Select 
                          name="source" 
                          menuPortalTarget={document.body} 
                          options={modelSourceOptions} 
                          value={_.find(modelSourceOptions, option => option.value === model.source)}
                          onChange={(newVal, e) => handleSelectChange(e, 'source', newVal, model)} 
                          styles={{ menuPortal: base => ({ ...base, zIndex: 9999 }) }} 
                        />} 
                      />
                    : GetTypeName(PartModelSources, model.source) }
                  </Table.Cell>
                  <Table.Cell width={5} textAlign="center">
                    { model.edit
                      ? <Popup content={<p>Enter the URL where the model comes from</p>} trigger={<Form.Input name="url" placeholder="https://www.snapeda.com/parts/LM358DT" value={model.url} onChange={(e, control) => handleChange(e, control, model)} />} /> 
                      : <Button size="tiny" onClick={e => handleDownload(e, model)}><Icon name="download" /> {t('button.download', "Download")}</Button>
                    }
                  </Table.Cell>
                  <Table.Cell width={2}>
                    <Button name="edit" icon="edit" size='tiny' title={t('label.edit', "Edit")} disabled={model.edit} onClick={(e, control) => handleEdit(e, control, model)} />
                    <Button name="delete" icon="delete" size='tiny' title={t('label.delete', "Delete")} onClick={(e, control) => handleDelete(e, control, model)} />
                  </Table.Cell>
                </Table.Row>))
              : <Table.Row><Table.Cell colSpan={5} textAlign="center">{t('comp.partModels.noResults', "No CAD models available.")}</Table.Cell></Table.Row>}
          </Table.Body>
        </Table>
      </Form>
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
  onAdd: PropTypes.func,
  onDelete: PropTypes.func,
  onChange: PropTypes.func
};