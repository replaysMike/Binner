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
export function Clipboard({ text = '', color = 'grey', size, ...rest}) {
	const handleCopy = (e, copyText) => {
		e.preventDefault();
		e.stopPropagation();
		navigator.clipboard.writeText(copyText);
		toast.info('Copied!');
	};

	return (<Popup 
		content="Copy" 
		offset={[-15, -5]} 
		trigger={<Icon 
			name="copy outline" 
			text={text} 
			color={color} 
			size={size} 
			{...rest} 
			className="clipboard" 
			onClick={e => handleCopy(e, text)} 
		/>}
	/>);
};

Clipboard.propTypes = {
  /** Text to copy */
  text: PropTypes.string,
	/** Color of icon */
	color: PropTypes.string,
	/** Size of icon */
	size: PropTypes.string
};
