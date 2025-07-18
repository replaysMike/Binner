import { memo, useMemo } from 'react';
import { useDrag } from 'react-dnd';
import { Box } from './Box.js';
import { ItemTypes } from './ItemTypes.js';
import PropTypes from "prop-types";

const style = {
	display: 'flex'
};

export const DraggableBox = memo(function DraggableBox({ resize = 'horizontal', acceptsValue = false, displaysValue = false, ...rest }) {
  const { id, left, top, children, name } = rest;

  const [{ isDragging }, drag] = useDrag(
    () => ({
      type: ItemTypes.Box,
      item: { id, left, top, children, name, resize, acceptsValue, displaysValue },
      collect: (monitor) => ({
        isDragging: monitor.isDragging(),
      }),
    }),
    [id, left, top, children, resize, acceptsValue, displaysValue],
  );

	const containerStyle = useMemo(
    () => ({
      ...style,
			left: `${left || 0}px`,
			top: `${top || 0}px`,
      opacity: isDragging ? 0.5 : 1,
    }),
    [isDragging, left, top],
  );

	const getResizeClass = (resize) => {
		switch(resize) {
			case 'both':
				return "resizeBoth";
			case 'vertical':
				return 'resizeVertical';
			default:
			case 'horizontal':
				return 'resizeHorizontal';
		}
	};

	const handleClick = (e) => {
		e.preventDefault();
		e.stopPropagation();
		if (rest.onClick) rest.onClick(e);
	};

	const styleToApply = {...containerStyle, ...(rest.absolute && { position: 'absolute' }), width: rest.style?.width, height: rest.style?.height };
	const boxStyleToApply = {...rest.style};
  return (
    <div ref={drag} id={id} name={rest.name} style={styleToApply} role="DraggableBox" onClick={handleClick} className={`draggableBox ${getResizeClass(rest.resize)}`} onKeyDown={rest.onKeyDown} tabIndex={-1}>
      <Box style={boxStyleToApply} name={rest.name} selected={rest.selected} className={`box ${rest.className}`}>{rest.children}</Box>
    </div>
  );
});

DraggableBox.propTypes = {
	name: PropTypes.string,
	id: PropTypes.string,
  onClick: PropTypes.func,
	/** True if currently selected */
	selected: PropTypes.bool,
	/** True to absolutely position the element */
	absolute: PropTypes.bool,
	/** True to allow this box to accept the value property (for custom text entry) */
	acceptsValue: PropTypes.bool,
	/** True to show the value */
	displaysValue: PropTypes.bool,
	style: PropTypes.object,
	onKeyDown: PropTypes.func,
	/** The resize type to allow */
	resize: PropTypes.string
};
