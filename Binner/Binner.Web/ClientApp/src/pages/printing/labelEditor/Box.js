import { memo, useMemo, useEffect, useState } from 'react';
import PropTypes from "prop-types";

export const Box = memo(function Box({ name, children, style, preview, selected, className }) {
	const [isSelected, setIsSelected] = useState(selected);
	useEffect(() => {
		setIsSelected(selected);
	}, [selected]);

	const boxStyle = useMemo(
    () => ({
			border: isSelected ? '2px dashed black' : '1px dashed gray',
			padding: '0.1rem 0.5rem',
			cursor: 'move',
    }),
    [isSelected],
  );

	return (
    <div
			name={name}
      style={{ ...boxStyle, ...style }}
      role={preview ? 'BoxPreview' : 'Box'}
			className={className}
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
