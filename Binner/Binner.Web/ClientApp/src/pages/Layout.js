import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { Footer } from './Footer';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <NavMenu />
        <Container fluid={true} className={"binner-container"}>
            {this.props.children}
         <Footer />
        </Container>
      </div>
    );
  }
}
