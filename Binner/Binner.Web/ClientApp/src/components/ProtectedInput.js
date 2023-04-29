import React, { useEffect, useRef } from "react";
import { Form } from "semantic-ui-react";
import PropTypes from "prop-types";
import { Events } from "../common/events";
import "./ProtectedInput.css";

/**
 * A form input
 */
export default function ProtectedInput(props) {
	const ScanSuccessClassRemovalMs = 2100;
	const DefaultProtectedClassName = "protectedInput";
	const DefaultIsScanningClassName = "isBarcodeScanning";
	const DefaultScanningCompleteClassName = "barcodeScanSuccess";
	const bufferedValue = useRef(null);
	const inputRef = useRef(null);

	const barcodeReadStarted = (e) => {
		inputRef.current.classList.add(DefaultIsScanningClassName);
	};

	const barcodeReadReceived = (e) => {
		bufferedValue.current = bufferedValue.current.replaceAll(e.detail.text, "");
		inputRef.current.classList.remove(DefaultIsScanningClassName);
		inputRef.current.classList.add(DefaultScanningCompleteClassName);
		// remove class in a bit to allow animations to finish
		setTimeout(() => { inputRef.current.classList.remove(DefaultScanningCompleteClassName); }, ScanSuccessClassRemovalMs);

		// clear/restore the text input
		props.onChange(e, {...props, value: bufferedValue.current });
	};

	useEffect(() => {
		document.body.addEventListener(Events.BarcodeReading, barcodeReadStarted);
		document.body.addEventListener(Events.BarcodeReceived, barcodeReadReceived);
		return () => {
			document.body.removeEventListener(Events.BarcodeReading, barcodeReadStarted);
			document.body.removeEventListener(Events.BarcodeReceived, barcodeReadReceived);
		};
	}, []);

	const internalOnChange = (e, control) => {
		// store current value
		bufferedValue.current = control.value;
		// pass input to child control
		return props.onChange(e, control);
	};

	// intercept the onChange & keydown events
	const privateProps = {...props, 
		onChange: (e, control) => internalOnChange(e, control), 
		onKeyDown: (e) => {	
			// block enter key completely if configured to do so
			const isCr = e.keyCode === 13;
			if (isCr && !props.allowEnter) {
				// prevent character
				e.preventDefault();
			}
		}
	};

	// exclude any props that only belong to our control
	const{ allowEnter, ...propsToReturn } = privateProps;

	// if the control has children, attach our refs and classes
	if (props.children) {
		// custom children are provided, modify the text input
		return <Form.Input { ...propsToReturn }>
			{props.children.map((child, key) => {
				if (React.isValidElement(child)) {
					const childProps = {...child.props };
					if (child.type === "input" && (childProps.type === undefined || childProps.type === "" || childProps.type === "text")) {
						// it's a text input, attach our ref and classnames
						const classNames = childProps.className?.split(' ') || [];
						classNames.push(DefaultProtectedClassName);
						return React.cloneElement(child, { ...childProps, key, ref: inputRef, className: classNames.join(' ') });
 					} else {
						// other child type
						return React.cloneElement(child, { ...childProps, key });
					}
				}
			})}
			</Form.Input>;	
	}

	// no children, render directly
	return <Form.Input { ...propsToReturn }><input ref={inputRef} className={DefaultProtectedClassName} /></Form.Input>;
};

ProtectedInput.propTypes = {
  /** True to block enter key */
  allowEnter: PropTypes.bool,
};

ProtectedInput.defaultProps = {
  allowEnter: false,
};