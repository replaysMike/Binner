import { memo, useMemo, useEffect, useState } from 'react';
import PropTypes from "prop-types";

export const Box = memo(function Box({ name, children, style, preview, selected, className }) {
	const [isSelected, setIsSelected] = useState(selected);
	useEffect(() => {
		setIsSelected(selected);
	}, [selected]);

	const boxStyle = useMemo(
    () => ({
			padding: '0.1rem 0.5rem',
			cursor: 'move',
    }),
    [isSelected],
  );

	const styleIsEmpty = Object.keys(style).length === 0;

	return (
    <div
			name={name}
      style={{ ...boxStyle, ...style, ...(!styleIsEmpty && { width: '100%', height: '100%' }) }}
      role={preview ? 'BoxPreview' : 'Box'}
			className={`${className} ${isSelected ? "selected" : ""}`}
    >{children}</div>
  );
});

Box.propTypes = {
	name: PropTypes.string,
  onClick: PropTypes.func,
	selected: PropTypes.bool,
	preview: PropTypes.bool,
	style: PropTypes.object,
	className: PropTypes.string
};
