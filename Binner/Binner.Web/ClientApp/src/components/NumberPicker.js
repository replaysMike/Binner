import React, { Component } from "react";
import _ from "lodash";
import { Button, Input } from "semantic-ui-react";
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
export default class NumberPicker extends Component {
  constructor(props) {
    super(props);
    this.state = {
      touched: false,
      buffer: {}
    };
    this.style = {
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
    this.handleAction = this.handleAction.bind(this);
    this.handleWheel = this.handleWheel.bind(this);
    this.validateInput = this.validateInput.bind(this);
    this.handleSetValue = this.handleSetValue.bind(this);
    this.inputRef = React.createRef();
    this.preventDefault = e => e.preventDefault();
  }

  componentDidMount() {
    this.inputRef.current.addEventListener('wheel', this.preventDefault, { passive: true });
  }

  componentWillUnmount () {
    this.inputRef.current.removeEventListener('wheel', this.preventDefault, { passive: true });
}

  handleWheel(event, v) {
    event.stopPropagation();
    const direction = event.deltaY !== 0 ? (event.deltaY > 0 ? 1 : -1) : 0;
    let currentValue = event.currentTarget.value.replace(",", ".");
    let stepSize = this.props.step;
    if (event.ctrlKey && event.altKey) stepSize = this.props.step * 1000;
    else if (event.ctrlKey) stepSize = this.props.step * 10;
    else if (event.altKey) stepSize = this.props.step * 100;
    // inverted mouse wheel direction
    switch (direction) {
      case 1:
        this.handleSetValue(DECREASE_VALUE, currentValue, stepSize);
        break;
      case -1:
        this.handleSetValue(INCREASE_VALUE, currentValue, stepSize);
        break;
      default:
        break;
    }
    return false;
  }

  handleAction(event, v) {
    let actionFilter = event.currentTarget.name;
    let currentValue = event.currentTarget.value.replace(",", ".");
    // test and only allow numeric values when manually input
    if (currentValue) {
      var newValue = parseInt(currentValue);
      if (isNaN(newValue)) return;
    }
    this.handleSetValue(actionFilter, currentValue, this.props.step);
  }

  handleSetValue(actionFilter, currentValue, requestedStepSize) {
    var setVal = _.isFinite(parseFloat(this.props.value)) ? parseFloat(this.props.value) : null;
    let stepSize = _.isFinite(parseFloat(requestedStepSize)) ? parseFloat(requestedStepSize) : 1;
    switch (actionFilter) {
      case DECREASE_VALUE:
        if (this.props.value - stepSize >= this.props.min) setVal -= stepSize;
        else setVal = this.props.min;

        break;
      case INCREASE_VALUE:
        if (setVal + stepSize <= this.props.max) setVal += stepSize;
        else setVal = this.props.max;

        break;
      case this.props.name:
        let parsedVal = parseFloat(currentValue);
        if (currentValue === "-") this.setState({ buffer: "-" });

        if (parsedVal > this.props.max || parsedVal < this.props.min) console.error("Invalid number specified");
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

    setTimeout(this.props.onChange, 1, { name: this.props.name, value: returnValue });
  }

  validateInput(event, v) {
    let actionFilter = event.target.name;
    let currentValue = event.target.value;

    var setVal = this.props.value;
    switch (actionFilter) {
      case this.props.name:
        let parsedVal = parseFloat(currentValue);
        setVal = _.isFinite(parsedVal) ? parsedVal : null;

        if (parsedVal > this.props.max) setVal = this.props.max;
        break;

      case DECREASE_VALUE:
      case INCREASE_VALUE:
      default:
        break;
    }
  }

  render() {
    var { name, step, maxLength, placeholder, required, autoComplete, circular, basic, compact, min, max, value, ...rest } = this.props;
    var style = circular ? this.style.circular : this.style.default;
    var display = { circular, basic, compact };
    return (
      <Input {...rest}>
        <Button {...display} type="button" icon="minus" onClick={this.handleAction} name={DECREASE_VALUE} style={style.buttonLeft} disabled={value <= min} />
        <input
          type="text"
          className="noscroll"
          name={name}
          min={min}
          max={max}
          step={step}
          maxLength={maxLength}
          placeholder={placeholder}
          required={required}
          value={this.props.value}
          onChange={this.handleAction}
          onBlur={this.validateInput}
          onWheel={this.handleWheel}
          style={style.input}
          autoComplete={autoComplete}
          ref={this.inputRef}
        />
        <Button {...display} type="button" icon="plus" onClick={this.handleAction} name={INCREASE_VALUE} style={style.buttonRight} disabled={value >= max} />
      </Input>
    );
  }

  static defaultProps = {
    placeholder: "0",
    /*
      Limiting min, max value to 1e10 to prevent javascript to switch into scientific notation
      */
    min: 1e10 * -1,
    max: 1e10,
    maxLength: 10,
    step: 1,
    required: false,
    basic: false,
    circular: false,
    compact: false,
    autoComplete: "off"
  };

  static propTypes = {
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
    autoComplete: PropTypes.string
  };
}
