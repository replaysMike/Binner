import { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import isSvg from "is-svg";
import { Button, Segment, Icon, Confirm, Breadcrumb, Popup, Checkbox, Input } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import { FormHeader } from "../components/FormHeader";
import { getIcon } from "../common/partTypes";
import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import { SimpleTreeView } from "@mui/x-tree-view";
import { TreeItem, treeItemClasses } from "@mui/x-tree-view";
import Typography from "@mui/material/Typography";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";
import { PartTypeEditModal } from "../components/modals/PartTypeEditModal";
import "./PartTypes.css";
import { Link } from "react-router-dom";

export function PartTypes() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const defaultPartType = {
    partTypeId: 0,
    name: "",
    parentPartTypeId: null,
    keywords: "",
    description: "",
    referenceDesignator: "",
    symbolId: "",
    icon: null,
    loading: false
  };
  const [parentPartType, setParentPartType] = useState(null);
  const [partTypesFiltered, setPartTypesFiltered] = useState([]);
  const [partTypes, setPartTypes] = useState([]);
  const [partType, setPartType] = useState(defaultPartType);
  const [loading, setLoading] = useState(true);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [selectedPartType, setSelectedPartType] = useState(null);
  const [chkHideEmptyTypes, setChkHideEmptyTypes] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [expandedNodeIds, setExpandedNodeIds] = useState([]);
  const [selectedNodeIds, setSelectedNodeIds] = useState([]);
  const [search, setSearch] = useState("");
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [updateTreeView, setUpdateTreeView] = useState(true);

  const loadPartTypes = useCallback((parentPartType = "") => {
    setLoading(true);
    if (parentPartType === undefined || parentPartType === null) parentPartType = "";
    fetchApi(`/api/partType/all?parent=${parentPartType}`).then((response) => {
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

  const handleSearchChange = (e, control) => {
    setSearch(control.value);
    let newPartTypesFiltered = recursivePreFilter(partTypes, null, control.value.toLowerCase());
    // now remove all part types that don't match the filter
    newPartTypesFiltered = _.filter(partTypes, i => i.filterMatch === true);
    const newPartTypesFilteredOrdered = _.sortBy(newPartTypesFiltered, x => x.exactMatch ? 0 : 1);
    setPartTypesFiltered(newPartTypesFilteredOrdered);
    if (control.value.length > 1) {
      setExpandedNodeIds(_.map(newPartTypesFiltered, (i) => (i.name)));
    } else {
      setExpandedNodeIds([]);
    }
    setUpdateTreeView(!updateTreeView);
  };

  const onDelete = async (partType) => {
    const response = await fetchApi("/api/partType", {
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

  const handleViewPartsForPartType = (e, partType) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(`/inventory?by=partType&value=${partType.name}`);
  };

  const handleOpenAdd = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setPartType({ ...defaultPartType, parentPartTypeId: parentPartType ? parentPartType.partTypeId : null });
    setIsEditModalOpen(true);
  };

  const handleDelete = async (e) => {
    await onDelete(selectedPartType);
  };

  const handleClearParentPartType = (e) => {
    e.preventDefault();
    setParentPartType(null);
    setSelectedNodeIds([]);

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
                trigger={<div>{labelInfo || 0}</div>}
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

  const recursivePreFilter = (partTypes, parentPartTypeId, filterBy) => {
    // go through every child, mark filtered matches

    const filterByLowerCase = filterBy.toLowerCase();
    const childrenComponents = [];
    let partTypesInCategory = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
    for (let i = 0; i < partTypesInCategory.length; i++) {
      partTypesInCategory[i].exactMatch = partTypesInCategory[i].name.toLowerCase() === filterByLowerCase;
      if (partTypesInCategory[i].name.toLowerCase().includes(filterByLowerCase)) {
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
      for (var c = 0; c < childs.length; c++) {
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
        const nodeId = children[i].name;
        const itemId = children[i].partTypeId;
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
              itemId={itemId}
              data={children[i]}
              labelText={partTypeName}
              labelIcon={() => getIcon(partTypeName, children[i].parentPartTypeId && basePartTypeName, partTypeIcon)({ className: `parttype parttype-${basePartTypeName || partTypeName}` })}
              labelInfo={`${children[i].parts || 0}`}
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
    return childrenComponents;
  };

  const handleOnNodeSelect = (e, itemId) => {
    const selectedPartType = partTypes.find((pt) => pt.partTypeId === itemId);
    
    if (selectedNodeIds.includes(selectedPartType.partTypeId)) {
      // unselect
      setSelectedNodeIds(_.filter(selectedNodeIds, i => i !== selectedPartType.partTypeId));
      setPartType(null);
      setParentPartType(null);
    } else {
      // select
      setSelectedNodeIds([selectedPartType.partTypeId]);
      setPartType(selectedPartType);
      setParentPartType(selectedPartType);
    }
  };

  const handleOnNodeToggle = (e, itemId) => {
    const selectedPartType = partTypes.find((pt) => pt.partTypeId === itemId);
    //e.preventDefault();
    //e.stopPropagation();
    // preventing event propagation leads to ui weirdness unfortunately
    if (expandedNodeIds.includes(selectedPartType.partTypeId))
      setExpandedNodeIds(_.filter(expandedNodeIds, i => i !== selectedPartType.partTypeId));
    else
      setExpandedNodeIds([selectedPartType.partTypeId]);
  };

  const handleOpenEditPartModal = (e, pt) => {
    e.preventDefault();
    e.stopPropagation();
    if (pt.icon) {
      if (isSvg(pt.icon)) {
        pt.svg = pt.icon;
        pt.icon = "";
      } else {
        pt.svg = "";

        // todo:
        //setIconDropdown(pt.icon);
      }
    }

    setParentPartType(null);
    setPartType(pt);
    setIsEditModalOpen(true);
  };

  const renderTreeView = useMemo(() => {
    return (
      <SimpleTreeView
        className="partTypesTreeView"
        defaultCollapseIcon={<ArrowDropDownIcon />}
        defaultExpandIcon={<ArrowRightIcon />}
        defaultEndIcon={<div style={{ width: 24 }} />}
        onItemClick={handleOnNodeSelect}
        onItemExpansionToggle={handleOnNodeToggle}
        expanded={expandedNodeIds}
        selectedItems={selectedNodeIds}
        multiSelect={false}
        sx={{ height: 650, flexGrow: 1, maxWidth: "100%", overflowY: "auto" }}
        getItemId={(item) => item.partTypeId || -1}
      >
        {recursiveTreeItem(partTypesFiltered).map((x) => x)}
      </SimpleTreeView>);
  }, [updateTreeView, expandedNodeIds, selectedNodeIds, partTypesFiltered, chkHideEmptyTypes]);

  const handlePartEditChange = (e, editMode, updatedPartType) => {
    setIsEditModalOpen(false);
    switch (editMode) {
      case 'add': {
        const newPartTypes = [...partTypes, updatedPartType];
        const sortedPartTypes = _.sortBy(newPartTypes, i => i.name);

        setPartTypes(sortedPartTypes);
        setPartTypesFiltered(sortedPartTypes);
        break;
      }
      case 'edit': {
        const newPartTypes = _.map(partTypes, i => i.partTypeId === updatedPartType.partTypeId ? updatedPartType : i);
        setPartTypes(newPartTypes);
        setPartTypesFiltered(newPartTypes);
        break;
      }
    }
    setUpdateTreeView(!updateTreeView);
  };

  return (
    <div className="partTypes">
      <PartTypeEditModal
        partType={partType}
        parentPartType={parentPartType}
        partTypes={partTypes}
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        onChange={handlePartEditChange}
        onClearParentPartType={handleClearParentPartType}
      />
      <Confirm
        className="confirm"
        header={t("confirm.header.deletePart", "Delete Part")}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDelete}
        content={confirmPartDeleteContent}
      />
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>
          {t("bc.home", "Home")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("page.partTypes.title", "Part Types")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.partTypes.title", "Part Types")} to="/">
        <Trans i18nKey="page.partTypes.description">
          Part Types allow you to separate your parts by type. <i>Parent</i> types allow for unlimited part type hierarchy.
          <br />
          For example: OpAmps may be a sub-type of IC's, so OpAmp's parent type is IC.
        </Trans>
      </FormHeader>

      <Segment loading={loading}>
        {parentPartType && (
          <div>
            Selected Node: {parentPartType.name}
            <Icon name="circle times" onClick={handleClearParentPartType} style={{ cursor: 'pointer', marginLeft: '10px' }} />
          </div>
        )}

        <div style={{ width: '300px '}}>
          <Input fluid name="search" placeholder="Search..." icon="search" onChange={handleSearchChange} style={{ marginTop: '5px', marginBottom: '10px' }} />
          <div className="tools small">
            <Popup
              content="Hide part types that have no parts assigned"
              trigger={
                <Checkbox
                  label={t("label.hideEmptyTypes", "Hide Empty Types")}
                  name="filterEmpty"
                  onChange={(e, control) => setChkHideEmptyTypes(!chkHideEmptyTypes)}
                  style={{ marginRight: '20px' }}
                  className="small"
                />
              }
            />
            <Link primary onClick={handleOpenAdd} icon size="mini" className="svg">
              <Icon name="add" /> {t("button.addPartType", "Add Part Type")}
            </Link>
          </div>
        </div>

        {/** https://mui.com/material-ui/react-tree-view/ */}
        {renderTreeView}
      </Segment>
    </div>
  );
}
