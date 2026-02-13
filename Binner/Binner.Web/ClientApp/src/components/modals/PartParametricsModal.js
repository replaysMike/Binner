import React, { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import TextSnippet from "@mui/icons-material/TextSnippet";
import { Table, Modal, Popup, Button, Icon, Form, Dropdown } from "semantic-ui-react";
import { ParametricUnits } from "../../common/ParametricUnits";
import { GetTypeMeta, GetTypeName, GetAdvancedTypeDropdown } from "../../common/Types";
import _ from "underscore";

/**
 * Displays available part parametric values
 */
export function PartParametricsModal({ part, isOpen, onClose, onAdd, onDelete, onChange }) {
  const { t } = useTranslation();
  const [partModalOpen, setPartModalOpen] = useState(isOpen);
  const [rows, setRows] = useState(part?.parametrics || []);
  const [newId, setNewId] = useState(0);
  const unitOptions = GetAdvancedTypeDropdown(ParametricUnits, true);

  useEffect(() => {
    setPartModalOpen(isOpen);
    setNewId(0);
  }, [isOpen]);

  useEffect(() => {
    setRows(part?.parametrics || []);
  }, [part]);

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
      value: '',
      valueNumber: '',
      units: 0,
      edit: true,
      partParametricId: virtualId
    };
    if (onAdd) onAdd(e, control, row);
  };

  const handleChange = (e, control, parametric) => {
    parametric[control.name] = control.value;
    if (onChange) onChange(e, control, parametric);
  };

  const handleEdit = (e, control, parametric) => {
    parametric.edit = !parametric.edit;
    if (onChange) onChange(e, control, parametric);
  };

  const handleDelete = (e, control, parametric) => {
    if (onDelete) onDelete(e, control, parametric);
  }

  const render = () => {
    return (
      <Form>
        <div style={{float: 'right', marginBottom: '5px'}}>
          <Button primary type="button" onClick={handleAdd} size='tiny'><Icon name="plus" /> Add</Button>
        </div>
        <Table compact celled selectable striped size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>{t('label.name', "Name")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.value', "Value")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.valueNumber', "Number")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.units', "Units")}</Table.HeaderCell>
              <Table.HeaderCell></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {rows?.length > 0 
              ? rows.map((parametric, key) => (
                <Table.Row key={key}>
                  <Table.Cell width={4}>
                    {parametric.edit 
                      ? <Form.Input name="name" value={parametric.name} onChange={(e, control) => handleChange(e, control, parametric)} /> 
                      : parametric.name
                      }
                    </Table.Cell>
                  <Table.Cell width={3} className={!parametric.edit && (parametric.value === '-' || parametric.value === '') ? 'disabled' : ''}>
                    {parametric.edit 
                      ? <Form.Input name="value" value={parametric.value} onChange={(e, control) => handleChange(e, control, parametric)} />
                      : parametric.value
                    }
                  </Table.Cell>
                  <Table.Cell width={2} className={!parametric.edit && (parametric.valueNumber === 0 || parametric.value === '-') ? 'disabled' : ''}>
                    {parametric.edit 
                      ? <Form.Input name="valueNumber" value={parametric.valueNumber} onChange={(e, control) => handleChange(e, control, parametric)} />
                    : parametric.valueNumber === 0 ? '' : parametric.valueNumber
                    }
                  </Table.Cell>
                  <Table.Cell width={5}>
                    {parametric.edit 
                      ? <Dropdown name="units" selection fluid options={unitOptions} value={parametric.units} onChange={(e, control) => handleChange(e, control, parametric)} /> 
                      : parametric.valueNumber !== 0 
                        ? <Popup content={GetTypeName(ParametricUnits, parametric.units)} trigger={<span>{GetTypeMeta(ParametricUnits, parametric.units, "value")?.units}</span>} />
                        : <></>
                    }
                  </Table.Cell>
                  <Table.Cell width={2}>
                    <Button name="edit" icon="edit" size='tiny' title={t('label.edit', "Edit")} onClick={(e, control) => handleEdit(e, control, parametric)} />
                    <Button name="delete" icon="delete" size='tiny' title={t('label.delete', "Delete")} onClick={(e, control) => handleDelete(e, control, parametric)} />
                  </Table.Cell>
                </Table.Row>)) 
              : <Table.Row>
                  <Table.Cell colSpan={5} textAlign="center">{t('comp.partParametrics.noResults', "No parametrics available.")}
                  </Table.Cell>
                </Table.Row>}
          </Table.Body>
        </Table>
      </Form>
    );
  };

  const parametricsList = render();

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
  onAdd: PropTypes.func,
  onDelete: PropTypes.func,
  onChange: PropTypes.func
};