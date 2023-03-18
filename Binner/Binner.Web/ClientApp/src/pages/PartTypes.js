import React, { useState, useEffect, useCallback } from "react";
import _ from "underscore";
import { Table, Input, Button, Segment, Form, Icon, Confirm, Breadcrumb, Header, Popup } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { toast } from "react-toastify";
import { FormHeader } from "../components/FormHeader";

export function PartTypes(props) {
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
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState("Are you sure you want to delete this part?");

  const loadPartTypes = useCallback((parentPartType = "") => {
    setLoading(true);
    if (parentPartType === undefined || parentPartType === null)
      parentPartType = "";
    fetchApi(`partType/list?parent=${parentPartType}`).then((response) => {
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

      setPartTypeOptions(data.map((i, key) => ({
        key: key,
        value: i.partTypeId,
        content: <Header icon='microchip' content={i.name} subheader={i.parentPartType} />,
        text: i.name
      })));
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
      toast.success(`Added part type '${response.data.name}'`);
      loadPartTypes(response.data.parentPartType);
    } else {
      toast.error(`Failed to add part type '${partType.name}'`);
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

    if (response.responseObject.status === 200){
      const isSuccess = response.data;
      if (isSuccess){
        const partTypesDeleted = _.without(partTypes, _.findWhere(partTypes, { partTypeId: selectedPartType.partTypeId }));
        setPartTypes(partTypesDeleted);
        if (parentPartType)
          loadPartTypes(parentPartType.name);
        else
          loadPartTypes();
        toast.success(`Deleted part type '${partType.name}'`);
      }else {
        toast.error(`Failed to delete part type '${partType.name}'`);
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
      toast.success(`Saved part type '${response.data.name}'`);
    } else {
      toast.error("failed to save partType");
    }
    p.loading = false;
    setPartTypes(partTypes);
    setLastSavedPartTypeId(lastSavedPartTypeId);
  };

  const handleShowAdd = (e) => {
    if(!addVisible){
      loadAllPartTypes();
      if (parentPartType)
        setPartType({...partType, parentPartTypeId: parentPartType.partTypeId });
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
    if (e.target.type === 'text')
      return;
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
    setConfirmPartDeleteContent(<p>Are you sure you want to delete part type <b>{partType.name}</b>?<br/><br/>This action is <i>permanent and cannot be recovered</i>.</p>);
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setSelectedPartType(null);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section href="/">Home</Breadcrumb.Section>
        <Breadcrumb.Divider />
        {parentPartType 
          ? <React.Fragment>
            <Breadcrumb.Section href="/partTypes" onClick={handleUnsetParentPartType}>Part Types</Breadcrumb.Section>
            <Breadcrumb.Divider />
            <Breadcrumb.Section active>{parentPartType.name}</Breadcrumb.Section>
            </React.Fragment> 
          : <Breadcrumb.Section active>Part Types</Breadcrumb.Section>}
      </Breadcrumb>
      <FormHeader name="Part Types" to="..">
        Part Types allow you to separate your parts by type. <i>Parent</i> types allow for unlimited part type hierarchy.
        <br />
        For example: OpAmps may be a sub-type of IC's, so OpAmp's parent type is IC.
			</FormHeader>
      <p>
      </p>
      <Confirm className="confirm" header='Delete Part' open={confirmDeleteIsOpen} onCancel={confirmDeleteClose} onConfirm={handleDelete} content={confirmPartDeleteContent} />

      <Segment loading={loading}>
        <div style={{float: 'left'}}>
          <Popup 
            content="Hide part types that have no parts assigned"
            trigger={<Form.Checkbox toggle label="Hide Empty Types" name="filterEmpty" onChange={(e, control) => setChkHideEmptyTypes(!chkHideEmptyTypes)} />}
          />          
        </div>
        <div style={{ minHeight: "35px" }}>
          <Button onClick={handleShowAdd} icon size="mini" floated="right">
            <Icon name="file" /> Add Part Type
          </Button>
        </div>
        {parentPartType && <Button size="mini" onClick={handleUnsetParentPartType}><Icon name="arrow alternate circle left"/> Back</Button>}
        <div>
          {addVisible && (
            <Segment>
              <Form onSubmit={onSubmit}>
                <Form.Input width={6} label="Name" required placeholder="Resistors" focus value={partType.name} onChange={handleChange} name="name" />
                <Form.Dropdown
                  width={6}
                  label="Parent"
                  selection
                  fluid
                  value={partType.parentPartTypeId}
                  options={partTypeOptions}
                  onChange={handleChange}
                  name="parentPartTypeId"
                />
                <Button primary type="submit" icon>
                  <Icon name="save" /> Save
                </Button>
              </Form>
            </Segment>
          )}
        </div>
        <Table compact celled sortable selectable striped size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell sorted={column === "name" ? direction : null} onClick={handleSort("name")}>
                Part Type
              </Table.HeaderCell>
              <Table.HeaderCell width={3} sorted={column === "parentPartTypeId" ? direction : null} onClick={handleSort("location")}>
                Parent
              </Table.HeaderCell>
              <Table.HeaderCell width={2} sorted={column === "parts" ? direction : null} onClick={handleSort("parts")}>
                Parts Count
              </Table.HeaderCell>
              <Table.HeaderCell width={2}></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {parentPartType && 
            <Table.Row key={-1}>
              <Table.Cell colSpan={2} style={{fontSize: '1.2em', marginLeft: '5px'}}><div style={{border: '1px solid #0d3d61', borderRadius: '4px', fontSize: '0.8em', color: '#fff', backgroundColor: '#2185d0', display: 'inline-block', padding: '1px 5px', marginRight: '10px'}}>Parent</div> <b>{parentPartType.name}</b></Table.Cell>
              <Table.Cell>{parentPartType.parts}</Table.Cell>
              <Table.Cell></Table.Cell>
            </Table.Row>
            }
            {partTypes.length > 0 
            ? partTypes.map((p) => (
              chkHideEmptyTypes && p.parts === 0 
              ? ""
              :
              <Table.Row key={p.partTypeId} onClick={(e) => handleEditRow(e, p)} className="clickablerow">
                <Table.Cell>
                  <Popup 
                    content="Edit the part type name"
                    trigger={<Input
                      width={4}
                      labelPosition="left"
                      type="text"
                      transparent
                      name="name"
                      className="inline-editable"
                      onBlur={saveColumn}
                      onChange={(e, control) => handleInlineChange(e, control, p)}
                      value={p.name || ""}
                      fluid
                    />}
                  />
                </Table.Cell>
                <Table.Cell>{p.parentPartType}</Table.Cell>
                <Table.Cell>{p.parts}</Table.Cell>
                <Table.Cell textAlign="center">
                  {!p.isSystem ? <Button icon="delete" size="tiny" onClick={(e) => confirmDeleteOpen(e, p)} /> : "System Type"}
                </Table.Cell>
              </Table.Row>
            ))
            : <Table.Row>
              <Table.Cell colSpan={4} textAlign='center'>
                There are no child part types.
              </Table.Cell>
              </Table.Row>}
          </Table.Body>
        </Table>
      </Segment>
    </div>
  );
}
