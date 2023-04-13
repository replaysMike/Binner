import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Button, Segment, Form, Icon, Confirm, Breadcrumb, Header, Popup } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import { FormHeader } from "../components/FormHeader";

import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import TreeView from "@mui/lab/TreeView";
import TreeItem, { treeItemClasses } from "@mui/lab/TreeItem";
import Typography from "@mui/material/Typography";
import { Memory as MemoryTwoTone } from "@mui/icons-material";
import Label from "@mui/icons-material/Label";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";

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
  const [partTypeOptions, setPartTypeOptions] = useState([]);
  const [partTypes, setPartTypes] = useState([]);
  const [partType, setPartType] = useState(defaultPartType);
  const [changeTracker, setChangeTracker] = useState([]);
  const [lastSavedPartTypeId, setLastSavedPartTypeId] = useState(0);
  const [addVisible, setAddVisible] = useState(false);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadingAllPartTypes, setLoadingAllPartTypes] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [selectedPartType, setSelectedPartType] = useState(null);
  const [chkHideEmptyTypes, setChkHideEmptyTypes] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);

  const loadPartTypes = useCallback((parentPartType = "") => {
    setLoading(true);
    if (parentPartType === undefined || parentPartType === null) parentPartType = "";
    fetchApi(`partType/all?parent=${parentPartType}`).then((response) => {
      const { data } = response;

      setPartTypes(data);
      //setPartTypes([]);
      setLoading(false);
    });
  }, []);

  useEffect(() => {
    loadPartTypes();
  }, [loadPartTypes]);

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setPartTypes(_.sortBy(partTypes, [clickedColumn]));
      setDirection("ascending");
    } else {
      setPartTypes(partTypes.reverse());
      setDirection(direction === "ascending" ? "descending" : "ascending");
    }
  };

  const loadAllPartTypes = () => {
    setLoadingAllPartTypes(true);
    fetchApi(`partType/all`).then((response) => {
      const { data } = response;

      setPartTypeOptions(
        data.map((i, key) => ({
          key: key,
          value: i.partTypeId,
          content: <Header icon="microchip" content={i.name} subheader={i.parentPartType} />,
          text: i.name
        }))
      );
      setLoadingAllPartTypes(false);
    });
  };

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    partType[control.name] = control.value;
    const newPartType = { ...partType };
    setPartType(newPartType);
  };

  const handleInlineChange = async (e, control, partType) => {
    e.preventDefault();
    e.stopPropagation();
    partType[control.name] = control.value;
    let changes = [...changeTracker];
    if (_.where(changes, { partTypeId: partType.partTypeId }).length === 0) {
      changes.push({ partTypeId: partType.partTypeId });
    }
    setPartTypes(partTypes);
    setChangeTracker(changes);
  };

  /**
   * Save new project
   * @param {any} e
   */
  const onSubmit = async (e) => {
    const request = {
      name: partType.name,
      parentPartTypeId: Number.parseInt(partType.parentPartTypeId)
    };
    const response = await fetchApi("partType", {
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
      toast.success(t("success.addedPartType", "Added part type {{name}}", { name: response.data.name }));
      loadPartTypes(response.data.parentPartType);
    } else {
      toast.error(t("error.failedAddedPartType", "Failed to add part type {{name}}", { name: partType.name }));
    }
  };

  const onDelete = async (partType) => {
    const response = await fetchApi("partType", {
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
        if (parentPartType) loadPartTypes(parentPartType.name);
        else loadPartTypes();
        toast.success(t("success.deletedPartType", "Deleted part type {{name}}", { name: partType.name }));
      } else {
        toast.error(t("error.failedDeletedPartType", "Failed to delete part type {{name}}", { name: partType.name }));
      }
      setConfirmDeleteIsOpen(false);
      setSelectedPartType(null);
    }
  };

  const saveColumn = async (e) => {
    changeTracker.forEach(async (val) => {
      const partType = _.where(partTypes, { partTypeId: val.partTypeId }) || [];
      if (partType.length > 0) await save(partType[0]);
    });
    setPartTypes(partTypes);
    setChangeTracker([]);
  };

  const save = async (partType) => {
    const p = _.where(partTypes, { partTypeId: partType.partTypeId });
    p.loading = false;
    setPartTypes(partTypes);
    let lastSavedPartTypeId = 0;
    const request = {
      partTypeId: Number.parseInt(partType.partTypeId),
      name: partType.name,
      parentPartTypeId: Number.parseInt(partType.parentPartTypeId)
    };
    const response = await fetchApi("partType", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      lastSavedPartTypeId = partType.partTypeId;
      toast.success(t("success.savedPartType", "Saved part type {{name}}", { name: response.data.name }));
    } else {
      toast.error(t("error.failedSavePartType", "Failed to save Part Type!"));
    }
    p.loading = false;
    setPartTypes(partTypes);
    setLastSavedPartTypeId(lastSavedPartTypeId);
  };

  const handleShowAdd = (e) => {
    if (!addVisible) {
      loadAllPartTypes();
      if (parentPartType) setPartType({ ...partType, parentPartTypeId: parentPartType.partTypeId });
    }
    setAddVisible(!addVisible);
  };

  const handleDelete = async (e) => {
    await onDelete(selectedPartType);
  };

  const handleEditRow = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
    // prevent handling of click if target is the input text box
    if (e.target.type === "text") return;
    setParentPartType(p);
    loadPartTypes(p.name);
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
              {labelInfo}
            </Typography>
            <Popup
              position="left center"
              content={<p>{t("button.delete", "Delete")}</p>}
              trigger={
                <Button
                  circular
                  icon="delete"
                  size="tiny"
                  style={{ fontSize: "0.4em", marginLeft: "40px", marginTop: "-3px" }}
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

  const recursiveTreeItem = (partTypes, parentPartTypeId = null) => {
    // build a tree graph
    const children = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
    const childrenComponents = [];
    if (children && children.length > 0) {
      for (let i = 0; i < children.length; i++) {
        const key = `${children[i].name}-${i}`;
        const childs = recursiveTreeItem(partTypes, children[i].partTypeId);
        if (chkHideEmptyTypes && children[i].parts === 0) {
          // don't display empty types
        } else {
          childrenComponents.push(
            <StyledTreeItem
              nodeId={key}
              key={key}
              data={children[i]}
              labelText={children[i].name}
              labelIcon={Label}
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

  return (
    <div>
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
      <FormHeader name={t("page.partTypes.title", "Part Types")} to="..">
        <Trans i18nKey="page.partTypes.description">
          Part Types allow you to separate your parts by type. <i>Parent</i> types allow for unlimited part type hierarchy.
          <br />
          For example: OpAmps may be a sub-type of IC's, so OpAmp's parent type is IC.
        </Trans>
      </FormHeader>
      <p></p>
      <Confirm
        className="confirm"
        header={t("confirm.header.deletePart", "Delete Part")}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDelete}
        content={confirmPartDeleteContent}
      />

      <Segment loading={loading} style={{ marginBottom: "50px" }}>
        <div style={{ marginBottom: "10px", padding: "5px", backgroundColor: "#fafafa" }}>
          {/** Tools Header */}
          <div style={{ float: "left", lineHeight: "2.25em" }}>
            <Popup
              content="Hide part types that have no parts assigned"
              trigger={
                <Form.Checkbox
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
              <MemoryTwoTone /> {t("button.addPartType", "Add Part Type")}
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
              <Form onSubmit={onSubmit}>
                <Form.Input
                  width={6}
                  label={t("label.name", "Name")}
                  required
                  placeholder={t("label.resistors", "Resistors")}
                  focus
                  value={partType.name}
                  onChange={handleChange}
                  name="name"
                />
                <Form.Dropdown
                  width={6}
                  label={t("label.parent", "Parent")}
                  selection
                  fluid
                  value={partType.parentPartTypeId}
                  options={partTypeOptions}
                  onChange={handleChange}
                  name="parentPartTypeId"
                />
                <Button primary type="submit" icon>
                  <Icon name="save" /> {t("button.save", "Save")}
                </Button>
              </Form>
            </Segment>
          )}
        </div>

        {/** https://mui.com/material-ui/react-tree-view/ */}
        <TreeView
          aria-label="gmail"
          defaultExpanded={["3"]}
          defaultCollapseIcon={<ArrowDropDownIcon />}
          defaultExpandIcon={<ArrowRightIcon />}
          defaultEndIcon={<div style={{ width: 24 }} />}
          sx={{ height: 500, flexGrow: 1, maxWidth: "100%", overflowY: "auto" }}
        >
          {recursiveTreeItem(partTypes).map((x) => x)}
          {/*<StyledTreeItem nodeId="1" labelText="All Mail" labelIcon={MailIcon} />
          <StyledTreeItem nodeId="2" labelText="Trash" labelIcon={DeleteIcon} />
          <StyledTreeItem nodeId="3" labelText="Categories" labelIcon={Label}>
            <StyledTreeItem
              nodeId="5"
              labelText="Social"
              labelIcon={SupervisorAccountIcon}
              labelInfo="90"
              color="#1a73e8"
              bgColor="#e8f0fe"
            />
            <StyledTreeItem
              nodeId="6"
              labelText="Updates"
              labelIcon={InfoIcon}
              labelInfo="2,294"
              color="#e3742f"
              bgColor="#fcefe3"
            />
            <StyledTreeItem
              nodeId="7"
              labelText="Forums"
              labelIcon={ForumIcon}
              labelInfo="3,566"
              color="#a250f5"
              bgColor="#f3e8fd"
            />
            <StyledTreeItem
              nodeId="8"
              labelText="Promotions"
              labelIcon={LocalOfferIcon}
              labelInfo="733"
              color="#3c8039"
              bgColor="#e6f4ea"
            />
          </StyledTreeItem>
        <StyledTreeItem nodeId="4" labelText="History" labelIcon={Label} />*/}
        </TreeView>
      </Segment>
    </div>
  );
}
