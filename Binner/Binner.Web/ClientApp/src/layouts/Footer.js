import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation, Trans } from "react-i18next";
import customEvents from '../common/customEvents';

export function Footer(props) {
  const { t } = useTranslation();
  const [subscription, setSubscription] = useState(null);
  const [version, setVersion] = useState(null);

  const updateVersion = useCallback((data, subscriptionId) => {
    setVersion(data.version);
  }, [setVersion]);

  useEffect(() => {
    setSubscription(customEvents.subscribe("version", (data, subscriptionId) => updateVersion(data, subscriptionId)));
  }, [updateVersion, setSubscription]);

  return (
    <div className='footer centered'>
      <div>{version ? <span>{t('footer.version', "Version")}: {version}</span> : ""}</div>
      <div className='promo'>
        <Trans i18nKey="footer.promo">
        Try <a href="https://binner.io">Binner Cloud Free</a>
        </Trans>
      </div>
    </div>
  );
}
