import { useEffect, useCallback, useState } from "react";
import { Checkbox } from "semantic-ui-react";
import PropTypes from "prop-types";
import customEvents from '../common/customEvents';
import { getLocalData, setLocalData } from "../common/storage";
import "./ThemeChangeToggle.css";

export function ThemeChangeToggle({ dark = false, ...rest }) {
  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'themeChangeToggle' })
  };

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'themeChangeToggle', ...options });
  };

  const [toggled, setToggled] = useState(getViewPreference('isDarkMode') || false);

  const getHtmlEl = () => document.getElementsByTagName('html');

  const isThemeDarkMode = (els) => {
    if (els && els.length > 0) {
      const el = els[0];
      if (el.dataset.colorMode === 'dark') return true;
    }
    return false;
  };

  const handleThemeChange = (e) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (document) {
      const els = getHtmlEl();
      if (isThemeDarkMode(els)) {
        els[0].dataset.colorMode = 'light';
        customEvents.notifySubscribers("theme", 'light');
        setViewPreference('isDarkMode', false);
        setToggled(false);
      } else {
        els[0].dataset.colorMode = 'dark';
        customEvents.notifySubscribers("theme", 'dark');
        setViewPreference('isDarkMode', true);
        setToggled(true);
      }
    }
  };

  /**
     * If the theme is changed by an outside control, indicate its state on the toggle checkbox
     */
  const updateTheme = useCallback((theme) => {
      if (theme === 'dark')
        setToggled(true);
      else
        setToggled(false);
    }, []);
  
    useEffect(() => {
      customEvents.subscribe("theme", (data) => updateTheme(data));
    }, [updateTheme]);

  const isDarkModeRendered = dark === true || isThemeDarkMode(getHtmlEl());

  return (
    <div className={`theme-selection${isDarkModeRendered ? ' dark' : ''}`}>
      <Checkbox toggle onChange={handleThemeChange} checked={toggled} />
    </div>);
};

ThemeChangeToggle.propTypes = {
  dark: PropTypes.bool,
};