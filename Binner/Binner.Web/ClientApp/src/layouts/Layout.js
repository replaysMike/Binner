import React from "react";
import { Trans } from "react-i18next";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';

export function Layout(props) {
  return (
    <div className="centered" style={{marginBottom: '50px', position: 'relative', zIndex: '50', textAlign: 'left'}}>
      <ToastContainer newestOnTop={true} autoClose={5000} hideProgressBar={true} theme="colored" position="top-center" />
      <div id="banner" />
      <Container>
        <Header />
        {props.children}
        <Footer />
      </Container>
      <div className="promo">
        <span>
          <Trans i18nKey="footer.promo">
            Try <a href="https://binner.io">Binner Cloud Free</a>
          </Trans>
        </span>
      </div>
    </div>
  );
}
