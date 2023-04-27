import React, { useState, useEffect, useCallback } from 'react';
import {useNavigate} from "react-router-dom";
import { useTranslation } from "react-i18next";
import customEvents from '../common/customEvents';

export function Footer(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [subscription, setSubscription] = useState(null);
  const [version, setVersion] = useState(null);
  const [avatar, setAvatar] = useState(false);

  const updateVersion = useCallback((data, subscriptionId) => {
    setVersion(data.version);
  }, [setVersion]);

  const updateAvatar = useCallback((enabled) => {
    setAvatar(enabled);
  }, [setAvatar]);

  useEffect(() => {
    setSubscription(customEvents.subscribe("version", (data, subscriptionId) => updateVersion(data, subscriptionId)));
    customEvents.subscribe("avatar", (enabled) => updateAvatar(enabled))
  }, [updateVersion, setSubscription, updateAvatar]);

  return (
    <footer className={`footer ${avatar && 'active'} centered`} id="footer">
      <div>
        <div>{version ? <span>{t('footer.version', "Version")}: {version}</span> : ""}</div>
      </div>
    </footer>
  );
}
