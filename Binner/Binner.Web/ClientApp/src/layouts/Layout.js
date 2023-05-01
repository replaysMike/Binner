import React, { useCallback, useEffect, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Container } from "reactstrap";
import { Header } from "./Header";
import { Footer } from "./Footer";
import customEvents from '../common/customEvents';
import 'react-toastify/dist/ReactToastify.css';

export function Layout(props) {
  const { t } = useTranslation('en');
  const noop = t('noop', "-do-not-translate-");
  const [version, setVersion] = useState(null);

  const updateVersion = useCallback((data) => {
    setVersion(data.version);
  }, [setVersion]);

  useEffect(() => {
    customEvents.subscribe("version", (data) => updateVersion(data));
  }, [updateVersion]);

  return (
    <div className="centered" style={{marginBottom: '50px', position: 'relative', zIndex: '50', textAlign: 'left'}}>
      
      <div id="banner" />
      <Container>
        <Header />
        {props.children}
        <Footer />
      </Container>
      <div className={`promo ${window.avatar ? 'masked' : ''}`}>
        <div style={{marginBottom: '10px'}}>{version ? <span>{t('footer.version', "Version")}: {version}</span> : ""}</div>
        <span>
          <Trans i18nKey="footer.promo">
            Try <a href="https://binner.io">Binner Cloud Free</a>
          </Trans>
        </span>
      </div>
    </div>
  );
}
