import React, { useEffect, useRef, useMemo } from "react";
import { Form, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import { v4 as uuidv4 } from 'uuid';
import { Events } from "../common/events";
import "./ProtectedInput.css";

/**
 * A form text input that is protected against barcode input.
 * When barcode input is received, the control is masked out and content is replaced and cleared after a successful barcode scan.
 */
export default function ProtectedInput({ clearOnScan = true, allowEnter = false, hideIcon = false, hideClearIcon = false, onClear, onBarcodeReadStarted, onBarcodeReadCancelled, onBarcodeReadReceived, ...rest }) {
	const IsDebug = false;
	const ScanSuccessClassRemovalMs = 2100;
	const DefaultProtectedClassName = "protectedInput";
	const DefaultIsScanningClassName = "isBarcodeScanning";
	const DefaultScanningCompleteClassName = "barcodeScanSuccess";
	const bufferedValue = useRef(null);
	const inputRef = useRef(null);
	const inputReceiving = useRef(false);
	const id = useMemo(() => rest.id || uuidv4(), [rest.id]);

	const barcodeReadStarted = (e) => {
    rest.onChange(e, { value: '' });
    inputReceiving.current = true;
		//window.requestAnimationFrame(() => { inputReceiving.current = true; });
		if (IsDebug) console.debug(`PI: sending read started id: ${id} dest: ${e.detail.destination.id}`);
		if (e.detail.destination.id !== id) return;
		if (IsDebug) console.debug('PI: barcodeReadStarted', e, e.detail.destination.id, id);
		inputRef.current.classList.add(DefaultIsScanningClassName);
		if (onBarcodeReadStarted) onBarcodeReadStarted(e);
	};

	const barcodeReadCancelled = (e) => {
    inputReceiving.current = false;
    //window.requestAnimationFrame(() => { inputReceiving.current = false; });
		if (IsDebug) console.debug(`PI: sending read cancelled id: ${id} dest: ${e.detail.destination.id}`);
		if (e.detail.destination.id !== id) return;
		if (IsDebug) console.debug('PI: barcodeReadCancelled', e, e.detail.destination.id, id);
		inputRef.current.classList.remove(DefaultIsScanningClassName);
		if (onBarcodeReadCancelled) onBarcodeReadCancelled(e);
	};

	const barcodeReadReceived = (e) => {
    inputReceiving.current = false;
    //window.requestAnimationFrame(() => { inputReceiving.current = false; });
		if (IsDebug) console.debug(`PI: sending read complete id: ${id} dest: ${e.detail.destination?.id}`);
		if (e.detail.destination && e.detail.destination.id !== id) return;
		if (IsDebug) console.debug('PI: barcodeReadReceived', e, e.detail.destination?.id, id, clearOnScan);
		// replace the text input control to the original before the barcode scan took place
		bufferedValue.current = bufferedValue.current?.replaceAll(e.detail.text, "");
		inputRef.current.classList.remove(DefaultIsScanningClassName);
		inputRef.current.classList.add(DefaultScanningCompleteClassName);
		// remove class in a bit to allow animations to finish
		setTimeout(() => { inputRef.current.classList.remove(DefaultScanningCompleteClassName); }, ScanSuccessClassRemovalMs);
		if (clearOnScan) {
			// clear/restore the text input
			rest.onChange(e, {...rest, clearOnScan, allowEnter, hideIcon, hideClearIcon, value: bufferedValue.current });
		}
		if (onBarcodeReadReceived) onBarcodeReadReceived(e);
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
		// pass input to child control only when not receiving barcode data
		if (!inputReceiving.current)
			return rest.onChange(e, control);
    else if (inputRef.current?.value?.length > 0) {
      // reset the input control value while receiving barcode data
      return rest.onChange(e, { value: ''});
    }
	};

	// intercept the onChange & keydown events
	const privateProps = {...rest, 
		onChange: (e, control) => internalOnChange(e, control), 
		onKeyDown: (e) => {	
      if (inputReceiving.current) {
        // dropping keydown, we are receiving a barcode
        e.preventDefault();
        return;
      }
      //console.log('kd', e.keyCode, inputReceiving.current);
      // block enter key completely if configured to do so
      const isCr = e.keyCode === 13;
			if (isCr && !allowEnter) {
				// prevent character
				e.preventDefault();
			}
		}
	};

	const handleClear = (e) => {
    if (onClear) onClear(e);
		return rest.onChange(e, { ...rest, clearOnScan, allowEnter, hideIcon, hideClearIcon, value: '' });
	};

	// propsToExclude: exclude any props that only belong to our control
	const{ ...propsToReturn } = privateProps;
	
	const getClearIconPosition = () => {
		if (rest.icon) {
			if (hideIcon)
				return '35px';
			else
				return '60px';
		} else if (!hideIcon) {
			return '35px';
		}
		return '10px';
	};

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

	// if the control has children, attach our refs and classes
	if (rest.children) {
		// custom children are provided, modify the text input
		const children = rest.children.map((child, key) => {
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
		});

		return <Form.Input { ...propsToReturn } id={id}>
			{children}
			{!hideClearIcon && <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: propsToReturn.iconPosition !== "left" && getClearIconPosition(), left: 'unset', opacity: rest.value?.length > 0 ? '0.5' : '0', visibility: rest.value?.length > 0 ? 'visible' : 'hidden'}} />}
			{!hideIcon && <Icon name="barcode" style={{right: propsToReturn.iconPosition !== "left" && propsForChild.icon ? '25px' : '0', left: 'unset'}} />}
			</Form.Input>;
	}

	// no children, render directly
	return <Form.Input { ...propsToReturn } id={id}>
						<input ref={inputRef} className={DefaultProtectedClassName} />
						{propsForChild.icon && <Icon name={propsForChild.icon} />}
						{!hideClearIcon && <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: propsToReturn.iconPosition !== "left" && getClearIconPosition(), left: 'unset', opacity: rest.value?.length > 0 ? '0.5' : '0', visibility: rest.value?.length > 0 ? 'visible' : 'hidden'}} />}
						{!hideIcon && <Icon name="barcode" style={{right: propsToReturn.iconPosition !== "left" && propsForChild.icon ? '25px' : '0', left: 'unset'}} />}
					</Form.Input>;
};

ProtectedInput.propTypes = {
	/** True to clear the input after a scan has completed */
	clearOnScan: PropTypes.bool,
  /** True to block enter key */
  allowEnter: PropTypes.bool,
	/** True to hide barcode icon */
  hideIcon: PropTypes.bool,
	/** True to hide clear icon */
	hideClearIcon: PropTypes.bool,
  /** Event triggered when the clear button is clicked */
  onClear: PropTypes.func,
	/** Event triggered when barcode reading has started */
	onBarcodeReadStarted: PropTypes.func,
	/** Event triggered when barcode reading has been cancelled */
	onBarcodeReadCancelled: PropTypes.func,
	/** Event triggered when barcode reading has completed */
	onBarcodeReadReceived: PropTypes.func
	/** !!! Note: All new props added must be excluded above. See propsToExclude */
};
