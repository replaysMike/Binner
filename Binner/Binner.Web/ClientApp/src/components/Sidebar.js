import { useEffect, useState } from "react";
import { Button } from "semantic-ui-react";
import PropTypes from "prop-types";
import "./Sidebar.css";

/**
 * Sidebar slideout control
 */
export function Sidebar({ children, onHide, onShow, visible = null, ...rest }) {
  const [isOpen, setIsOpen] = useState(false);

  const getControlElement = () => document.getElementById('sidebar');

  const onClick = (e, control) => {
    const el = getControlElement();
    if (el.contains(e.target)) {
      // click is inside sidebar
    } else {
      // click is outside sidebar
      setNewState(false);
    }
  };

  useEffect(() => {
    if (document)
      document.addEventListener("mousedown", onClick);
    return () => {
      if (document)
        document.removeEventListener("mousedown", onClick);
    };
  }, []);

  useEffect(() => {
    setIsOpen(visible);
    setNewState(visible);
  }, [visible]);

  const getHandle = () => {
    if (isOpen)
      return <Button icon="angle double left" onClick={handleHandleClick} />;
    return <Button icon="angle double right" onClick={handleHandleClick} />;
  }

  const handleHandleClick = (e, control) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    const newIsOpen = !isOpen;
    setNewState(newIsOpen, e, control);
  };

  const setNewState = (newIsOpen, e, control) => {
    setIsOpen(newIsOpen);
    const el = getControlElement();
    //e.style.transform = 'translate3d(0,0,0)'; 
    if (newIsOpen) {
      el.style.left = '0px';
      if (onShow) onShow(e, control);
    } else {
      el.style.left = '-450px';
      if (onHide) onHide(e, control);
    }
  };

  return (<div {...rest} className={`sidebar${isOpen ? ' open' : ''}`} id='sidebar'>
    <div className="content" id='content'>
      {children}
    </div>
    <div className="handle">
      {getHandle()}
    </div>
  </div>);
};

Sidebar.propTypes = {
  children: PropTypes.any,
  onHide: PropTypes.func,
  onShow: PropTypes.func,
  visible: PropTypes.bool
};