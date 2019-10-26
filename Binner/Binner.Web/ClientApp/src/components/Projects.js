import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label, Button, Segment, Form, TextArea, Icon } from 'semantic-ui-react';
import { ProjectColors } from './Types';

export class Projects extends Component {
  static displayName = Projects.name;

  constructor(props) {
    super(props);
    this.state = {
      projects: [],
      project: {
        name: '',
        description: '',
        color: '',
      },
      addProjectVisible: false,
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      lastSavedProjectId: 0,
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
    this.onSubmit = this.onSubmit.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);
    this.handleAddProject = this.handleAddProject.bind(this);
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

  /**
   * Save new project
   * @param {any} e
   */
  async onSubmit(e) {
    const { project } = this.state;
    const response = await fetch('project', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(project)
    });
    if (response.status === 200) {
      // reset form
      this.setState({
        project: {
          name: '',
          description: '',
          color: '',
        },
        addProjectVisible: false
      });
      await this.loadProjects(this.state.page);
    }
  }

  handleAddProject(e) {
    this.setState({ addProjectVisible: !this.state.addProjectVisible });
  }

  renderProjects(projects, column, direction) {
    const { project, lastSavedProjectId, colors, addProjectVisible } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
        <div>
          <div style={{ minHeight: '35px' }}>
            <Button onClick={this.handleAddProject} icon size='mini' floated='right'><Icon name='file' /> Add Project</Button>
          </div>
          <div>
            {addProjectVisible &&
              <Segment>
                <Form onSubmit={this.onSubmit}>
                  <Form.Input width={6} label='Name' required placeholder='555 Timer Project' focus value={project.name} onChange={this.handleChange} name='name' />
                  <Form.Field width={10} control={TextArea} label='Description' value={project.description} onChange={this.handleChange} name='description' />
                  <Form.Dropdown width={4} label='Color' selection value={project.color} options={colors} onChange={this.handleChange} name='color' />
                  <Button primary type='submit' icon><Icon name='save' /> Save</Button>
                </Form>
              </Segment>
            }
          </div>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell sorted={column === 'name' ? direction : null} onClick={this.handleSort('name')}>Project</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'description' ? direction : null} onClick={this.handleSort('description')}>Description</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'location' ? direction : null} onClick={this.handleSort('location')}>Location</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'parts' ? direction : null} onClick={this.handleSort('parts')}>Parts</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {projects.map(p =>
                <Table.Row key={p.projectId} onClick={this.handleClick}>
                  <Table.Cell><Label ribbon={lastSavedProjectId === p.projectId}>{p.name}</Label></Table.Cell>
                  <Table.Cell>{p.description}</Table.Cell>
                  <Table.Cell>{p.location}</Table.Cell>
                  <Table.Cell>{p.parts}</Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </div>
      </Visibility>
    );
  }

  render() {
    const { projects, column, direction, loading } = this.state;
    let contents = loading
      ? <p><em>Loading...</em></p>
      : this.renderProjects(projects, column, direction);

    return (
      <div>
        <h1>Projects</h1>
        {contents}
      </div>
    );
  }
}
