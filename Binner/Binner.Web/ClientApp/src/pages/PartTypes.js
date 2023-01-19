import React, { Component } from 'react';
import _ from 'underscore';
import { Table, Input, Button, Segment, Form, Icon, Dropdown } from 'semantic-ui-react';
import { fetchApi } from '../common/fetchApi';

export class PartTypes extends Component {
  static displayName = PartTypes.name;

  constructor(props) {
    super(props);
    this.state = {
      partTypes: [],
      partType: {
        partTypeId: 0,
        name: '',
        parentPartTypeId: '',
        loading: false,
      },
      partTypeOptions: [],
      changeTracker: [],
      lastSavedPartTypeId: 0,
      addVisible: false,
      page: 1,
      records: 10,
      column: null,
      direction: null,
      noRemainingData: false,
      lastSavedProjectId: 0,
      loading: true
    };

    this.handleChange = this.handleChange.bind(this);
    this.handleInlineChange = this.handleInlineChange.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.handleSort = this.handleSort.bind(this);
    this.handleShowAdd = this.handleShowAdd.bind(this);
    this.handleDelete = this.handleDelete.bind(this);
    this.save = this.save.bind(this);
    this.saveColumn = this.saveColumn.bind(this);
  }

  async componentDidMount() {
    await this.loadPartTypes(this.state.page);
  }

  async loadPartTypes(page, reset = false) {
    const { partTypes } = this.state;
    this.setState({ loading: true });
    let endOfData = false;
    const response = await fetchApi(`partType/list`);
    const pageOfData = response.data;
    if (pageOfData && pageOfData.length === 0)
      endOfData = true;
    let newData = [];
    if (reset)
      newData = [...pageOfData];
    else
      newData = [...partTypes, ...pageOfData];

    const blankRow = { key: 999, value: null, text: '' };
    const partTypeOptions = _.map(newData, function (item) {
      return {
        key: item.partTypeId,
        value: item.partTypeId,
        text: item.name
      };
    });
    partTypeOptions.unshift(blankRow);
    this.setState({ partTypes: newData, page, noRemainingData: endOfData, partTypeOptions, loading: false });
  }

  handleSort = (clickedColumn) => () => {
    const { column, partTypes, direction } = this.state

    if (column !== clickedColumn) {
      this.setState({
        column: clickedColumn,
        partTypes: _.sortBy(partTypes, [clickedColumn]),
        direction: 'ascending',
      })
    } else {
      this.setState({
        partTypes: partTypes.reverse(),
        direction: direction === 'ascending' ? 'descending' : 'ascending',
      })
    }
  }

  handleChange(e, control) {
    const { partType } = this.state;
    partType[control.name] = control.value;
    this.setState({ partType });
  }

  async handleInlineChange(e, control, partType) {
    const { partTypes, changeTracker } = this.state;
    partType[control.name] = control.value;
    let changes = [...changeTracker];
    if (_.where(changes, { partTypeId: partType.partTypeId }).length === 0)
      changes.push({ partTypeId: partType.partTypeId });
    this.setState({ partTypes, changeTracker: changes });
  }

  /**
   * Save new project
   * @param {any} e
   */
  async onSubmit(e) {
    const { partType } = this.state;
    const request = {
      name: partType.name,
      parentPartTypeId: Number.parseInt(partType.parentPartTypeId)
    };
    const response = await fetchApi('partType', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      // reset form
      this.setState({
        partType: {
          name: '',
          parentPartTypeId: '',
          loading: false,
        },
        addVisible: false
      });
      await this.loadPartTypes(this.state.page, true);
    }
  }

  async onDelete(partType) {
    await fetchApi('partType', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ partTypeId: partType.partTypeId })
    });
    await this.loadPartTypes(this.state.page, true);
  }

  async saveColumn(e) {
    const { partTypes, changeTracker } = this.state;
    changeTracker.forEach(async (val) => {
      const partType = _.where(partTypes, { partTypeId: val.partTypeId }) || [];
      if (partType.length > 0)
        await this.save(partType[0]);
    });
    this.setState({ partTypes, changeTracker: [] });
  }

  async save(partType) {
    const { partTypes } = this.state;
    const p = _.where(partTypes, { partTypeId: partType.partTypeId });
    p.loading = false;
    this.setState({ partTypes });
    let lastSavedPartTypeId = 0;
    const request = {
      partTypeId: Number.parseInt(partType.partTypeId),
      name: partType.name,
      parentPartTypeId: Number.parseInt(partType.parentPartTypeId)
    };
    const response = await fetchApi('partType', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      lastSavedPartTypeId = partType.partTypeId;
    }
    else
      console.log('failed to save partType');
    p.loading = false;
    this.setState({ partTypes, lastSavedPartTypeId });
  }

  handleShowAdd(e) {
    this.setState({ addVisible: !this.state.addVisible });
  }

  async handleDelete(e, partType) {
    await this.onDelete(partType);
  }

  renderPartTypes(partTypes, column, direction) {
    const { partType, addVisible, partTypeOptions, loading } = this.state;
    return (
      <Segment loading={loading}>
        <div style={{ minHeight: '35px' }}>
          <Button onClick={this.handleShowAdd} icon size='mini' floated='right'><Icon name='file' /> Add Part Type</Button>
        </div>
        <div>
          {addVisible &&
            <Segment>
              <Form onSubmit={this.onSubmit}>
              <Form.Input width={6} label='Name' required placeholder='Resistors' focus value={partType.name} onChange={this.handleChange} name='name' />
              <Form.Dropdown width={4} label='Parent' selection value={partType.parentPartTypeId} options={partTypeOptions} onChange={this.handleChange} name='color' />
                <Button primary type='submit' icon><Icon name='save' /> Save</Button>
              </Form>
            </Segment>
          }
        </div>
        <Table compact celled sortable selectable striped size='small'>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell sorted={column === 'name' ? direction : null} onClick={this.handleSort('name')}>Part Type</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'parentPartTypeId' ? direction : null} onClick={this.handleSort('location')}>Parent</Table.HeaderCell>
              <Table.HeaderCell sorted={column === 'parts' ? direction : null} onClick={this.handleSort('parts')}>Parts</Table.HeaderCell>
              <Table.HeaderCell></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {partTypes.map(p =>
              <Table.Row key={p.partTypeId} onClick={this.handleClick}>
                <Table.Cell><Input labelPosition='left' type='text' transparent name='name' onBlur={this.saveColumn} onChange={(e, control) => this.handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
                <Table.Cell><Dropdown width={4} label='Parent' selection name='parentPartTypeId' value={p.parentPartTypeId} options={partTypeOptions} onBlur={this.saveColumn} onChange={(e, control) => this.handleInlineChange(e, control, p)} /></Table.Cell>
                <Table.Cell>{p.parts}</Table.Cell>
                <Table.Cell textAlign='center'><Button icon='delete' size='tiny' onClick={e => this.handleDelete(e, p)} /></Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table>
      </Segment>
    );
  }

  render() {
    const { partTypes, column, direction } = this.state;
    let contents = this.renderPartTypes(partTypes, column, direction);

    return (
      <div>
        <h1>Part Types</h1>
        <p>
          Part Types allow you to separate your parts by type. <i>Parent</i> types allow for unlimited part type hierarchy.<br/>
          For example: OpAmps may be a sub-type of IC's, so OpAmp's parent type is IC.
        </p>
        {contents}
      </div>
    );
  }
}
