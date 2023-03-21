import React from "react";
import { Icon, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import { toast } from "react-toastify";
import "./Clipboard.css";

/**
 * Clipboard copy/paste control
 * @param {props} props 
 * @returns 
 */
export function Clipboard(props) {
	const handleCopy = (e, copyText) => {
		navigator.clipboard.writeText(copyText);
		toast.info('Copied!');
	};

	return (<Popup content="Copy" offset={[-15, -5]} trigger={<Icon name="copy outline" {...props} className="clipboard" onClick={e => handleCopy(e, props.text)} />}/>);
};

Clipboard.propTypes = {
  /** Text to copy */
  text: PropTypes.string,
	/** Color of icon */
	color: PropTypes.string,
	/** Size of icon */
	size: PropTypes.string
};

Clipboard.defaultProps = {
  text: '',
	color: 'grey',
};