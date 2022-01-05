import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label, Button, Segment, Form, TextArea, Icon } from 'semantic-ui-react';
import { ProjectColors } from '../common/Types';

export class Projects extends Component {
  static displayName = Projects.name;

  constructor(props) {
    super(props);
    this.state = {
      projects: [],
      project: {
        name: '',
        description: '',
        location: '',
        color: 0,
        loading: false,
      },
      changeTracker: [],
      lastSavedProjectId: 0,
      addVisible: false,
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      loading: true,
      colors: _.map(ProjectColors, function (c) {
        return {
          key: c.value,
          value: c.value,
          text: c.name,
          label: { ...(c.name !== '' && { color: c.name }), circular: true, empty: true, size: 'tiny' }
        }
      })
    };

    this.handleChange = this.handleChange.bind(this);
    this.handleClick = this.handleClick.bind(this);
    this.handleInlineChange = this.handleInlineChange.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handleShowAdd = this.handleShowAdd.bind(this);
    this.handleDelete = this.handleDelete.bind(this);
    this.save = this.save.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
  }

  async componentDidMount() {
    await this.loadProjects(this.state.page);
  }

  async loadProjects(page, reset = false) {
    const { records, projects } = this.state;
    this.setState({ loading: true });
    let endOfData = false;
    const response = await fetch(`project/list?orderBy=DateCreatedUtc&direction=Descending&results=${records}&page=${page}`);
    const pageOfData = await response.json();
    pageOfData.forEach(function (element) {
      element.loading = true;
    });
    if (pageOfData && pageOfData.length === 0)
      endOfData = true;
    let newData = [];
    if (reset)
      newData = [...pageOfData];
    else
      newData = [...projects, ...pageOfData];
    this.setState({ projects: newData, page, noRemainingData: endOfData, loading: false });
  }

  handleSort = (clickedColumn) => () => {
    const { column, projects, direction } = this.state

    if (column !== clickedColumn) {
      this.setState({
        column: clickedColumn,
        projects: _.sortBy(projects, [clickedColumn]),
        direction: 'ascending',
      })
    } else {
      this.setState({
        projects: projects.reverse(),
        direction: direction === 'ascending' ? 'descending' : 'ascending',
      })
    }
  }

  handleNextPage() {
    const { page, noRemainingData } = this.state;
    if (noRemainingData) return;

    const nextPage = page + 1;
    this.loadProjects(nextPage);
  }

  handleChange(e, control) {
    const { project } = this.state;
    project[control.name] = control.value;
    this.setState({ project });
  }

  handleClick(e, control) {
  }

  handleInlineChange(e, control, project) {
    const { projects, changeTracker } = this.state;
    project[control.name] = control.value;
    let changes = [...changeTracker];
    if (_.where(changes, { projectId: project.projectId }).length === 0)
      changes.push({ projectId: project.projectId });
    this.setState({ projects, changeTracker: changes });
  }

  /**
   * Save new project
   * @param {any} e
   */
  async onSubmit(e) {
    const { project } = this.state;
    const request = {
      name: project.name,
      description: project.description,
      location: project.location,
      color: Number.parseInt(project.color) || 0
    };
    const response = await fetch('project', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.status === 200) {
      // reset form
      this.setState({
        project: {
          name: '',
          description: '',
          location: '',
          color: 0,
          loading: false,
        },
        addProjectVisible: false
      });
      await this.loadProjects(this.state.page, true);
    }
  }

  async onDelete(project) {
    await fetch('project', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ projectId: project.projectId})
    });
    await this.loadProjects(this.state.page, true);
  }

  async saveColumn(e) {
    const { projects, changeTracker } = this.state;
    changeTracker.forEach(async (val) => {
      const project = _.where(projects, { projectId: val.projectId }) || [];
      if (project.length > 0)
        await this.save(project[0]);
    });
    this.setState({ projects, changeTracker: [] });
  }

  async save(project) {
    const { projects } = this.state;
    const p = _.where(projects, { projectId: project.projectId });
    p.loading = false;
    this.setState({ projects });
    let lastSavedProjectId = 0;
    project.color = Number.parseInt(project.color) || 0;
    const request = {
      name: project.name,
      description: project.description,
      location: project.location,
      color: project.color
    };
    const response = await fetch('project', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.status === 200) {
      const data = await response.json();
      lastSavedProjectId = data.projectId;
    }
    else
      console.log('failed to save project');
    p.loading = false;
    this.setState({ projects, lastSavedProjectId });
  }

  handleShowAdd(e) {
    this.setState({ addVisible: !this.state.addVisible });
  }

  async handleDelete(e, project) {
    await this.onDelete(project);
  }

  renderProjects(projects, column, direction) {
    const { project, colors, addVisible, loading } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <Segment loading={loading}>
          <div style={{ minHeight: '35px' }}>
            <Button onClick={this.handleShowAdd} icon size='mini' floated='right'><Icon name='file' /> Add Project</Button>
          </div>
          <div>
            {addVisible &&
              <Segment>
                <Form onSubmit={this.onSubmit}>
                  <Form.Input width={6} label='Name' required placeholder='555 Timer Project' focus value={project.name} onChange={this.handleChange} name='name' />
                  <Form.Field width={10} control={TextArea} label='Description' value={project.description} onChange={this.handleChange} name='description' />
                  <Form.Input width={6} label='Location' required placeholder='New York' focus value={project.location} onChange={this.handleChange} name='location' />
                  <Form.Dropdown width={4} label='Color' selection value={project.color} options={colors} onChange={this.handleChange} name='color' />
                  <Button primary type='submit' icon><Icon name='save' /> Save</Button>
                </Form>
              </Segment>
            }
          </div>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell></Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'name' ? direction : null} onClick={this.handleSort('name')}>Project</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'description' ? direction : null} onClick={this.handleSort('description')}>Description</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>Location</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'parts' ? direction : null} onClick={this.handleSort('parts')}>Parts</Table.HeaderCell>
                <Table.HeaderCell></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {projects.map(p =>
                <Table.Row key={p.projectId} onClick={this.handleClick}>
                  <Table.Cell textAlign='center'><Label circular {...(_.find(ProjectColors, c => c.value === p.color).name !== '' && { color: _.find(ProjectColors, c => c.value === p.color).name })} size='mini' /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' type='text' transparent name='name' onBlur={this.saveColumn} onChange={(e, control) => this.handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' transparent name='description' onBlur={this.saveColumn} onChange={(e, control) => this.handleInlineChange(e, control, p)} value={p.description || ''} fluid /></Table.Cell>
                  <Table.Cell><Input type='text' transparent name='location' onBlur={this.saveColumn} onChange={(e, control) => this.handleInlineChange(e, control, p)} value={p.location || ''} fluid /></Table.Cell>
                  <Table.Cell>{p.parts}</Table.Cell>
                  <Table.Cell textAlign='center'><Button icon='delete' size='tiny' onClick={e => this.handleDelete(e, p)} /></Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </Segment>
      </Visibility>
    );
  }

  render() {
    const { projects, column, direction } = this.state;
    let contents = this.renderProjects(projects, column, direction);

    return (
      <div>
        <h1>Projects</h1>
        <p>
          Projects allow you to associate parts for a particular project.<br/>
          You will be able to generate part lists and reports by Project.
        </p>
        {contents}
      </div>
    );
  }
}
