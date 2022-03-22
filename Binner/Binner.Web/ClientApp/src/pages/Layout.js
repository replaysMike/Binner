import React, { Component } from "react";
import { Container } from "reactstrap";
import { NavMenu } from "./NavMenu";
import { Footer } from "./Footer";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <div>
        <ToastContainer newestOnTop={true} autoClose={1500} />
        <Container fluid={true} className={"binner-container"}>
          <NavMenu />
          {this.props.children}
          <Footer />
        </Container>
      </div>
    );
  }
}
