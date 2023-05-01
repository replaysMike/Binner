import React from "react";
import { Form, Input, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import "./ClearableInput.css";

/**
 * A form text input that contains a clear button icon
 */
export default function ClearableInput(props) {

	const handleClear = (e) => {
		return props.onChange(e, { ...props, value: '' });
	};

	const getClearIconPosition = () => {
		if (props.action) {
			return '90px'; // this is wrong but works for now, it would have to be calculated somehow
		}
		if (props.icon) {
			return '35px';
		}
		return '10px';
	};

	// propsToExclude: exclude any props that only belong to our control
	const{ type, ...propsToReturn } = props;
	
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
	if (props.children) {
		// custom children are provided, modify the text input
		const children = props.children.map((child, key) => {
			if (React.isValidElement(child)) {
				const childProps = {...child.props };
				if (child.type === "input" && (childProps.type === undefined || childProps.type === "" || childProps.type === "text")) {
					// it's a text input, attach our ref and classnames
					return React.cloneElement(child, { ...childProps, key });
				} else {
					// other child type
					return React.cloneElement(child, { ...childProps, key });
				}
			}
		});
		if (props.type === "Form.Input") {
			return (<Form.Input {...propsToReturn}>
				{children}
				{propsToReturn.iconPosition === "left"
					? <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{left: 'unset', right: '0.75em', opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
					: <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: getClearIconPosition(), opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
				}
			</Form.Input>);
		}
		return (<Input {...propsToReturn}>
			{children}
			{propsToReturn.iconPosition === "left"
				? <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{left: 'unset', right: '0.75em', opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
				: <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: getClearIconPosition(), opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
			}
		</Input>);		
	}
	
	// no children, render directly
	if (props.type === "Form.Input") {
			return (<Form.Input {...propsToReturn}>
				<input />
				{propsForChild.icon && <Icon name={propsForChild.icon} />}
				{propsToReturn.iconPosition === "left"
					? <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{left: 'unset', right: '0.75em', opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
					: <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: getClearIconPosition(), opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
				}
				</Form.Input>);
	}
	return (<Input {...propsToReturn}>
		<input />
		{propsForChild.icon && <Icon name={propsForChild.icon} />}
		{propsToReturn.iconPosition === "left"
			? <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{left: 'unset', right: '0.75em', opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
			: <Icon name="times" circular link size="small" className="clearIcon" onClick={handleClear} style={{right: getClearIconPosition(), opacity: props.value?.length > 0 ? '0.5' : '0', visibility: props.value?.length > 0 ? 'visible' : 'hidden'}} />
		}
		</Input>);
};

ClearableInput.propTypes = {
	/** The type of element to render */
	type: PropTypes.oneOf(['Form.Input', 'Input'])
	/** !!! Note: All new props added must be excluded above. See propsToExclude */
};

ClearableInput.defaultProps = {
	type: "Form.Input",  
};