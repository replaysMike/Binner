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
export function Clipboard({ text = '', color = 'grey', size, className, showEmpty = false, ...rest}) {
	const handleCopy = (e, copyText) => {
		e.preventDefault();
		e.stopPropagation();
		navigator.clipboard.writeText(copyText);
		toast.info('Copied!');
	};

  return ((showEmpty || text) && <Popup 
		content="Copy" 
		offset={[-15, -5]} 
		trigger={<Icon 
			name="copy outline" 
			text={text} 
			color={color} 
			size={size} 
			{...rest} 
			className={`clipboard ${className}`}
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
	size: PropTypes.string,
  /** True to show the control even when it has no value */
  showEmpty: PropTypes.bool,
  /** additional class names */
  className: PropTypes.string
};
