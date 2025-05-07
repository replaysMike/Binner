import React, { useState, useEffect, useRef } from "react";
import _ from "lodash";
import { Button, Input, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";

export const DECREASE_VALUE = "DECREASE_VALUE";
export const INCREASE_VALUE = "INCREASE_VALUE";

/*
 USAGE EXAMPLES:
 <Form.Field inline control={NumberPicker} name={MULTIPLY_INPUT + ".times"} onChange={this.triggerChange} label="Copies to create" defaultValue={1} min={1} max={999} placeholder="Repeat ..." />
 <Form.Field width="8" control={NumberPicker} compact label="compact buttons" placeholder="Enter a number" defaultValue={6} min={-41} max={45} step={1} />
 <Form.Field width="8" control={NumberPicker} circular label="circular buttons" placeholder="Enter a number" defaultValue={6} min={-41} max={45} step={1} />
 <Form.Field width="8" control={NumberPicker} basic label="basic buttons" placeholder="Enter a number" defaultValue={4} min={-40} max={40} step={2} />

 */
export default function NumberPicker({ 
  placeholder = '0', min = 1e10 * -1, max = 1e10, maxLength = 10, step = 1, required = false, basic = false, circular = false, compact = false, autoComplete = 'off', 
  help, helpWide, helpWideVery, helpPosition, helpPositionFixed, helpHoverable, helpDisabled, helpHideOnScroll, helpOnOpen,
  ...rest }) {
  const [touched, setTouched] = useState(false);
  const [buffer, setBuffer] = useState({});
  const defaultStyles = {
    default: {
      input: {
        borderRadius: "0px",
        textAlign: "right"
      },
      buttonLeft: {
        borderTopRightRadius: "0px",
        borderBottomRightRadius: "0px",
        margin: "0px"
      },
      buttonRight: {
        borderTopLeftRadius: "0px",
        borderBottomLeftRadius: "0px"
      }
    },
    circular: {
      input: {
        textAlign: "right"
      },
      buttonLeft: {
        marginRight: "3.5px"
      },
      buttonRight: {
        marginLeft: "3.5px"
      }
    }
  };
  const inputRef = useRef(null);
  const preventDefault = (e) => e.preventDefault();
  
  useEffect(() => {
    inputRef.current.addEventListener('wheel', preventDefault, { passive: false });
    return () => {
      if (inputRef?.current) 
        inputRef.current.removeEventListener('wheel', preventDefault, { passive: false });
    };
  }, []);

  const handleWheel = (event, v) => {
    event.stopPropagation();
    const direction = event.deltaY !== 0 ? (event.deltaY > 0 ? 1 : -1) : 0;
    let currentValue = event.currentTarget.value.replace(",", ".");
    let stepSize = step;
    if (event.ctrlKey && event.altKey) stepSize = step * 1000;
    else if (event.ctrlKey) stepSize = step * 10;
    else if (event.altKey) stepSize = step * 100;
    // inverted mouse wheel direction
    switch (direction) {
      case 1:
        handleSetValue(DECREASE_VALUE, currentValue, stepSize);
        break;
      case -1:
        handleSetValue(INCREASE_VALUE, currentValue, stepSize);
        break;
      default:
        break;
    }
    return false;
  };

  const handleAction = (event, v) => {
    let actionFilter = event.currentTarget.name;
    let currentValue = event.currentTarget.value.replace(",", ".");
    // test and only allow numeric values when manually input
    if (currentValue) {
      var newValue = parseInt(currentValue);
      if (isNaN(newValue)) return;
    }
    handleSetValue(actionFilter, currentValue, step);
  };

  const handleSetValue = (actionFilter, currentValue, requestedStepSize) => {
    var setVal = _.isFinite(parseFloat(rest.value)) ? parseFloat(rest.value) : null;
    let stepSize = _.isFinite(parseFloat(requestedStepSize)) ? parseFloat(requestedStepSize) : 1;
    switch (actionFilter) {
      case DECREASE_VALUE:
        if (rest.value - stepSize >= min) setVal -= stepSize;
        else setVal = min;

        break;
      case INCREASE_VALUE:
        if (setVal + stepSize <= max) setVal += stepSize;
        else setVal = max;

        break;
      case rest.name:
        let parsedVal = parseFloat(currentValue);
        if (currentValue === '-') setBuffer('-');

        if (parsedVal > max || parsedVal < min) console.error("Invalid number specified");
        else setVal = currentValue;
        break;
      default:
        break;
    }

    let lastChar = ("" + setVal).charAt(setVal.length - 1) || "";
    let returnValue = setVal;
    let precision = 1000;
    if (_.isFinite(parseFloat(setVal))) returnValue = Math.floor(parseFloat(setVal) * precision) / precision;

    if (setVal === "" || setVal === "-" || lastChar === "." || lastChar === ",") returnValue = setVal;

    setTimeout(rest.onChange, 1, { name: rest.name, value: returnValue });
  };

  const validateInput = (event, v) => {
    let actionFilter = event.target.name;
    let currentValue = event.target.value;

    var setVal = rest.value;
    switch (actionFilter) {
      case rest.name:
        let parsedVal = parseFloat(currentValue);
        setVal = _.isFinite(parsedVal) ? parsedVal : null;

        if (parsedVal > max) setVal = max;
        break;

      case DECREASE_VALUE:
      case INCREASE_VALUE:
      default:
        break;
    }
  };

  var _style = circular ? defaultStyles.circular : defaultStyles.default;
  var display = { circular, basic, compact };
  const popupOptions = {
    position: helpPosition,
    positionFixed: helpPositionFixed,
    wide: helpWideVery ? 'very' : helpWide,
    hoverable: helpHoverable,
    disabled: helpDisabled,
    hideOnScroll: helpHideOnScroll,
    onOpen: helpOnOpen,
  };
  let inputElement = (<Input {...rest}>
    <Button {...display} type="button" icon="minus" onClick={handleAction} name={DECREASE_VALUE} style={_style.buttonLeft} disabled={rest.value <= min} />
    <input
      type="text"
      className="noscroll"
      name={rest.name}
      min={min}
      max={max}
      step={step}
      maxLength={maxLength}
      placeholder={placeholder}
      required={required}
      value={rest.value}
      onChange={handleAction}
      onBlur={validateInput}
      onWheel={handleWheel}
      style={_style.input}
      autoComplete={autoComplete}
      ref={inputRef}
    />
    <Button {...display} type="button" icon="plus" onClick={handleAction} name={INCREASE_VALUE} style={_style.buttonRight} disabled={rest.value >= max} />
  </Input>);
  if (help) {
    inputElement = (<Popup {...popupOptions} content={<>{help}</>} trigger={inputElement} />);
  }

  return inputElement;
}
  
NumberPicker.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.any.isRequired,
  onChange: PropTypes.func.isRequired,
  placeholder: PropTypes.string,
  min: PropTypes.number,
  max: PropTypes.number,
  step: PropTypes.number,
  maxLength: PropTypes.number,
  required: PropTypes.bool,
  basic: PropTypes.bool,
  circular: PropTypes.bool,
  compact: PropTypes.bool,
  autoComplete: PropTypes.string,
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