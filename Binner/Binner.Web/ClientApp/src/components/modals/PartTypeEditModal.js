import { useEffect, useState } from "react";
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import { Table, Modal, Popup, Button, Icon, Form, Dropdown } from "semantic-ui-react";
import { getIcon } from "../../common/partTypes";
import { fetchApi } from "../../common/fetchApi";
import { toast } from "react-toastify";
import _ from "underscore";
import isSvg from "is-svg";
import {
  CableIcon,
  CapacitorIcon,
  CeramicCapacitorIcon,
  MicaCapacitorIcon,
  PaperCapacitorIcon,
  SuperCapacitorIcon,
  MylarCapacitorIcon,
  TantalumCapacitorIcon,
  VariableCapacitorIcon,
  PolyesterCapacitorIcon,
  ConnectorIcon,
  CrystalIcon,
  DiodeIcon,
  EvaluationIcon,
  HardwareIcon,
  ICIcon,
  InductorIcon,
  KitIcon,
  LEDIcon,
  ModuleIcon,
  RelayIcon,
  ResistorIcon,
  PotentiometerIcon,
  SCRIcon,
  SensorIcon,
  SwitchIcon,
  TransformerIcon,
  TransistorIcon
} from "../../common/icons";
import "./PartTypeEditModal.css";

/**
 * Displays available part parametric values
 */
export function PartTypeEditModal({ partType, parentPartType, partTypes, isOpen, onClose, onChange, onClearParentPartType }) {
  const { t } = useTranslation();
  const [_isOpen, setIsOpen] = useState(isOpen);
  const [iconDropdown, setIconDropdown] = useState("");
  const [_partType, setPartType] = useState(partType);
  const [_parentPartType, setParentPartType] = useState(parentPartType);
  const [_partTypes, setPartTypes] = useState(partTypes);

  const iconNames = [
    { key: -1, text: "None", value: "" },
    { key: 0, text: "CableIcon", value: "CableIcon", icon: <CableIcon /> },
    { key: 1, text: "CapacitorIcon", value: "CapacitorIcon", icon: <CapacitorIcon /> },
    { key: 2, text: "CeramicCapacitorIcon", value: "CeramicCapacitorIcon", icon: <CeramicCapacitorIcon /> },
    { key: 3, text: "MicaCapacitorIcon", value: "MicaCapacitorIcon", icon: <MicaCapacitorIcon /> },
    { key: 4, text: "PaperCapacitorIcon", value: "PaperCapacitorIcon", icon: <PaperCapacitorIcon /> },
    { key: 5, text: "SuperCapacitorIcon", value: "SuperCapacitorIcon", icon: <SuperCapacitorIcon /> },
    { key: 6, text: "MylarCapacitorIcon", value: "MylarCapacitorIcon", icon: <MylarCapacitorIcon /> },
    { key: 7, text: "TantalumCapacitorIcon", value: "TantalumCapacitorIcon", icon: <TantalumCapacitorIcon /> },
    { key: 8, text: "VariableCapacitorIcon", value: "VariableCapacitorIcon", icon: <VariableCapacitorIcon /> },
    { key: 9, text: "PolyesterCapacitorIcon", value: "PolyesterCapacitorIcon", icon: <PolyesterCapacitorIcon /> },
    { key: 10, text: "ConnectorIcon", value: "ConnectorIcon", icon: <ConnectorIcon /> },
    { key: 11, text: "CrystalIcon", value: "CrystalIcon", icon: <CrystalIcon /> },
    { key: 12, text: "DiodeIcon", value: "DiodeIcon", icon: <DiodeIcon /> },
    { key: 13, text: "EvaluationIcon", value: "EvaluationIcon", icon: <EvaluationIcon /> },
    { key: 14, text: "HardwareIcon", value: "HardwareIcon", icon: <HardwareIcon /> },
    { key: 15, text: "ICIcon", value: "ICIcon", icon: <ICIcon /> },
    { key: 16, text: "InductorIcon", value: "InductorIcon", icon: <InductorIcon /> },
    { key: 17, text: "KitIcon", value: "KitIcon", icon: <KitIcon /> },
    { key: 18, text: "LEDIcon", value: "LEDIcon", icon: <LEDIcon /> },
    { key: 19, text: "ModuleIcon", value: "ModuleIcon", icon: <ModuleIcon /> },
    { key: 20, text: "RelayIcon", value: "RelayIcon", icon: <RelayIcon /> },
    { key: 21, text: "ResistorIcon", value: "ResistorIcon", icon: <ResistorIcon /> },
    { key: 22, text: "PotentiometerIcon", value: "PotentiometerIcon", icon: <PotentiometerIcon /> },
    { key: 23, text: "SCRIcon", value: "SCRIcon", icon: <SCRIcon /> },
    { key: 24, text: "SensorIcon", value: "SensorIcon", icon: <SensorIcon /> },
    { key: 25, text: "SwitchIcon", value: "SwitchIcon", icon: <SwitchIcon /> },
    { key: 26, text: "TransformerIcon", value: "TransformerIcon", icon: <TransformerIcon /> },
    { key: 27, text: "TransistorIcon", value: "TransistorIcon", icon: <TransistorIcon /> }
  ];

  useEffect(() => {
    setIsOpen(isOpen);
  }, [isOpen]);

  useEffect(() => {
    setPartType(partType);
  }, [partType]);

  useEffect(() => {
    setParentPartType(parentPartType);
  }, [parentPartType]);

  useEffect(() => {
    setPartTypes(partTypes);
  }, [partTypes]);

  const doSavePartType = async (e) => {
    const request = { ..._partType };
    if (request.svg) {
      if ((request.svg.includes("<") || request.svg.includes("&lt;")) && !isSvg(request.svg)) {
        toast.error("Icon contains invalid SVG!");
        return;
      }
      request.icon = request.svg;
    }

    const response = await fetchApi("/api/partType", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      const part = { ...data, parts: 0 };

      if (onChange) onChange(e, 'edit', part);
      toast.success(t("success.savedPartType", "Saved part type {{name}}", { name: part.name }));
    } else {
      toast.error(t("error.failedSavePartType", "Failed to save Part Type!"));
    }
    setPartType(null);
    handleCloseModal(e);
  };

  const doCreatePartType = async (e) => {
    if (_partType.name.length === 0) {
      toast.error('Name is required!');
      return;
    }
    const request = {
      ..._partType,
      parentPartTypeId: Number.parseInt(_parentPartType?.partTypeId || _partType.parentPartTypeId),
    };
    if (request.svg) {
      if ((request.svg.includes("<") || request.svg.includes("&lt;")) && !isSvg(request.svg)) {
        toast.error("Icon contains invalid SVG!");
        return;
      }
      request.icon = request.svg;
    }

    const response = await fetchApi("/api/partType", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      // reset form
      const { data } = response;
      const part = { ...data, parts: 0 };
      if (onChange) onChange(e, 'add', part);
      toast.success(t("success.addedPartType", "Added part type {{name}}", { name: response.data.name }));
    } else {
      toast.error(t("error.failedAddedPartType", "Failed to add part type {{name}}", { name: partType.name }));
    }
  };

  const handleSavePartType = async (e) => {
    if (_partType?.partTypeId > 0) {
      await doSavePartType(e);
    } else {
      await doCreatePartType(e);
    }
  };

  const handleNewPartTypeNameChange = (e, control) => {
    setPartType({ ..._partType, name: control.value });
  };

  const handleDescriptionChange = (e, control) => {
    setPartType({ ..._partType, description: control.value });
  };

  const handleKeywordsChange = (e, control) => {
    setPartType({ ..._partType, keywords: control.value });
  };

  const handleReferenceDesignatorChange = (e, control) => {
    setPartType({ ..._partType, referenceDesignator: control.value });
  };

  const handleSymbolIdChange = (e, control) => {
    setPartType({ ..._partType, symbolId: control.value });
  };

  const handleNewPartTypeIconNameChange = (e, control) => {
    setIconDropdown("");
    setPartType({ ..._partType, svg: control.value, icon: "" });
  };

  const handleNewPartTypeIconChange = (e, control) => {
    setIconDropdown(control.value);
    setPartType({ ..._partType, icon: control.value, svg: "" });
  };

  const GetIsSvgValidIcon = () => {
    try {
      if (_partType?.icon?.length > 0) {
        if ((_partType?.icon.length > 0 && !_partType.icon.includes("<")) || (_partType?.icon.length > 0 && _partType.icon.includes("<") && isSvg(_partType.icon))) {
          return <Icon name="check circle" color="green" />;
        }
        return <Icon name="times circle" color="red" />;
      } else {
        return <></>;
      }
    } catch {
      return <Icon name="times circle" color="gray" />;
    }
  };

  const handleCloseModal = (e) => {
    if (onClose) onClose(e);
  };

  const handleClearParentPartType = (e) => {
    setParentPartType(null);
    if (onClearParentPartType) onClearParentPartType(e);
  };

  return (<Modal open={_isOpen}>
    <Modal.Header>{_partType?.partTypeId > 0 ? t('page.partTypes.edit', "Edit Part Type") : t('page.partTypes.add', "Add Part Type")}</Modal.Header>
    <Modal.Content style={{ minHeight: '500px' }}>
      <Form>
        {_parentPartType && <Form.Field>
          <label>Parent Part Type</label>
          {_parentPartType.name} <Icon name="times circle" onClick={handleClearParentPartType} style={{ cursor: 'pointer' }} />
        </Form.Field>}
        <Form.Field>
          <Form.Input label="Name" name="name" value={_partType?.name || ''} onChange={handleNewPartTypeNameChange} />
        </Form.Field>
        <Form.Field>
          <Form.Input label="Description" name="description" value={_partType?.description || ''} onChange={handleDescriptionChange} />
        </Form.Field>
        <Form.Field>
          <Popup 
            wide='very'
            content={<p>Keywords are used to help with part type detection. They should be comma delimited.<br /><i>Example: "screws,hinges,bolts,gears"</i></p>} 
            trigger={<Form.Input label="Keywords" name="keywords" value={_partType?.keywords || ''} onChange={handleKeywordsChange} />} 
          />
        </Form.Field>
        <Form.Field>
          <Popup
            wide='very'
            hoverable
            content={<p>Default reference designator for the part type.<br/><i>Examples: "R" for resistors, "C" for capacitors, etc.</i><br/><a href="/referenceDesignators" target="_blank" rel="noopener noreferrer">View reference designators</a></p>}
            trigger={<Form.Input label="Reference designator" name="referenceDesignator" value={_partType?.referenceDesignator || ''} onChange={handleReferenceDesignatorChange} />}
          />
        </Form.Field>
        <Form.Field>
          <Popup
            wide='very'
            content={<p>KiCad symbol ID for the part type (optional)<br /><i>Examples: "Device:R" for resistors, "Device:C" for capacitors, etc.</i></p>}
            trigger={<Form.Input label="Symbol id" name="symbolId" value={_partType?.symbolId || ''} onChange={handleSymbolIdChange} />}
          />
        </Form.Field>
        <Form.Field>
          <label>Icon</label>
          <div className="small">Choose the icon for this part type. If not set, a default icon will be automatically chosen.</div>
          <div style={{ display: 'flex', width: '100%', gap: '10px' }}>
            <Popup 
              wide
              content={<p>The chosen default icon.</p>}
              trigger={<div style={{ verticalAlign: 'middle', margin: 'auto' }}>
                {_partType && getIcon(_partType.name, _.find(_partTypes, x => x.partTypeId === _partType.parentPartTypeId)?.name, _partType.icon)({ className: `parttype parttype-${_.find(_partTypes, x => x.partTypeId === _partType.parentPartTypeId)?.name || _partType.name}` })}
                </div>
              }
            />
            <Dropdown className="icons" selection fluid value={_partType?.icon || ''} options={iconNames} onChange={handleNewPartTypeIconChange} />
          </div>
          <Popup
            wide="very"
            hoverable
            content={<p>Paste an SVG html tag here and a preview will be shown below. It should start with &lt;svg&gt; and contain any svg drawing operations.<br /><br />Example: <i>&lt;svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"&gt;&lt;circle cx="10" cy="10" r="10" fill="green"/&gt;&lt;/svg&gt;</i></p>}
            trigger={<Form.Input label="or provide an SVG" icon placeholder="<svg><path ... /></svg>" name="icon" value={_partType?.svg || ''} onChange={handleNewPartTypeIconNameChange}>
              <input />
              {GetIsSvgValidIcon()}
            </Form.Input>}
          />
        </Form.Field>
      </Form>
      {_partType?.svg &&
        <div style={{ marginTop: '5px', minWidth: '60px', minHeight: '60px' }}>
          <img src={`data:image/svg+xml;utf8,${encodeURIComponent(_partType.svg)}`} alt="" style={{ maxWidth: '60px', maxHeight: 'auto', border: '1px solid #ccc', padding: '5px' }} />
        </div>}
    </Modal.Content>
    <Modal.Actions>
      <Button onClick={handleCloseModal}>{t('button.cancel', "Cancel")}</Button>
      <Button primary onClick={handleSavePartType}>
        <Icon name="save" /> {t('button.save', "Save")}
      </Button>
    </Modal.Actions>
  </Modal>);

};

PartTypeEditModal.propTypes = {
  /** The part type being edited */
  partType: PropTypes.object,
  /** The parent part type being edited */
  parentPartType: PropTypes.object,
  /** List of part types */
  partTypes: PropTypes.array,
  /** Event handler to set if the modal is open */
  isOpen: PropTypes.bool,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Event handler when the part type has changed (add or edit) */
  onChange: PropTypes.func,
  onClearParentPartType: PropTypes.func
};