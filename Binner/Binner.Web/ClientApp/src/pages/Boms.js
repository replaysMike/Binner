import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import { Trans, useTranslation } from "react-i18next";
import { Icon, Input, Label, Button, TextArea, Form, Table, Segment, Breadcrumb, Pagination, Dropdown, Confirm } from "semantic-ui-react";
import { FormHeader } from "../components/FormHeader";
import _ from 'underscore';
import { toast } from "react-toastify";
import { fetchApi } from '../common/fetchApi';
import { ProjectColors } from "../common/Types";

export function Boms (props) {
  const { t } = useTranslation();
  const defaultProject = {
    name: '',
    description: '',
    location: '',
    color: 0,
    loading: false,
  };
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(parseInt(localStorage.getItem("bomsRecordsPerPage")) || 5);
  const [loading, setLoading] = useState(true);
  const [addVisible, setAddVisible] = useState(false);
  const [project, setProject] = useState(defaultProject);
  const [projects, setProjects] = useState([]);
  const [changeTracker, setChangeTracker] = useState([]);
  const [lastSavedProjectId, setLastSavedProjectId] = useState(0);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [noRemainingData, setNoRemainingData] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmProjectDeleteContent] = useState(null);
  const [confirmDeleteSelectedProject, setConfirmDeleteSelectedProject] = useState(null);

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

  const loadProjects = async (page, pageSize, reset = false) => {
    setLoading(true);
    let endOfData = false;
    const response = await fetchApi(`api/bom/list?orderBy=DateCreatedUtc&direction=Descending&results=${pageSize}&page=${page}`);
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
    setTotalPages(Math.ceil(newData.length / pageSize));
    setNoRemainingData(endOfData);
    setLoading(false);
  };

  useEffect(() => {
    loadProjects(page, pageSize);
  }, [page]);

  const handleNextPage = () => {
    if (noRemainingData) return;

    const nextPage = page + 1;
    loadProjects(nextPage, pageSize);
  };

  const handlePageSizeChange = async (e, control) => {
    const newPageSize = parseInt(control.value);
    const newTotalPages = Math.ceil(projects.length / newPageSize);
    let newPage = page;
    setPageSize(newPageSize);
    setTotalPages(newTotalPages);
    localStorage.setItem("bomsRecordsPerPage", newPageSize);
    // redirect to the last page if less pages
    if (page > newTotalPages) {
      newPage = newTotalPages;
      setPage(newPage);
    }

    await loadProjects(newPage, newPageSize, true);
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
    props.history(`/bom/${encodeURIComponent(p.name)}`);
  };

  const onCreateProject = async () => {
    const request = {
      name: project.name,
      description: project.description,
      location: project.location,
      color: Number.parseInt(project.color) || 0
    };
    const response = await fetchApi('api/project', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      // reset form
      setProject(defaultProject);
      setAddVisible(false);
      loadProjects(page, pageSize, true);
    }
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
      if (project.length > 0) await inlineSave(project[0]);
    });
    setProjects(projects);
    setChangeTracker([]);
  };

  const inlineSave = async (project) => {
    const p = _.where(projects, { projectId: project.projectId });
    p.loading = false;
    project.color = Number.parseInt(project.color) || 0;
    const request = {
      projectId: project.projectId,
      name: project.name,
      description: project.description,
      location: project.location,
      color: project.color,
    };
    const response = await fetchApi('api/project', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
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

  const confirmDeleteOpen = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteSelectedProject(p);
    setConfirmDeleteIsOpen(true);
    setConfirmProjectDeleteContent(
      <p>
        {t('confirm.deleteProject', 'Are you sure you want to delete this project and your entire BOM?')}
        <br />
        <br />
        <b>
          {p.name}
        </b>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const handleDeleteProject = async (e) => {
    if (!confirmDeleteSelectedProject)
      return null;

    const response = await fetchApi('api/project', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ projectId: confirmDeleteSelectedProject.projectId})
    });
    if (response.responseObject.ok) {
      setConfirmDeleteIsOpen(false);
      await loadProjects(page, pageSize, true);   
    } else {
      toast.error(t('deleteProjectFailed', 'Failed to delete project!'));
    }
  };

  const focusColumn = (e) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
    setConfirmDeleteSelectedProject(null);
  };
  
  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => props.history("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.bom', "BOM")}</Breadcrumb.Section>
      </Breadcrumb>
      <Confirm
        className="confirm"
        header={t('label.deleteProject', 'Delete Project')}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDeleteProject}
        content={confirmPartDeleteContent}
      />

      <FormHeader name={t('page.bom.header.title', 'Bill of Materials')} to="..">
        <Trans i18nKey="page.boms.header.description">
        Bill of Materials, or BOM allows you to manage inventory quantities per project. You can reduce quantities for each PCB you produce, check which parts you need to buy more of and analyze costs.<br/><br/>
        Choose or create the project to manage BOM for.<br/>
        </Trans>
			</FormHeader>

      <div style={{ minHeight: '35px' }}>
        <Button primary onClick={handleShowAdd} icon size='mini' floated='right'><Icon name='plus' /> <Trans i18nKey="button.addBomProject">Add BOM Project</Trans></Button>
      </div>
      <div>
        {addVisible &&
          <Segment style={{marginBottom: '10px'}}>
            <Form onSubmit={onCreateProject}>
              <Form.Input width={6} label={t('label.name', 'Name')} required placeholder='555 Timer Project' focus value={project.name} onChange={handleChange} name='name' />
              <Form.Field width={10} control={TextArea} label={t('label.description', 'Description')} value={project.description} onChange={handleChange} name='description' style={{height: '60px'}} />
              <Form.Group>
                <Form.Input width={6} label={t('label.location', 'Location')} placeholder='New York' focus value={project.location} onChange={handleChange} name='location' />
                <Form.Dropdown width={4} label={t('label.color', 'Color')} selection value={project.color} options={colors} onChange={handleChange} name='color' />
              </Form.Group>
              <Button primary type='submit' icon><Icon name='save' /> <Trans i18nKey="button.save">Save</Trans></Button>
            </Form>
          </Segment>
        }
      </div>
      <Segment style={{marginTop: '0'}}>
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
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'name' ? direction : null} onClick={handleSort('name')}><Trans i18nKey="label.project">Project</Trans></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'description' ? direction : null} onClick={handleSort('description')}><Trans i18nKey="label.description">Description</Trans></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={handleSort('location')}><Trans i18nKey="label.location">Location</Trans></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'partCount' ? direction : null} onClick={handleSort('partCount')}><Trans i18nKey="label.parts">Parts</Trans></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'pcbCount' ? direction : null} onClick={handleSort('pcbCount')}><Trans i18nKey="label.pcbs">Pcbs</Trans></Table.HeaderCell>
                <Table.HeaderCell width={2}></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {projects.map(p =>
                <Table.Row key={p.projectId} onClick={e => handleLoadBom(e, p)}>
                  <Table.Cell textAlign='center' style={{verticalAlign: 'middle'}}><Label circular {...(_.find(ProjectColors, c => c.value === p.color).name !== '' && { color: _.find(ProjectColors, c => c.value === p.color).name })} size='mini' /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='name' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' className="inline-editable" transparent name='description' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.description || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' className="inline-editable" transparent name='location' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.location || ''} fluid /></Table.Cell>
                  <Table.Cell>{p.partCount}</Table.Cell>
                  <Table.Cell>{p.pcbCount}</Table.Cell>
                  <Table.Cell textAlign='center'><Button icon='edit' size='tiny' onClick={e => { e.preventDefault(); e.stopPropagation(); props.history(`/project/${p.name}`); }} title="Edit project" /> <Button icon='delete' size='tiny' onClick={e => confirmDeleteOpen(e, p)} title="Delete project" /></Table.Cell>
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