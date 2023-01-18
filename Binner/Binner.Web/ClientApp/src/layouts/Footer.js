import React, { useState, useEffect, useCallback } from 'react';
import customEvents from '../common/customEvents';

export function Footer(props) {
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
      {version ? <span>Version: {version}</span> : ""}
    </div>
  );
}
