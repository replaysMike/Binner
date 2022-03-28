import React from "react";
import { useLocation } from "react-router-dom";
import { Container } from "reactstrap";
import { NavMenu } from "./NavMenu";
import { Footer } from "./Footer";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

export function Layout(props) {
  const location = useLocation();

  return (
    <div className="centered" style={{marginBottom: '50px'}}>
      <ToastContainer newestOnTop={true} autoClose={1500} />
      <Container>
        <NavMenu />
        <div style={{textAlign: 'left'}}>
          {props.children}
          <Footer />
        </div>
      </Container>
    </div>
  );
}
