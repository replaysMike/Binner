import React, { useState, useEffect, useMemo } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import {
  Icon,
  Input,
  Label,
  Button,
  TextArea,
  Image,
  Form,
  Table,
  Segment,
  Popup,
  Modal,
  Dimmer,
  Loader,
  Header,
  Confirm,
  Grid,
  Card,
  Menu,
  Placeholder,
  Flag,
  Checkbox,
  Pagination,
  Dropdown
} from "semantic-ui-react";
import _ from "underscore";
import debounce from "lodash.debounce";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import PartsGrid from "../components/PartsGrid";
import "./Bom.css";

export function Bom(props) {
  Bom.abortController = new AbortController();
  const defaultProject = {
    projectId: null,
    name: "",
    description: "",
    location: "",
    color: 0,
    loading: false,
    parts: []
  };
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [addVisible, setAddVisible] = useState(false);
  const [project, setProject] = useState(defaultProject);
  const [addPartSearchResults, setAddPartSearchResults] = useState([]);
  const [addPartSelectedPart, setAddPartSelectedPart] = useState(null);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [btnDeleteDisabled, setBtnDeleteDisabled] = useState(true);
  const [addPartModalOpen, setAddPartModalOpen] = useState(false);
  const [addPartForm, setAddPartForm] = useState({keyword: ''})

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
    const response = await fetchApi(`project?name=${projectName}`);
    const { data } = response;
    console.log("project", data);
    setProject(data);
    setLoading(false);
  };

  useEffect(() => {
    loadProject(props.params.project);
  }, [props.params]);


  const search = async (keyword) => {
    console.log('search', keyword);
    Bom.abortController.abort(); // Cancel the previous request
    Bom.abortController = new AbortController();

    setLoading(true);

    try {
      const response = await fetch(`part/search?keywords=${keyword}`, {
        signal: Bom.abortController.signal
      });

      if (response.status === 200) {
        const data = await response.json();
        setAddPartSearchResults(data || []);
        setLoading(false);
      } else {
        setAddPartSearchResults([]);
        setLoading(false);
      }
    } catch (ex) {
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const searchDebounced = useMemo(() => debounce(search, 400), []);

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

  const handleAddPartFormChange = (e, control) => {
    addPartForm[control.name] = control.value;
    switch (control.name) {
      case "keyword":
        if (control.value && control.value.length > 0)
          searchDebounced(control.value);
        break;
      default:
        break;
    }
    setAddPartForm({ ...addPartForm });
  };

  const handleShowAdd = () => {
    setAddVisible(!addVisible);
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

  const handleDelete = (e, control) => {};

  const handleOpenAddPart = (e, control) => {
    setAddPartModalOpen(true);
  };

  const handleDownload = (e, control) => {};

  const handleProducePcb = (e, control) => {};

  const handlePartSelected = (e, part) => {
    console.log('checked', e);

    const checkboxes = document.getElementsByName("chkSelect");
    let checkboxesChecked = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkboxesChecked.push(checkboxes[i]);
    }

    console.log('checked boxes', checkboxesChecked);
    if (checkboxesChecked.length > 0) {
      setBtnDeleteDisabled(false);
    } else {
      setBtnDeleteDisabled(true);
    }
  };

  const getPage = (page, recordCount) => {
    const start = (page - 1) * recordCount;
    console.log('getPage', start, recordCount);
    const partsPage = [];
    for(let i = start; i < recordCount; i++) {
      if (i < project.parts.length)
        partsPage.push(project.parts[i]);
    }
    return partsPage;
  };

  const handleAddPartModalClose = (e) => {
    setAddPartModalOpen(false);
  };

  const handleAddPart = async (e) => {
    // add part to BOM/project
    setAddPartModalOpen(false);
    setLoading(true);
    const request = {
      name: project.name,
      part: addPartSelectedPart
    };
    const response = await fetch("project/part", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    const { data } = response;
    setProject(data);
    setLoading(false);
  };

  const handleAddPartsNextPage = (e) => {
  };

  const handleAddPartSelectPart = (e, part) => {
    setAddPartSelectedPart(part);
  };

  return (
    <div>
      <h1>Bill of Materials</h1>
      <Modal
        centered
        open={addPartModalOpen}
        onClose={handleAddPartModalClose}
      >
		<Modal.Header>Add Part</Modal.Header>
		<Modal.Content scrolling>
      <Form style={{marginBottom: '10px'}}>
        <Form.Field width={8}>
          <Form.Input label="Part Number" placeholder="LM358" name="keyword" value={addPartForm.keyword} onChange={handleAddPartFormChange} />
        </Form.Field>
      </Form>

      <PartsGrid
        parts={addPartSearchResults}
        page={page}
        totalPages={totalPages}
        loading={loading}
        loadPage={handleAddPartsNextPage}
        onPartClick={handleAddPartSelectPart}
        onPageSizeChange={handlePageSizeChange}
        selectedPart={addPartSelectedPart}
        columns='partNumber,quantity,manufacturerPartNumber,description,location,binNumber,binNumber2,cost,digikeyPartNumber,mouserPartNumber,datasheetUrl'
        editable={false}
        visitable={false}
        name="partsGrid"
      >No matching results.</PartsGrid>
      
		</Modal.Content>
    <Modal.Actions>
      <Button onClick={handleAddPartModalClose}>Cancel</Button>
      <Button primary onClick={handleAddPart}><Icon name="plus" /> Add</Button>
    </Modal.Actions>
	</Modal>

      <Form>
        <Segment>
          <Grid columns={2}>
            <Grid.Column>
              <Form.Group>
                <Form.Field width={14}>
                  <Popup
                    hideOnScroll
                    content="Your project name"
                    trigger={<Form.Input placeholder="My Project" value={project.name || ""} onChange={handleChange} name="name" className="large" />}
                  />
                </Form.Field>
              </Form.Group>
            </Grid.Column>
            <Grid.Column>
              <div className="datacontainer">
                <Form.Group>
                  <Form.Field>
                    In Stock (<b>0</b>)
                  </Form.Field>
                  <Form.Field>
                    Out of Stock (<b>0</b>)
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
              wide
              content="Reduce inventory quantities when a PCB is assembled"
              trigger={
                <Button secondary onClick={handleProducePcb} size="mini">
                  <Icon name="microchip" color="blue" /> Produce PCB
                </Button>
              }
            />
            <Popup
              content="Delete selected parts"
              trigger={
                <Button onClick={handleDelete} disabled={btnDeleteDisabled} size="mini">
                  <Icon name="trash alternate" /> Delete Part
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
                    <Table.HeaderCell width={2} sorted={column === "partNumber" ? direction : null} onClick={handleSort("partNumber")}>
                      Part Number
                    </Table.HeaderCell>
                    <Table.HeaderCell width={2} sorted={column === "manufacturerPartNumber" ? direction : null} onClick={handleSort("manufacturerPartNumber")}>
                      Mfr Part
                    </Table.HeaderCell>
                    <Table.HeaderCell sorted={column === "partType" ? direction : null} onClick={handleSort("partType")}>
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
                    <Table.HeaderCell style={{ width: "200px" }} sorted={column === "description" ? direction : null} onClick={handleSort("description")}>
                      Description
                    </Table.HeaderCell>
                    <Table.HeaderCell style={{ width: "200px" }} sorted={column === "note" ? direction : null} onClick={handleSort("note")}>
                      Note
                    </Table.HeaderCell>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {getPage(page, pageSize).map((p, key) => (
                    <Table.Row key={key}>
                      <Table.Cell>
                        <input type="checkbox" name="chkSelect" onChange={(e) => handlePartSelected(e, p)} />
                      </Table.Cell>
                      <Table.Cell>
                        <Link to={`/inventory/${p.partNumber}`}>{p.partNumber}</Link>
                      </Table.Cell>
                      <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                      <Table.Cell>{p.partType}</Table.Cell>
                      <Table.Cell>{p.cost}</Table.Cell>
                      <Table.Cell>{p.quantity}</Table.Cell>
                      <Table.Cell>{p.stock}</Table.Cell>
                      <Table.Cell>{p.leadTime}</Table.Cell>
                      <Table.Cell className="overflow">
                        <div style={{ width: "200px" }}>
                          <Popup hoverable content={<p>{p.description}</p>} trigger={<span>{p.description}</span>} />
                        </div>
                      </Table.Cell>
                      <Table.Cell className="overflow">
                        <div style={{ width: "200px" }}>
                          <Popup hoverable content={<p>{p.note}</p>} trigger={<span>{p.note}</span>} />
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
