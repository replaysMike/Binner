import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import isSvg from "is-svg";
import { Button, Segment, Form, Icon, Confirm, Breadcrumb, Popup, Modal, Checkbox, Input } from "semantic-ui-react";
import PartTypeSelectorMemoized from "../components/PartTypeSelectorMemoized";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import { FormHeader } from "../components/FormHeader";
import { getIcon } from "../common/partTypes";
import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import { TreeView } from "@mui/x-tree-view";
import { TreeItem, treeItemClasses } from "@mui/x-tree-view";
import Typography from "@mui/material/Typography";
import { Memory as MemoryTwoTone } from "@mui/icons-material";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";
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
} from "../common/icons";
import "./PartTypes.css";

export function PartTypes(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const defaultPartType = {
    partTypeId: 0,
    name: "",
    parentPartTypeId: "",
    loading: false
  };
  const [parentPartType, setParentPartType] = useState(null);
  const [partTypesFiltered, setPartTypesFiltered] = useState([]);
  const [partTypes, setPartTypes] = useState([]);
  const [partType, setPartType] = useState(defaultPartType);
  const [addVisible, setAddVisible] = useState(false);
  const [loading, setLoading] = useState(true);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [selectedPartType, setSelectedPartType] = useState(null);
  const [chkHideEmptyTypes, setChkHideEmptyTypes] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [expandedNodeIds, setExpandedNodeIds] = useState([]);
  const [search, setSearch] = useState("");
  const [isRenameModalOpen, setIsRenameModalOpen] = useState(false);
  const [btnAddDisabled, setBtnAddDisabled] = useState(true);
  const [modalContext, setModalContext] = useState(null);
  const [iconDropdown, setIconDropdown] = useState("");
  const [updateTreeView, setUpdateTreeView] = useState(true);
  const iconNames = [
  { key: -1, text: "None", value:"" },
  { key: 0,  text: "CableIcon", value:"CableIcon", icon: <CableIcon /> },
  { key: 1,  text: "CapacitorIcon", value:"CapacitorIcon", icon: <CapacitorIcon /> },
  { key: 2,  text: "CeramicCapacitorIcon", value:"CeramicCapacitorIcon", icon: <CeramicCapacitorIcon /> },
  { key: 3,  text: "MicaCapacitorIcon", value:"MicaCapacitorIcon", icon: <MicaCapacitorIcon /> },
  { key: 4,  text: "PaperCapacitorIcon", value:"PaperCapacitorIcon", icon: <PaperCapacitorIcon /> },
	{ key: 5,  text: "SuperCapacitorIcon", value:"SuperCapacitorIcon", icon: <SuperCapacitorIcon /> },
  { key: 6,  text: "MylarCapacitorIcon", value:"MylarCapacitorIcon", icon: <MylarCapacitorIcon /> },
  { key: 7,  text: "TantalumCapacitorIcon", value:"TantalumCapacitorIcon", icon: <TantalumCapacitorIcon /> },
  { key: 8,  text: "VariableCapacitorIcon", value:"VariableCapacitorIcon", icon: <VariableCapacitorIcon /> },
  { key: 9,  text: "PolyesterCapacitorIcon", value:"PolyesterCapacitorIcon", icon: <PolyesterCapacitorIcon /> },
  { key: 10, text: "ConnectorIcon", value:"ConnectorIcon", icon: <ConnectorIcon /> },
  { key: 11, text: "CrystalIcon", value:"CrystalIcon", icon: <CrystalIcon /> },
  { key: 12, text: "DiodeIcon", value:"DiodeIcon", icon: <DiodeIcon /> },
	{ key: 13, text: "EvaluationIcon", value:"EvaluationIcon", icon: <EvaluationIcon /> },
  { key: 14, text: "HardwareIcon", value:"HardwareIcon", icon: <HardwareIcon /> },
  { key: 15, text: "ICIcon", value:"ICIcon", icon: <ICIcon /> },
  { key: 16, text: "InductorIcon", value:"InductorIcon", icon: <InductorIcon /> },
  { key: 17, text: "KitIcon", value:"KitIcon", icon: <KitIcon /> },
  { key: 18, text: "LEDIcon", value:"LEDIcon", icon: <LEDIcon /> },
  { key: 19, text: "ModuleIcon", value:"ModuleIcon", icon: <ModuleIcon /> },
  { key: 20, text: "RelayIcon", value:"RelayIcon", icon: <RelayIcon /> },
  { key: 21, text: "ResistorIcon", value:"ResistorIcon", icon: <ResistorIcon /> },
	{ key: 22, text: "PotentiometerIcon", value:"PotentiometerIcon", icon: <PotentiometerIcon /> },
  { key: 23, text: "SCRIcon", value:"SCRIcon", icon: <SCRIcon /> },
  { key: 24, text: "SensorIcon", value:"SensorIcon", icon: <SensorIcon /> },
  { key: 25, text: "SwitchIcon", value:"SwitchIcon", icon: <SwitchIcon /> },
  { key: 26, text: "TransformerIcon", value:"TransformerIcon", icon: <TransformerIcon /> },
  { key: 27, text: "TransistorIcon", value:"TransistorIcon", icon: <TransistorIcon /> }
  ];

  const loadPartTypes = useCallback((parentPartType = "") => {
    setLoading(true);
    if (parentPartType === undefined || parentPartType === null) parentPartType = "";
    fetchApi(`api/partType/all?parent=${parentPartType}`).then((response) => {
      const { data } = response;

      setPartTypes(data);
      setPartTypesFiltered(data);
      setLoading(false);
      setUpdateTreeView(!updateTreeView);
    });
  }, []);

  useEffect(() => {
    loadPartTypes();
  }, [loadPartTypes]);

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    partType[control.name] = control.value;
    setPartType({...partType});
    if (control.value.length === 0) 
      setBtnAddDisabled(true);
    else
      setBtnAddDisabled(false);
  };

  const handleSearchChange = (e, control) => {
    setSearch(control.value);
    let newPartTypesFiltered = recursivePreFilter(partTypes, null, control.value.toLowerCase());
    // now remove all part types that don't match the filter
    newPartTypesFiltered = _.filter(partTypes, i => i.filterMatch === true);
    const newPartTypesFilteredOrdered = _.sortBy(newPartTypesFiltered, x => x.exactMatch ? 0 : 1);
    setPartTypesFiltered(newPartTypesFilteredOrdered);
    if (control.value.length > 1) {
      setExpandedNodeIds(_.map(newPartTypesFiltered, (i) => (i.name)));
    }else{
      setExpandedNodeIds([]);
    }
    setUpdateTreeView(!updateTreeView);
  };

  const handleNewPartTypeNameChange = (e, control) => {
    setModalContext({ ...modalContext, name: control.value });
  };

  const handleNewPartTypeIconNameChange = (e, control) => {
    setIconDropdown("");
    setModalContext({ ...modalContext, svg: control.value, icon: "" });
  };

  const handleNewPartTypeIconChange = (e, control) => {
    setIconDropdown(control.value);
    setModalContext({ ...modalContext, icon: control.value, svg: "" });
  };

  /**
   * Update part type
   * @param {any} e
   */
  const onCreatePartType = async (e) => {
    setBtnAddDisabled(true);
    const request = {
      name: partType.name,
      parentPartTypeId: Number.parseInt(partType.parentPartTypeId)
    };
    const response = await fetchApi("api/partType", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      // reset form
      setPartType(defaultPartType);
      setAddVisible(false);
      setUpdateTreeView(!updateTreeView);
      toast.success(t("success.addedPartType", "Added part type {{name}}", { name: response.data.name }));
      loadPartTypes(response.data.parentPartType);
    } else {
      toast.error(t("error.failedAddedPartType", "Failed to add part type {{name}}", { name: partType.name }));
    }
    setBtnAddDisabled(false);
  };

  const onDelete = async (partType) => {
    const response = await fetchApi("api/partType", {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ partTypeId: partType.partTypeId })
    });

    if (response.responseObject.status === 200) {
      const isSuccess = response.data;
      if (isSuccess) {
        const partTypesDeleted = _.without(partTypes, _.findWhere(partTypes, { partTypeId: selectedPartType.partTypeId }));
        setPartTypes(partTypesDeleted);
        setPartTypesFiltered(partTypesDeleted);
        if (parentPartType) loadPartTypes(parentPartType.name);
        else loadPartTypes();
        setUpdateTreeView(!updateTreeView);
        toast.success(t("success.deletedPartType", "Deleted part type {{name}}", { name: partType.name }));
      } else {
        toast.error(t("error.failedDeletedPartType", "Failed to delete part type {{name}}", { name: partType.name }));
      }
      setConfirmDeleteIsOpen(false);
      setSelectedPartType(null);
    }
  };

  const handleEditPartType = async (e) => {
    const request = { ...modalContext };
    if (request.svg) {
      if ((request.svg.includes("<") || request.svg.includes("&lt;")) && !isSvg(request.svg)) {
        toast.error("Icon contains invalid SVG!");
        return;
      }
      request.icon = request.svg;
    }

    const response = await fetchApi("api/partType", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      const newPartTypes = [..._.filter(partTypes, i => i.partTypeId !== modalContext.partTypeId), {...data, parts: modalContext.parts}];
      const sortedPartTypes = _.sortBy(newPartTypes, i => i.name);
      setPartTypes(sortedPartTypes);
      setPartTypesFiltered(sortedPartTypes);
      setUpdateTreeView(!updateTreeView);

      toast.success(t("success.savedPartType", "Saved part type {{name}}", { name: response.data.name }));
    } else {
      toast.error(t("error.failedSavePartType", "Failed to save Part Type!"));
    }
    setModalContext(null);
    setIsRenameModalOpen(false);
  };

  const handleViewPartsForPartType = (e, partType) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(`/inventory?by=partType&value=${partType.name}`);
  };

  const handleShowAdd = (e) => {
    if (!addVisible) {
      if (parentPartType) setPartType({ ...partType, parentPartTypeId: parentPartType.partTypeId });
    }
    setAddVisible(!addVisible);
  };

  const handleDelete = async (e) => {
    await onDelete(selectedPartType);
  };

  const handleUnsetParentPartType = (e) => {
    e.preventDefault();
    setParentPartType(null);
    setAddVisible(false);
    loadPartTypes();
  };

  const confirmDeleteOpen = (e, partType) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(true);
    setSelectedPartType(partType);
    setConfirmPartDeleteContent(
      <p>
        <Trans i18nKey="confirm.deletePartType" name={partType.name}>
          Are you sure you want to delete part type <i>{{ name: partType.name }}</i>?
        </Trans>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPartType(null);
  };

  const StyledTreeItemRoot = styled(TreeItem)(({ theme }) => ({
    color: theme.palette.text.secondary,
    [`& .${treeItemClasses.content}`]: {
      color: theme.palette.text.secondary,
      borderTopRightRadius: theme.spacing(2),
      borderBottomRightRadius: theme.spacing(2),
      paddingRight: theme.spacing(1),
      fontWeight: theme.typography.fontWeightMedium,
      "&.Mui-expanded": {
        fontWeight: theme.typography.fontWeightRegular
      },
      "&:hover": {
        backgroundColor: theme.palette.action.hover
      },
      "&.Mui-focused, &.Mui-selected, &.Mui-selected.Mui-focused": {
        backgroundColor: `var(--tree-view-bg-color, ${theme.palette.action.selected})`,
        color: "var(--tree-view-color)"
      },
      [`& .${treeItemClasses.label}`]: {
        fontWeight: "inherit",
        color: "inherit"
      }
    },
    [`& .${treeItemClasses.group}`]: {
      marginLeft: 0,
      [`& .${treeItemClasses.content}`]: {
        paddingLeft: theme.spacing(2)
      }
    }
  }));

  const StyledTreeItem = (props) => {
    const { bgColor, color, labelIcon: LabelIcon, labelColor, labelFontWeight, labelInfo, labelText, data, ...other } = props;

    return (
      <StyledTreeItemRoot
        label={
          <Box sx={{ display: "flex", alignItems: "center", p: 0.5, pr: 0 }}>
            <Box component={LabelIcon} color="inherit" sx={{ mr: 1 }} />
            <Typography variant="body2" sx={{ fontWeight: "inherit", flexGrow: 1 }}>
              {labelText}
            </Typography>
            <Typography variant="caption" color={labelColor} sx={{ fontWeight: labelFontWeight }}>
            <Popup
              position="left center"
              content={<p>{t("page.partTypes.partCount", "The number of parts in this category")}</p>}
              trigger={<div>{labelInfo}</div>}
            />              
            </Typography>
            <Popup
              position="left center"
              content={<p>{t("page.partTypes.viewParts", "View Parts")}</p>}
              trigger={
                <Button
                  size="tiny"
                  style={{ fontSize: "0.6em", padding: '0.5em 1.5em', marginLeft: "40px", marginTop: "-3px" }}
                  onClick={(e) => handleViewPartsForPartType(e, data)}
                >{t("button.parts", "Parts")}</Button>
              }
            />
            <Popup
              position="left center"
              content={<p>{t("button.edit", "Edit")}</p>}
              trigger={
                <Button
                  size="tiny"
                  style={{ fontSize: "0.6em", padding: '0.5em 1.5em', marginLeft: "10px", marginTop: "-3px" }}
                  onClick={(e) => handleOpenEditPartModal(e, data)}
                >{t("button.edit", "Edit")}</Button>
              }
            />
            <Popup
              position="left center"
              content={<p>{t("button.delete", "Delete")}</p>}
              trigger={
                <Button
                  circular
                  icon="delete"
                  size="tiny"
                  style={{ fontSize: "0.4em", marginLeft: "10px", marginTop: "-3px" }}
                  onClick={(e) => confirmDeleteOpen(e, data)}
                />
              }
            />
          </Box>
        }
        style={{
          "--tree-view-color": color,
          "--tree-view-bg-color": bgColor
        }}
        {...other}
      />
    );
  };

  const getPartTypeFromName = (name) => {
		const lcName = name.toLowerCase();
		return _.find(partTypes, (i) => i.name.toLowerCase() === lcName)
	};

  const recursivePreFilter = (partTypes, parentPartTypeId, filterBy) => {
		// go through every child, mark filtered matches

    const filterByLowerCase = filterBy.toLowerCase();
		const childrenComponents = [];
		let partTypesInCategory = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
		for(let i = 0; i < partTypesInCategory.length; i++){
        partTypesInCategory[i].exactMatch = partTypesInCategory[i].name.toLowerCase() === filterByLowerCase;
			if (partTypesInCategory[i].name.toLowerCase().includes(filterByLowerCase)){
				partTypesInCategory[i].filterMatch = true;
			} else {
				partTypesInCategory[i].filterMatch = false;
			}
			childrenComponents.push(partTypesInCategory[i]);

			// now filter the children of this category
			const childs = recursivePreFilter(partTypes, partTypesInCategory[i].partTypeId, filterBy);
			if (_.find(childs, i => i.filterMatch)) {
				// make sure the parent matches the filter because it has children that does
				partTypesInCategory[i].filterMatch = true;
			}
			for(var c = 0; c < childs.length; c++) {
				childrenComponents.push(childs[c]);
			}
		}
		return childrenComponents;
	};

  const recursiveTreeItem = (partTypes, parentPartTypeId = null) => {
    // build a tree graph
    const children = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
    const childrenComponents = [];
    if (children && children.length > 0) {
      for (let i = 0; i < children.length; i++) {
        const key = `${children[i].name}-${i}`;
        const nodeId = `${children[i].name}`;
        const childs = recursiveTreeItem(partTypes, children[i].partTypeId);
        if (chkHideEmptyTypes && children[i].parts === 0) {
          // don't display empty types
        } else {
          const basePartTypeName = _.find(partTypes, x => x.partTypeId === children[i].parentPartTypeId)?.name;
          const partTypeName = children[i].name;
          const partTypeIcon = children[i].icon;
          childrenComponents.push(
            <StyledTreeItem
              nodeId={nodeId}
              key={key}
              data={children[i]}
              labelText={partTypeName}
              labelIcon={() => getIcon(partTypeName, children[i].parentPartTypeId && basePartTypeName, partTypeIcon)({className: `parttype parttype-${basePartTypeName || partTypeName}`})}
              labelInfo={`${children[i].parts}`}
              labelColor={children[i].parts > 0 ? "#1a73e8" : "inherit"}
              labelFontWeight={children[i].parts > 0 ? "700" : "inherit"}
              color="#1a73e8"
              bgColor="#e8f0fe"
            >
              {childs}
            </StyledTreeItem>
          );
        }
      }
    }
    /*
        <StyledTreeItem
              nodeId="5"
              labelText="Social"
              labelIcon={SupervisorAccountIcon}
              labelInfo="90"
              color="#1a73e8"
              bgColor="#e8f0fe"
            />
    */

    return childrenComponents;
  };

  const handleOnNodeSelect = (e, selectedPartTypeName) => {
    const selectedPartType = getPartTypeFromName(selectedPartTypeName);
    if (selectedPartType) {
      setPartType(selectedPartType);
      // fire event
			if (props.onSelect) props.onSelect(e, selectedPartType);
    }
  };

  const handleOnNodeToggle = (e, node) => {
    //e.preventDefault();
    //e.stopPropagation();
		// preventing event propagation leads to ui weirdness unfortunately
		if (expandedNodeIds.includes(node))
			setExpandedNodeIds(_.filter(expandedNodeIds, i => i !== node));
		else
			setExpandedNodeIds(node);
  };

  const handleOpenEditPartModal = (e, pt) => {
    if (pt.icon) {
      if (isSvg(pt.icon)) {
        pt.svg = pt.icon;
        pt.icon = "";
      } else {
        pt.svg = "";
        setIconDropdown(pt.icon);
      }
    }
    setModalContext(pt);
    setIsRenameModalOpen(true);
  };

  const handlePartTypeSelectorChange = (e, newParentPartType) => {
    setPartType({...partType, parentPartTypeId: newParentPartType.partTypeId});
  };

  const renderTreeView = useMemo(() => {
    return (
      <TreeView
        className="partTypesTreeView"
        defaultCollapseIcon={<ArrowDropDownIcon />}
        defaultExpandIcon={<ArrowRightIcon />}
        defaultEndIcon={<div style={{ width: 24 }} />}
        onNodeSelect={handleOnNodeSelect}
        onNodeToggle={handleOnNodeToggle}
        expanded={expandedNodeIds}
        sx={{ height: 650, flexGrow: 1, maxWidth: "100%", overflowY: "auto" }}
      >
        {recursiveTreeItem(partTypesFiltered).map((x) => x)}
      </TreeView>);
  }, [updateTreeView, expandedNodeIds, partTypesFiltered, chkHideEmptyTypes]);

  const GetIsSvgValidIcon = () => {
    try{
      if (modalContext?.icon?.length > 0) {
        if((modalContext?.icon.length > 0 && !modalContext.icon.includes("<")) || (modalContext?.icon.length > 0 && modalContext.icon.includes("<") && isSvg(modalContext.icon))){
          return <Icon name="check circle" color="green" />;
        }
        return <Icon name="times circle" color="red" />;
      } else {
        return <></>;
      }
    }catch{
      return <Icon name="times circle" color="gray" />;
    }
  };

  return (
    <div className="partTypes">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>
          {t("bc.home", "Home")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        {parentPartType ? (
          <React.Fragment>
            <Breadcrumb.Section href="/partTypes" onClick={handleUnsetParentPartType}>
              {t("bc.partTypes", "Part Types")}
            </Breadcrumb.Section>
            <Breadcrumb.Divider />
            <Breadcrumb.Section active>{parentPartType.name}</Breadcrumb.Section>
          </React.Fragment>
        ) : (
          <Breadcrumb.Section active>{t("page.partTypes.title", "Part Types")}</Breadcrumb.Section>
        )}
      </Breadcrumb>
      <FormHeader name={t("page.partTypes.title", "Part Types")} to="/">
        <Trans i18nKey="page.partTypes.description">
          Part Types allow you to separate your parts by type. <i>Parent</i> types allow for unlimited part type hierarchy.
          <br />
          For example: OpAmps may be a sub-type of IC's, so OpAmp's parent type is IC.
        </Trans>
      </FormHeader>
      <Confirm
        className="confirm"
        header={t("confirm.header.deletePart", "Delete Part")}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDelete}
        content={confirmPartDeleteContent}
      />
      <Modal open={isRenameModalOpen}>
        <Modal.Header>{t('page.partTypes.edit', "Edit Part Type")}</Modal.Header>
        <Modal.Content style={{minHeight: '500px'}}>
          <Form>
            <Form.Field>
              <Form.Input label="New Name" name="name" value={modalContext?.name || ''} onChange={handleNewPartTypeNameChange} />
            </Form.Field>
            <Form.Field>
              <Form.Dropdown className="icons" selection fluid value={iconDropdown} options={iconNames} onChange={handleNewPartTypeIconChange}  />
              <Popup 
                wide="very"
                hoverable
                content={<p>Paste an SVG html tag here and a preview will be shown below. It should start with &lt;svg&gt; and contain any svg drawing operations.<br/><br/>Example: <i>&lt;svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"&gt;&lt;circle cx="10" cy="10" r="10" fill="green"/&gt;&lt;/svg&gt;</i></p>}
                trigger={<Form.Input label="or provide an SVG" icon placeholder="<svg><path ... /></svg>" name="icon" value={modalContext?.svg || ''} onChange={handleNewPartTypeIconNameChange}>
                  <input />
                  {GetIsSvgValidIcon()}
                </Form.Input>}
              />
            </Form.Field>
          </Form>
          {modalContext?.svg && 
            <div style={{marginTop: '5px', minWidth: '60px', minHeight: '60px'}}>
              <img src={`data:image/svg+xml;utf8,${encodeURIComponent(modalContext.svg)}`} alt="" style={{ maxWidth: '60px', maxHeight: 'auto', border: '1px solid #ccc', padding: '5px' }} />
            </div>}
          {modalContext?.icon && modalContext?.icon.length > 0 && partTypes && 
            <div style={{color: '#bbb', marginTop: '5px', padding: '5px'}}>
              {getIcon(modalContext.name, _.find(partTypes, x => x.partTypeId === modalContext.parentPartTypeId)?.name, modalContext.icon)()}
            </div>
          }
        </Modal.Content>
        <Modal.Actions>
        <Button onClick={() => setIsRenameModalOpen(false) }>{t('button.cancel', "Cancel")}</Button>
          <Button primary onClick={handleEditPartType}>
            <Icon name="save" /> {t('button.save', "Save")}
          </Button>
        </Modal.Actions>
      </Modal>

      <Segment loading={loading}>
        <div style={{ marginBottom: "10px", padding: "5px", backgroundColor: "#fafafa" }}>
          {/** Tools Header */}
          <div style={{ float: "left", lineHeight: "2.25em" }}>
            <Popup
              content="Hide part types that have no parts assigned"
              trigger={
                <Checkbox
                  toggle
                  label={t("label.hideEmptyTypes", "Hide Empty Types")}
                  name="filterEmpty"
                  onChange={(e, control) => setChkHideEmptyTypes(!chkHideEmptyTypes)}
                />
              }
            />
          </div>
          <div style={{ minHeight: "35px", marginTop: "2px" }}>
            <Button onClick={handleShowAdd} icon size="mini" floated="right" className="svg">
              <MemoryTwoTone style={{color: "#2185d0"}} /> {t("button.addPartType", "Add Part Type")}
            </Button>
          </div>
        </div>
        {parentPartType && (
          <Button size="mini" onClick={handleUnsetParentPartType}>
            <Icon name="arrow alternate circle left" /> {t("button.back", "Back")}
          </Button>
        )}
        <div>
          {addVisible && (
            <Segment>
              <Form onSubmit={onCreatePartType}>
                <Form.Field width={8}>
                  <Form.Input
                    label={t("label.name", "Name")}
                    required
                    placeholder={t("label.resistors", "Resistors")}
                    focus
                    value={partType.name}
                    onChange={handleChange}
                    name="name"
                  />
                </Form.Field>
                <Form.Field width={8}>
                  <PartTypeSelectorMemoized 
                    label={t("label.parent", "Parent")}
                    name="parentPartTypeId"
                    value={partType.parentPartTypeId || ""}
                    partTypes={partTypes} 
                    onSelect={handlePartTypeSelectorChange}
                  />
                </Form.Field>
                <Button primary type="submit" icon disabled={btnAddDisabled}>
                  <Icon name="add" /> {t("button.add", "Add")}
                </Button>
              </Form>
            </Segment>
          )}
        </div>

        <Input name="search" placeholder="Search..." icon="search" onChange={handleSearchChange} style={{marginTop: '5px', marginBottom: '10px', width: '300px'}} />
        {/** https://mui.com/material-ui/react-tree-view/ */}
        {renderTreeView}
      </Segment>
    </div>
  );
}
