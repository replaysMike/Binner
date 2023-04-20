import React from "react";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

export function Layout(props) {
  return (
    <div className="centered" style={{marginBottom: '50px'}}>
      <ToastContainer newestOnTop={true} autoClose={5000} hideProgressBar={true} theme="colored" position="top-center" />
      <div id="banner" />
      <Container>
      <Header />
        <div style={{textAlign: 'left'}}>
          {props.children}
          <Footer />
        </div>
      </Container>
    </div>
  );
}
