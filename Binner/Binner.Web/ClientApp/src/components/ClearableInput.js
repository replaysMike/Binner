import React from "react";
import { Form, Input, Icon, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import "./ClearableInput.css";

/**
 * A form text input that contains a clear button icon
 */
export default function ClearableInput({ type = "Form.Input", ...rest }) {
  const handleClear = (e) => {
    if (rest.onClear) rest.onClear(e);
    if (!e.defaultPrevented && rest.onChange) {
      rest.onChange(e, { ...rest, value: '' });
    }
  };

  const getClearIconPosition = () => {
    if (rest.action) {
      return '90px'; // this is wrong but works for now, it would have to be calculated somehow
    }
    if (rest.icon) {
      return '35px';
    }
    return '10px';
  };

  // propsToExclude: exclude any props that only belong to our control
  const {
    onClear,
    help,
    helpWide,
    helpWideVery,
    helpPosition,
    helpPositionFixed,
    helpHoverable,
    helpDisabled,
    helpHideOnScroll,
    helpOnOpen,
    label, // remove the label prop and implement manually to get rid of semantic warning
    ...propsToReturn } = rest;
  const propsForChild = {};
  if (propsToReturn.icon && typeof propsToReturn.icon === "string") {
    // handle icon. When semantic Input has icon="name", it throws an error if it has children defined (our custom child input).
    // we need to reimplement what it does internally
    propsForChild.icon = propsToReturn.icon;
    propsToReturn.icon = true;
    // propsToReturn.iconPosition = "left";
  }
  if (!propsToReturn.icon)
    propsToReturn.icon = true;

  const popupOptions = {
    position: rest.helpPosition,
    positionFixed: rest.helpPositionFixed,
    wide: rest.helpWideVery ? 'very' : rest.helpWide,
    hoverable: rest.helpHoverable,
    disabled: rest.helpDisabled,
    hideOnScroll: rest.helpHideOnScroll,
    onOpen: rest.helpOnOpen,
  };
  const icons = propsToReturn.iconPosition === "left"
    ? <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{ left: 'unset', right: '0.75em', opacity: rest.value?.length > 0 ? '0.5' : '0', visibility: rest.value?.length > 0 ? 'visible' : 'hidden' }} />
    : <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{ right: getClearIconPosition(), opacity: rest.value?.length > 0 ? '0.5' : '0', visibility: rest.value?.length > 0 ? 'visible' : 'hidden' }} />;

  let inputElement = ((<Input {...propsToReturn}>
    {label && <div><label htmlFor={propsToReturn.name}>{label}</label></div>}
    <input />
    {propsForChild.icon && <Icon name={propsForChild.icon} />}
    {icons}
  </Input>));

  // if the control has children, attach our classes
  if (rest.children) {
    // custom children are provided, modify the text input
    const children = rest.children.map((child, key) => {
      if (React.isValidElement(child)) {
        const childProps = { ...child.props };
        if (child.type === "input" && (childProps.type === undefined || childProps.type === "" || childProps.type === "text")) {
          // it's a text input, attach our classnames
          return React.cloneElement(child, { ...childProps, key });
        } else {
          // other child type
          return React.cloneElement(child, { ...childProps, key });
        }
      }
    });
    if (type === "Form.Input") {
      // use Form.Input component
      inputElement = (<Form.Input {...propsToReturn}>
        {label && <div><label htmlFor={propsToReturn.name}>{label}</label></div>}
        {children}
        {icons}
      </Form.Input>);
      if (help) inputElement = (<Popup {...popupOptions} content={<>{help}</>} trigger={inputElement} />);
      return inputElement;
    }
    // use Input component
    inputElement = (<Input {...propsToReturn}>
      {label && <div><label htmlFor={propsToReturn.name}>{label}</label></div>}
      {children}
      {icons}
    </Input>);
    if (help) inputElement = (<Popup {...popupOptions} content={<>{help}</>} trigger={inputElement} />);
    return inputElement;
  }

  // no children, render directly
  if (type === "Form.Input") {
    inputElement = (<Form.Input {...propsToReturn}>
      {label && <div><label htmlFor={propsToReturn.name}>{label}</label></div>}
      <input />
      {propsForChild.icon && <Icon name={propsForChild.icon} />}
      {icons}
    </Form.Input>);
    if (help) inputElement = <Popup {...popupOptions} content={<>{help}</>} trigger={inputElement} />
    return inputElement;
  }

  return inputElement;
};

ClearableInput.propTypes = {
  /** The type of element to render */
  type: PropTypes.oneOf(['Form.Input', 'Input']),
  /** !!! Note: All new props added must be excluded above. See propsToExclude */
  /** Event triggered when the clear button is clicked */
  onClear: PropTypes.func,
  help: PropTypes.string,
  helpWide: PropTypes.bool,
  helpWideVery: PropTypes.bool,
  helpPosition: PropTypes.oneOf(['top center', 'top left', 'top right', 'bottom center', 'bottom left', 'bottom right', 'right center', 'left center']),
  helpPositionFixed: PropTypes.bool,
  helpHoverable: PropTypes.bool,
  helpDisabled: PropTypes.bool,
  helpHideOnScroll: PropTypes.bool,
  helpOnOpen: PropTypes.func
};
