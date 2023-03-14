import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import { Icon, Input, Label, Button, TextArea, Image, Form, Table, Segment, Popup, Modal, Dimmer, Loader, Header, Confirm, Grid,
  Card, Menu, Placeholder, Flag, Checkbox, Pagination, Dropdown
} from "semantic-ui-react";
import _ from 'underscore';
import { fetchApi } from '../common/fetchApi';
import { ProjectColors } from "../common/Types";

export function Boms (props) {
  const defaultProject = {
    name: '',
    description: '',
    location: '',
    color: 0,
    loading: false,
  };
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [addVisible, setAddVisible] = useState(false);
  const [project, setProject] = useState(defaultProject);
  const [projects, setProjects] = useState([]);
  const [changeTracker, setChangeTracker] = useState([]);
  const [lastSavedProjectId, setLastSavedProjectId] = useState(0);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [noRemainingData, setNoRemainingData] = useState(false);
  const [colors] = useState(_.map(ProjectColors, function (c) {
    return {
      key: c.value,
      value: c.value,
      text: c.name,
      label: { ...(c.name !== '' && { color: c.name }), circular: true, empty: true, size: 'tiny' }
    }
  }));

  const itemsPerPageOptions = [
    { key: 1, text: '5', value: 5 },
    { key: 2, text: '10', value: 10 },
    { key: 3, text: '25', value: 25 },
    { key: 4, text: '50', value: 50 },
    { key: 5, text: '100', value: 100 },
  ];

  const loadProjects = async (page, reset = false) => {
    setLoading(true);
    let endOfData = false;
    const response = await fetchApi(`project/list?orderBy=DateCreatedUtc&direction=Descending&results=${pageSize}&page=${page}`);
    const pageOfData = response.data;
    pageOfData.forEach(function (element) {
      element.loading = true;
    });
    if (pageOfData && pageOfData.length === 0)
      endOfData = true;
    let newData = [];
    if (reset) {
      newData = [...pageOfData];
    } else {
      newData = [...projects, ...pageOfData];
    }
    setProjects(newData);
    setPage(page);
    setNoRemainingData(endOfData);
    setLoading(false);
  };

  useEffect(() => {
    loadProjects(page);
  }, [page]);

  const handleNextPage = () => {
    if (noRemainingData) return;

    const nextPage = page + 1;
    loadProjects(nextPage);
  };

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    await loadProjects(page, true);
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
    // loadPage(e, control.activePage);
  };

  const handleChange = (e, control) => {
    //const updatedProject = { ...project };
    project[control.name] = control.value;
    setProject({...project});
  };

  const handleShowAdd = () => {
    setAddVisible(!addVisible);
  };

  const handleLoadBom = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
    props.history(`/bom/${p.name}`);
  };

  const onSubmit = () => {

  };

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setProjects(_.sortBy(projects, [clickedColumn]));
      setDirection('ascending');
    } else {
      setColumn(clickedColumn);
      setProjects(projects.reverse());
      setDirection(direction === 'ascending' ? 'descending' : 'ascending');
    }
  };

  const saveColumn = async (e) => {
    changeTracker.forEach(async (val) => {
      const project = _.where(projects, { projectId: val.projectId }) || [];
      if (project.length > 0) await save(project[0]);
    });
    setProjects(projects);
    setChangeTracker([]);
  };

  const save = async (project) => {
    const p = _.where(projects, { projectId: project.projectId });
    p.loading = false;
    let lastSavedProjectId = 0;
    project.color = Number.parseInt(project.color) || 0;
    const request = {
      name: project.name,
      description: project.description,
      location: project.location,
      color: project.color
    };
    const response = await fetchApi('project', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      lastSavedProjectId = data.projectId;
    }
    else {
      console.log('failed to save project');
    }
    p.loading = false;
    setLastSavedProjectId(lastSavedProjectId);
  };

  const handleInlineChange = (e, control, project) => {
    project[control.name] = control.value;
    let changes = [...changeTracker];
    
    if (_.where(changes, { projectId: project.projectId }).length === 0)
      changes.push({ projectId: project.projectId });
    
    setProject({...project});
    setChangeTracker(changes);
  };

  const handleDelete = async (e, project) => {
    await fetchApi('project', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ projectId: project.projectId})
    });

    await loadProjects(page, true);   
  };
  
  return (
    <div>
      <h1>Bill of Materials</h1>
      <p>
        Bill of Materials, or BOM allows you to manage inventory quantities per project. You can reduce quantities for each PCB you produce, check which parts you need to buy more of and analyze costs.<br/><br/>
        Choose or create the project to manage BOM for.<br/>
      </p>
      <Segment>
        <div style={{float: 'right', verticalAlign: 'middle', fontSize: '0.9em'}}>
          <Dropdown 
            selection
            options={itemsPerPageOptions}
            value={pageSize}
            className='small labeled'
            onChange={handlePageSizeChange}
          />
          <span>records per page</span>
        </div>
        <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size='mini' />
        <Segment loading={loading}>
          <div style={{ minHeight: '35px' }}>
            <Button primary onClick={handleShowAdd} icon size='mini' floated='right'><Icon name='plus' /> Add BOM Project</Button>
          </div>
          <div>
            {addVisible &&
              <Segment>
                <Form onSubmit={onSubmit}>
                  <Form.Input width={6} label='Name' required placeholder='555 Timer Project' focus value={project.name} onChange={handleChange} name='name' />
                  <Form.Field width={10} control={TextArea} label='Description' value={project.description} onChange={handleChange} name='description' style={{height: '60px'}} />
                  <Form.Group>
                    <Form.Input width={6} label='Location' required placeholder='New York' focus value={project.location} onChange={handleChange} name='location' />
                    <Form.Dropdown width={4} label='Color' selection value={project.color} options={colors} onChange={handleChange} name='color' />
                  </Form.Group>
                  <Button primary type='submit' icon><Icon name='save' /> Save</Button>
                </Form>
              </Segment>
            }
          </div>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'name' ? direction : null} onClick={handleSort('name')}>Project</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'description' ? direction : null} onClick={handleSort('description')}>Description</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={handleSort('location')}>Location</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'parts' ? direction : null} onClick={handleSort('parts')}>Parts</Table.HeaderCell>
                <Table.HeaderCell></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {projects.map(p =>
                <Table.Row key={p.projectId} onClick={e => handleLoadBom(e, p)}>
                  <Table.Cell textAlign='center'><Label circular {...(_.find(ProjectColors, c => c.value === p.color).name !== '' && { color: _.find(ProjectColors, c => c.value === p.color).name })} size='mini' /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' type='text' transparent name='name' onBlur={saveColumn} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' transparent name='description' onBlur={saveColumn} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.description || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' transparent name='location' onBlur={saveColumn} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.location || ''} fluid /></Table.Cell>
                  <Table.Cell>{p.parts}</Table.Cell>
                  <Table.Cell textAlign='center'><Button icon='delete' size='tiny' onClick={e => handleDelete(e, p)} /></Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </Segment>
      </Segment>
    </div>
  );
};

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => (
  <Boms {...props} params={useParams()} history={useNavigate()} location={window.location} />
);