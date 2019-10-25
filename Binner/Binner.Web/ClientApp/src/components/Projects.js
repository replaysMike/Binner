import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Visibility, Input, Label } from 'semantic-ui-react';

export class Projects extends Component {
  static displayName = Projects.name;

  constructor(props) {
    super(props);
    this.state = {
      projects: [],
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      lastSavedProjectId: 0,
      loading: true
    };
    this.handleSort = this.handleSort.bind(this);
    this.handleNextPage = this.handleNextPage.bind(this);

  }

  async componentDidMount() {
    await this.loadProjects(this.state.page);
  }

  async loadProjects(page, reset = false) {
    const { records, projects } = this.state;
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

  renderProjects(parts, column, direction) {
    const { keyword, lastSavedProjectId } = this.state;
    return (
      <Visibility onBottomVisible={this.handleNextPage} continuous>
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
            {parts.map(p =>
              <Table.Row key={p.partId} onClick={this.handleClick}>
                <Table.Cell><Label ribbon={lastSavedProjectId === p.projectId}>{p.name}</Label></Table.Cell>
                <Table.Cell>{p.description}</Table.Cell>
                <Table.Cell>{p.location}</Table.Cell>
                <Table.Cell>{p.parts}</Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table>
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
