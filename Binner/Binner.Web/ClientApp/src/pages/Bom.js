import React, { useState, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import {
  Icon,
  Input,
  Label,
  Button,
  TextArea,
  Form,
  Table,
  Segment,
  Popup,
  Grid,
  Pagination,
  Dropdown,
  Confirm,
  Breadcrumb
} from "semantic-ui-react";
import { FormHeader } from "../components/FormHeader";
import axios from "axios";
import _ from "underscore";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import { toast } from "react-toastify";
import { formatCurrency } from "../common/Utils";
import "./Bom.css";
import { AddBomPartModal } from "../components/AddBomPartModal";
import { AddPcbModal } from "../components/AddPcbModal";
import { ProducePcbModal } from "../components/ProducePcbModal";

export function Bom(props) {
  const defaultProject = {
    projectId: null,
    name: "",
    description: "",
    location: "",
    color: 0,
    loading: false,
    parts: [],
    pcbs: []
  };
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [project, setProject] = useState(defaultProject);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [btnDeleteDisabled, setBtnDeleteDisabled] = useState(true);
  const [addPartModalOpen, setAddPartModalOpen] = useState(false);
  const [addPcbModalOpen, setAddPcbModalOpen] = useState(false);
  const [producePcbModalOpen, setProducePcbModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState("Are you sure you want to remove these part(s) from your BOM?");
  const [isDirty, setIsDirty] = useState(false);

  const [colors] = useState(
    _.map(ProjectColors, function (c) {
      return {
        key: c.value,
        value: c.value,
        text: c.name,
        label: { ...(c.name !== "" && { color: c.name }), circular: true, empty: true, size: "tiny" }
      };
    })
  );

  const itemsPerPageOptions = [
    { key: 1, text: "5", value: 5 },
    { key: 2, text: "10", value: 10 },
    { key: 3, text: "25", value: 25 },
    { key: 4, text: "50", value: 50 },
    { key: 5, text: "100", value: 100 }
  ];

  const loadProject = async (projectName) => {
    setLoading(true);
    const response = await fetchApi(`bom?name=${projectName}`);
    const { data } = response;
    setProject(data);
    setLoading(false);
  };

  useEffect(() => {
    loadProject(props.params.project);
  }, [props.params]);

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
  };

  const handleChange = (e, control) => {
    project[control.name] = control.value;
    setProject({ ...project });
  };

  const handleClick = (e, control) => {};

  const onSubmit = () => {};

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setProject({ ...project, parts: _.sortBy(project.parts, [clickedColumn]) });
      setDirection("ascending");
    } else {
      setColumn(clickedColumn);
      setProject({ ...project, parts: project.parts.reverse() });
      setDirection(direction === "ascending" ? "descending" : "ascending");
    }
  };

  const handleDelete = async (e, control) => {
    const checkboxes = document.getElementsByName("chkSelect");
    let checkedValues = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkedValues.push(parseInt(checkboxes[i].value));
    }

    setLoading(true);
    const request = {
      projectId: project.projectId,
      ids: checkedValues
    };
    const response = await fetchApi('bom/part', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const parts = _.filter(project.parts, i => !checkedValues.includes(i.projectPartAssignmentId));

      // also reset the checkboxes
      for (let i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = false;
      }
      setProject({...project, parts: parts});
    }
    else
      toast.error("Failed to remove parts from BOM!");
    setLoading(false);
    setBtnDeleteDisabled(true);
    setConfirmDeleteIsOpen(false);
  };

  const handleOpenAddPart = (e, control) => {
    setAddPartModalOpen(true);
  };

  const handleOpenProducePcb = (e, control) => {
    setProducePcbModalOpen(true);
  };

  const handleOpenAddPcb = (e, control) => {
    setAddPcbModalOpen(true);
  };

  const handlePartSelected = (e, part) => {
    const checkboxesChecked = getPartsSelected();
    if (checkboxesChecked.length > 0) {
      setBtnDeleteDisabled(false);
    } else {
      setBtnDeleteDisabled(true);
    }
  };

  const getPartsSelected = () => {
    const checkboxes = document.getElementsByName("chkSelect");
    let checkboxesChecked = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkboxesChecked.push(checkboxes[i]);
    }
    return checkboxesChecked;
  };

  const saveColumn = async (e, bomPart) => {
    await savePartInlineChange(bomPart);
  };

  const savePartInlineChange = async (bomPart) => {
    if (!isDirty) return;
    setLoading(true);
    const request = {...bomPart, quantity: parseInt(bomPart.quantity) || 0, part: {...bomPart.part, quantity: parseInt(bomPart.part.quantity) || 0}};
    const response = await fetchApi('bom/part', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
    }
    else
      toast.error("Failed to save project change!");
    setLoading(false);
    setIsDirty(false);
  };

  const handlePartsInlineChange = (e, control, part) => {
    e.preventDefault();
    e.stopPropagation();
    if (part[control.name] !== control.value)
      setIsDirty(true);
    switch(control.name) {
      case 'quantity':
        let parsed = parseInt(control.value);
        if (!isNaN(parsed)) {
          part[control.name] = parsed;
        }
        break;
      default:
        part[control.name] = control.value;
        break;
    }
    setProject({...project});
  };

  const getPage = (page, recordCount) => {
    const start = (page - 1) * recordCount;
    const partsPage = [];
    for(let i = start; i < recordCount; i++) {
      if (i < project.parts.length)
        partsPage.push(project.parts[i]);
    }
    return partsPage;
  };

  const handleAddPart = async (e, addPartSelectedPart) => {
    if (!addPartSelectedPart) {
      toast.error('No part selected!');
      return;
    }
    // add part to BOM/project
    setAddPartModalOpen(false);
    setLoading(true);
    const request = {
      projectId: project.projectId,
      partNumber: addPartSelectedPart.part.partNumber,
      pcbId: addPartSelectedPart.pcbId,
      quantity: addPartSelectedPart.quantity,
      notes: addPartSelectedPart.notes,
      referenceId: addPartSelectedPart.referenceId,
    };
    const response = await fetch("bom/part", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.ok) {
      const data = await response.json();
      setProject(data);
    } else {
      toast.error('Failed to add part!');
    }
    setLoading(false);
  };

  const handleAddPcb = async (e, pcb) => {
    // add part to BOM/project
    setAddPcbModalOpen(false);
    setLoading(true);
    const request = { ...pcb, projectId: project.projectId };
    const response = await fetch("bom/pcb", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.ok) {
      const data = await response.json();
      project.pcbs.push(data);
      toast.success(`Added ${pcb.name} pcb to project!`);
      setProject({...project});
    } else {
      toast.error('Failed to add pcb!');
    }
    setLoading(false);
  };

  const handleProducePcb = async (e, producePcbRequest) => {
    // produce BOM pcb(s) by reducing inventory quantities
    setProducePcbModalOpen(false);
    setLoading(true);
    const request = { ...producePcbRequest, projectId: project.projectId };
    const response = await fetch("bom/produce", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.ok) {
      const data = await response.json();
      setProject(data);
      toast.success(`${producePcbRequest.quantity} PCB's were produced!`);
    } else {
      const message = await response.text();
      toast.error(message, { autoClose: 10000 });
    }
    setLoading(false);
  };  

  const handleDownload = async (e) => {
    // download a BOM parts list
    setLoading(true);
    axios
      .post(`bom/download`, { projectId: project.projectId }, {
        responseType: "blob"
      })
      .then((blob) => {
        // specifying blob filename, must create an anchor tag and use it as suggested: https://stackoverflow.com/questions/19327749/javascript-blob-filename-without-link
        var file = window.URL.createObjectURL(blob.data);
        var a = document.createElement("a");
        document.body.appendChild(a);
        a.style = "display: none";
        a.href = file;
        const today = new Date();
        a.download = `${project.name}-BOM.zip`;
        a.click();
        window.URL.revokeObjectURL(file);
        toast.success(`BOM exported successfully!`);
        setLoading(false);
      })
      .catch((error) => {
        toast.dismiss();
        console.error("error", error);
        toast.error(`BOM export failed!`);
        setLoading(false);
      });
  };

  const confirmDeleteOpen = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const checkboxesChecked = getPartsSelected();
    const selectedPartAssignmentIds = checkboxesChecked.map(c => parseInt(c.value));
    setConfirmDeleteIsOpen(true);
    setConfirmPartDeleteContent(
      <p>
        Are you sure you want to remove these <b>{checkboxesChecked.length}</b> part(s) from your BOM?<br />
        <br />
        <b>{project.parts.filter(p => selectedPartAssignmentIds.includes(p.projectPartAssignmentId)).map(p => p.partName).join(', ')}</b>
        <br /><br />
        This action is <i>permanent and cannot be recovered</i>.
      </p>
    );
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => props.history("/")}>Home</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => props.history("/bom")}>BOMs</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>BOM</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name="Bill of Materials" to="..">
        Manage your BOM by creating PCB(s) and adding your parts.
			</FormHeader>
      <AddBomPartModal isOpen={addPartModalOpen} onAdd={handleAddPart} onClose={() => setAddPartModalOpen(false)} pcbs={project.pcbs || []} />
      <AddPcbModal isOpen={addPcbModalOpen} onAdd={handleAddPcb} onClose={() => setAddPcbModalOpen(false)} />
      <ProducePcbModal isOpen={producePcbModalOpen} onSubmit={handleProducePcb} onClose={() => setProducePcbModalOpen(false)} pcbs={project.pcbs || []} />
      <Confirm
        className="confirm"
        header="Remove Part from BOM"
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDelete}
        content={confirmPartDeleteContent}
      />

      <Form>
        <Segment>
          <Grid columns={2}>
            <Grid.Column>
              <span className="large">{project.name}</span>
              <div style={{float: 'right'}}><Link to={`/project/${project.name}`} className="small">Edit Project</Link></div>
            </Grid.Column>
            <Grid.Column>
              <div className="datacontainer">
                <Form.Group>
                  <Form.Field>
                    In Stock (<b>{_.filter(project.parts, s => s.part.quantity >= s.quantity).length}</b>)
                  </Form.Field>
                  <Form.Field>
                    Out of Stock (<b className={`${_.filter(project.parts, s => s.part.quantity < s.quantity).length > 0 ? "outofstock" : ""}`}>{_.filter(project.parts, s => s.part.quantity < s.quantity).length}</b>)
                  </Form.Field>
                  <Form.Field>
                    Total Parts (<b>{project.parts.length}</b>)
                  </Form.Field>
                </Form.Group>
              </div>
            </Grid.Column>
          </Grid>
          <Label className="small">Last modified: {new Date().toDateString()}</Label>

          <br />
          <br />

          <div className="buttons">
            <Popup
              content="Add a part to the BOM"
              trigger={
                <Button primary onClick={handleOpenAddPart} size="mini">
                  <Icon name="plus" /> Add Part
                </Button>
              }
            />
            <Popup
              content="Download a BOM part list"
              trigger={
                <Button onClick={handleDownload} size="mini">
                  <Icon name="download" /> Download
                </Button>
              }
            />
            <Popup
              content="Remove selected parts from the BOM"
              trigger={
                <Button onClick={confirmDeleteOpen} disabled={btnDeleteDisabled} size="mini">
                  <Icon name="trash alternate" /> Remove Part
                </Button>
              }
            />
            <Popup
              wide
              content="Add a PCB to this project"
              trigger={
                <Button onClick={handleOpenAddPcb} size="mini">
                  <Icon name="microchip" color="blue" /> Add PCB
                </Button>
              }
            />
            <Popup
              wide
              content="Reduce inventory quantities when a PCB is assembled"
              trigger={
                <Button secondary onClick={handleOpenProducePcb} size="mini">
                  <Icon name="microchip" color="blue" /> Produce PCB
                </Button>
              }
            />

          </div>

          <div style={{ float: "right", verticalAlign: "middle", fontSize: "0.9em", marginTop: "5px" }}>
            <Dropdown selection options={itemsPerPageOptions} value={pageSize} className="small labeled" onChange={handlePageSizeChange} />
            <span>records per page</span>
          </div>

          <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" />
          <Segment loading={loading}>
            <div className="scroll-container">
              <Table className="bom">
                <Table.Header>
                  <Table.Row>
                    <Table.HeaderCell></Table.HeaderCell>
                    <Table.HeaderCell style={{width: '120px'}} sorted={column === "PCB" ? direction : null} onClick={handleSort("PCB")}>
                      PCB
                    </Table.HeaderCell>
                    <Table.HeaderCell width={2} sorted={column === "partNumber" ? direction : null} onClick={handleSort("partNumber")}>
                      Part Number
                    </Table.HeaderCell>
                    <Table.HeaderCell width={2} sorted={column === "manufacturerPartNumber" ? direction : null} onClick={handleSort("manufacturerPartNumber")}>
                      Mfr Part
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{width: '100px'}} sorted={column === "partType" ? direction : null} onClick={handleSort("partType")}>
                      Part Type
                    </Table.HeaderCell>
                    <Table.HeaderCell sorted={column === "cost" ? direction : null} onClick={handleSort("cost")}>
                      Cost
                    </Table.HeaderCell>
                    <Table.HeaderCell sorted={column === "quantity" ? direction : null} onClick={handleSort("quantity")}>
                      <Popup content={<p>The quantity of parts required for a single PCB</p>} trigger={<span>Quantity</span>} />
                    </Table.HeaderCell>
                    <Table.HeaderCell sorted={column === "stock" ? direction : null} onClick={handleSort("stock")}>
                      <Popup content={<p>The quantity of parts currently in inventory</p>} trigger={<span>In Stock</span>} />
                    </Table.HeaderCell>
                    <Table.HeaderCell sorted={column === "leadTime" ? direction : null} onClick={handleSort("leadTime")}>
                      Lead Time
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{width: '110px'}} sorted={column === "referenceId" ? direction : null} onClick={handleSort("referenceId")}>
                      Reference Id
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{ width: "200px" }} sorted={column === "description" ? direction : null} onClick={handleSort("description")}>
                      Description
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{ width: "200px" }} sorted={column === "note" ? direction : null} onClick={handleSort("note")}>
                      Note
                    </Table.HeaderCell>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {getPage(page, pageSize).map((bomPart, key) => (
                    <Table.Row key={key}>
                      <Table.Cell>
                        <input type="checkbox" name="chkSelect" value={bomPart.projectPartAssignmentId} onChange={(e) => handlePartSelected(e, bomPart)} />
                      </Table.Cell>
                      <Table.Cell className="overflow"><div style={{ maxWidth: "120px" }}>{_.find(project.pcbs, x => x.pcbId === bomPart.pcbId)?.name}</div></Table.Cell>
                      <Table.Cell>
                        {bomPart.part 
                        ? <Link to={`/inventory/${bomPart.part.partNumber}`}>{bomPart.part.partNumber}</Link>
                        : bomPart.partName}
                      </Table.Cell>
                      <Table.Cell>{bomPart.part.manufacturerPartNumber}</Table.Cell>
                      <Table.Cell>{bomPart.part.partType}</Table.Cell>
                      <Table.Cell>{formatCurrency(bomPart.part.cost)}</Table.Cell>
                      <Table.Cell><Input type='text' transparent name='quantity' onBlur={e => saveColumn(e, bomPart)} onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)} value={bomPart.quantity || 0} fluid className={`inline-editable ${(bomPart.quantity > bomPart.part.quantity ? "outofstock" : "")}`} /></Table.Cell>
                      <Table.Cell>{bomPart.part.quantity}</Table.Cell>
                      <Table.Cell>{bomPart.part.leadTime}</Table.Cell>
                      <Table.Cell><Input type='text' transparent name='referenceId' onBlur={e => saveColumn(e, bomPart)} onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)} value={bomPart.referenceId || ''} fluid className="inline-editable" /></Table.Cell>
                      <Table.Cell className="overflow">
                        <div style={{ width: "250px" }}>
                          <Popup hoverable content={<p>{bomPart.part.description}</p>} trigger={<span>{bomPart.part.description}</span>} />
                        </div>
                      </Table.Cell>
                      <Table.Cell>
                        <div style={{ maxWidth: "250px" }}>
                          <Form.Field type='text' control={TextArea} style={{height: '50px', minHeight: '50px', padding: '5px'}} name='notes' onBlur={e => saveColumn(e, bomPart)} onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)} value={bomPart.notes || ''} className="transparent inline-editable" />
                        </div>
                      </Table.Cell>
                    </Table.Row>
                  ))}
                </Table.Body>
              </Table>
            </div>
          </Segment>
        </Segment>
      </Form>
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Bom {...props} params={useParams()} history={useNavigate()} location={window.location} />;
