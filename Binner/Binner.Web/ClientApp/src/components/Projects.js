import React, { Component } from 'react';

export class Projects extends Component {
  static displayName = Projects.name;

  constructor(props) {
    super(props);
    this.state = { loading: true };
  }

  componentDidMount() {
  }

  render() {
    return (
      <div>
        <h1>Not supported yet!</h1>
        <p>This feature is under development.</p>
      </div>
    );
  }
}
