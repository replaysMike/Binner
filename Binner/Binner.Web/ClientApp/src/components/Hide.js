import React, { useEffect, useState } from "react";
import { useTranslation, Trans } from "react-i18next";
import { Icon, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import "./Hide.css";

/**
 * Clipboard copy/paste control
 * @param {props} props 
 * @returns 
 */
export function Hide({ element = 'token', size, ...rest }) {
  const { t } = useTranslation();
  const [iconName, setIconName] = useState('eye');

  useEffect(() => {
    const el = document.getElementsByName(element);
    if (el.length > 0) el[0].classList.add('hidden');
  }, []);

  const handleHide = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const el = document.getElementsByName(element);
    if (el.length > 0) {
      el[0].classList.toggle('hidden');
      if (iconName === 'eye')
        setIconName('eye slash');
      else
        setIconName('eye');
    }
    else
      console.error(`Element '${element}' not found`);
  };

  return (<Popup
    content={t('comp.hide.toggleShow', "Toggle show/hide")}
    trigger={<Icon
      className="hider"
      name={iconName}
      size={size}
      {...rest}
      onClick={handleHide}
    />}
  />);
};

Hide.propTypes = {
  /** Element to show/hide */
  element: PropTypes.string,
  /** Color of icon */
  color: PropTypes.string,
  /** Size of icon */
  size: PropTypes.string
};
