import React, { useEffect, useRef, useMemo } from "react";
import { Form, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import { v4 as uuidv4 } from 'uuid';
import { Events } from "../common/events";
import "./ProtectedInput.css";

/**
 * A form input that is protected against barcode input
 */
export default function ProtectedInput(props) {
	const IsDebug = true;
	const ScanSuccessClassRemovalMs = 2100;
	const DefaultProtectedClassName = "protectedInput";
	const DefaultIsScanningClassName = "isBarcodeScanning";
	const DefaultScanningCompleteClassName = "barcodeScanSuccess";
	const bufferedValue = useRef(null);
	const inputRef = useRef(null);
	const renderCount = useRef(0);
	const id = useMemo(() => props.id || uuidv4(), [props.id]);

	const barcodeReadStarted = (e) => {
		if (e.detail.destination.id !== id) return;
		if (IsDebug) console.log('barcodeReadStarted', e, e.detail.destination.id, id);
		inputRef.current.classList.add(DefaultIsScanningClassName);
	};

	const barcodeReadCancelled = (e) => {
		if (e.detail.destination.id !== id) return;
		if (IsDebug) console.log('barcodeReadCancelled', e, e.detail.destination.id, id);
		inputRef.current.classList.remove(DefaultIsScanningClassName);
	};

	const barcodeReadReceived = (e) => {
		if (e.detail.destination.id !== id) return;
		if (IsDebug) console.log('barcodeReadReceived', e, e.detail.destination.id, id, props.clearOnScan);
		// replace the text input control to the original before the barcode scan took place
		bufferedValue.current = bufferedValue.current?.replaceAll(e.detail.text, "");
		inputRef.current.classList.remove(DefaultIsScanningClassName);
		inputRef.current.classList.add(DefaultScanningCompleteClassName);
		// remove class in a bit to allow animations to finish
		setTimeout(() => { inputRef.current.classList.remove(DefaultScanningCompleteClassName); }, ScanSuccessClassRemovalMs);

		if (props.clearOnScan) {
			// clear/restore the text input
			props.onChange(e, {...props, value: bufferedValue.current });
		}
	};

	useEffect(() => {
		document.body.addEventListener(Events.BarcodeReading, barcodeReadStarted);
		document.body.addEventListener(Events.BarcodeReadingCancelled, barcodeReadCancelled);
		document.body.addEventListener(Events.BarcodeReceived, barcodeReadReceived);
		return () => {
			document.body.removeEventListener(Events.BarcodeReading, barcodeReadStarted);
			document.body.removeEventListener(Events.BarcodeReadingCancelled, barcodeReadCancelled);
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
	const{ allowEnter, hideIcon, clearOnScan, ...propsToReturn } = privateProps;

	// if the control has children, attach our refs and classes
	if (props.children) {
		// custom children are provided, modify the text input
		return <Form.Input { ...propsToReturn } id={id}>
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

	//renderCount.current = renderCount.current + 1;
	//if (IsDebug) console.log('render', renderCount.current);

	return <Form.Input { ...propsToReturn } id={id}>
						{propsForChild.icon && <Icon name={propsForChild.icon} />}
						{!hideIcon && <Icon name="barcode" style={{right: propsToReturn.iconPosition !== "left" && propsForChild.icon ? '25px' : '0', left: 'unset'}} />}
						<input ref={inputRef} className={DefaultProtectedClassName} />
					</Form.Input>;
};

ProtectedInput.propTypes = {
	/** True to clear the input after a scan has completed */
	clearOnScan: PropTypes.bool,
  /** True to block enter key */
  allowEnter: PropTypes.bool,
	/** True to hide barcode icon */
  hideIcon: PropTypes.bool,
};

ProtectedInput.defaultProps = {
	clearOnScan: true,
  allowEnter: false,
	hideIcon: false,
};