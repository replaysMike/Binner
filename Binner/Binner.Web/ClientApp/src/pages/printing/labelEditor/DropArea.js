import { useCallback, useState, useEffect } from 'react';
import PropTypes from "prop-types";
import { useDrop } from 'react-dnd';
import { DraggableBox } from './DraggableBox';
import { ItemTypes } from './ItemTypes';
import { snapToGrid as doSnapToGrid } from './snapToGrid';
import { updateStateItem } from '../../../common/reactHelpers';
import { getChildrenByName } from './labelEditorComponents';
import _ from 'underscore';

const style = {
  border: '1px solid black',
  position: 'relative',
};

export const DropArea = ({ snapToGrid, onDrop, onMove, onRemove, width = 300, height = 300, margin,  onSelectedItemChanged, itemProperties, clearSelectedItem, boxes }) => {
	const [internalBoxes, setInternalBoxes] = useState(boxes);

	const deselectAll = useCallback(() => {
		if (internalBoxes.length > 0) {
			for(var i = 0; i < internalBoxes.length; i++) {
				internalBoxes[i].selected = false;
			}
		}
	}, [internalBoxes]);

	useEffect(() => {
		deselectAll();
		setInternalBoxes([...internalBoxes]);
		if (onSelectedItemChanged) onSelectedItemChanged(null, null);
	}, [clearSelectedItem]);

	useEffect(() => {
		setInternalBoxes(boxes);
	}, [boxes]);

	const setSelectedItem = useCallback((item) => {
		const originalSelected = item.selected;
		deselectAll();
		item.selected = true;
		if (onSelectedItemChanged && originalSelected !== item.selected) onSelectedItemChanged(null, item);
	}, [deselectAll, onSelectedItemChanged]);

  const [{ isOver, draggingColor, canDrop }, drop] = useDrop(
    () => ({
      accept: [ItemTypes.DraggableBox, ItemTypes.Box],
      drop(item, monitor) {
				// fired when a new box has been dropped on the drop area
				let newBoxes = internalBoxes;
				let newItem = item;
				let name = newItem.name;
				let resize = newItem.resize;
				let rotate = newItem.rotate;
				let acceptsValue = newItem.acceptsValue;
				let displayValue = newItem.displayValue;
				let left = newItem.left || 0;
				let top = newItem.top || 0;
				const containerEl = document.getElementById('container');
				const boundingBox = containerEl.getBoundingClientRect();
				// this seems to break dropping as it contains unexpected values
				//const boundingBox = { width: containerEl?.clientWidth || 0, height: containerEl?.clientHeight || 0, x: containerEl?.clientLeft || 0, left: containerEl?.clientLeft || 0, y: containerEl?.clientTop || 0, top: containerEl?.clientTop || 0 };

				const exists = _.find(internalBoxes, i => i.id === item.id);
				if (!exists) {
					// add new item to the drop area
					const offset = monitor.getSourceClientOffset();
					const deltaX = offset.x - boundingBox.x;
					const deltaY = offset.y - boundingBox.y;

					newBoxes = [...internalBoxes];
					const lastBox = newBoxes.length > 0 ? newBoxes[newBoxes.length - 1] : null;
					let lastId = 0;
					if (lastBox) {
						lastId = parseInt(lastBox.id.split('-')[0]);
					}
					let nextId = lastId + 1;
					newItem = {
						id: `${nextId}-${name}`,
						name: name,
						top: deltaY, 
						left: deltaX, 
						children: item.children || (<span>No label set!</span>),
						selected: false,
						resize: resize,
						rotate: rotate,
						acceptsValue: acceptsValue,
						displayValue: displayValue
					};
					top = newItem.top;
					left = newItem.left;
					newBoxes.push(newItem);
					setInternalBoxes([...newBoxes]);
					setSelectedItem(newItem);
					if (onDrop) onDrop(newItem);
				} else {
					// move an existing item on the drop area
					const el = document.getElementById(newItem.id);
					const elBounds = { width: el?.clientWidth || 0, height: el?.clientHeight || 0, x: el?.clientLeft || 0, left: el?.clientLeft || 0, y: el?.clientTop || 0, top: el?.clientTop || 0 };
					const delta = monitor.getDifferenceFromInitialOffset();
					left = Math.round(newItem.left + delta.x);
					top = Math.round(newItem.top + delta.y);
					if (snapToGrid) {
						[left, top] = doSnapToGrid(left, top);
					}
					if (left + elBounds.width + 2 > width) left = width - elBounds.width - 2;
					if (left < 0) left = 0;
					if (top + elBounds.height + 2 > height) top = height - elBounds.height - 2;
					if (top < 0) top = 0;
					newItem.left = left;
					newItem.top = top;
					setSelectedItem(newItem);
					setInternalBoxes(updateStateItem(internalBoxes, newItem, "name", name));
					if (onMove) onMove(newItem);
				}       
				
        return undefined;
      },
			collect: (monitor) => ({
        isOver: monitor.isOver(),
        canDrop: monitor.canDrop(),
        draggingColor: monitor.getItemType(),
      }),
    }),
    [internalBoxes],
  );

	const handleSelectedPart = (e, box) => {
		deselectAll();
		setSelectedItem(box);
		setInternalBoxes([...internalBoxes]);
	};

	const getItemPropertyStyle = (name) => {
		const box = _.find(boxes, i => i.name === name);
		const p = _.find(itemProperties, i => i.name === name);
		if (!p) return {};
		let color = 'black';
		switch(p.color) {
			default:
			case 0:
				color = 'black';
				break;
			case 1:
				color = 'blue';
				break;
			case 2:
				color = 'gray';
				break;
			case 3:
				color = 'green';
				break;
			case 4:
				color = 'orange';
				break;
			case 5:
				color = 'purple';
				break;
			case 6:
				color = 'red';
				break;
			case 7:
				color = 'yellow';
				break;
		}
		let align='center';
		switch(p.align){
			default:
			case 0:
				align='center'
				break;
				case 1:
					align='left'
					break;
					case 2:
						align='right'
						break;
		}
		let fontSize='1em';
		switch(p.fontSize){
			default:
			case 0:
				fontSize='0.55em'
				break;
			case 1:
				fontSize='0.6em'
				break;
			case 2:
				fontSize='0.7em'
				break;
			case 3:
				fontSize='0.9em'
				break;
			case 4:
				fontSize='1.0em'
				break;
			case 5:
				fontSize='1.1em'
				break;
			case 6:
			fontSize='1.4em'
			break;
		}
		let fontWeight='300';
		switch(p.fontWeight){
			default:
			case 0:
				fontWeight='300'
				break;
			case 1:
				fontWeight='700'
				break;
		}
		let rotate = '0';
		switch(p.rotate) {
			default:
			case 0:
				rotate='rotate(0)';
				break;
			case 1:
				rotate='rotate(45deg)';
				break;
			case 2:
				rotate='rotate(90deg)';
				break;
			case 3:
				rotate='rotate(135deg)';
				break;
			case 4:
				rotate='rotate(180deg)';
				break;
			case 5:
				rotate='rotate(225deg)';
				break;
			case 6:
				rotate='rotate(270deg)';
				break;
			case 7:
				rotate='rotate(315deg)';
				break;
		}
		return {
			color: color,
			textAlign: align,
			fontSize: fontSize,
			fontWeight: fontWeight,
			transform: rotate,
			width: box.width,
			height: box.height
		};
	};

	const stylesToApply = {...style, width: width, height: height };

	const handleKeyDown = (e) => {
		const name = e.target.getAttribute("name");
		const el = document.getElementById(e.target.getAttribute("id"));
		const elBounds = { width: el?.clientWidth || 0, height: el?.clientHeight || 0, x: el?.clientLeft || 0, left: el?.clientLeft || 0, y: el?.clientTop || 0, top: el?.clientTop || 0 };
		switch(e.key) {
			case "Delete":
				// delete the focused box
				const itemToRemove = _.find(internalBoxes, i => i.name === name);
				if (onRemove) onRemove(itemToRemove);
				const newBoxes = _.filter(internalBoxes, i => i.name !== name);
				setInternalBoxes(newBoxes);
				break;
			case "ArrowUp": 
			{
				e.preventDefault();
				const boxToMove = _.find(internalBoxes, i => i.name === name);
				boxToMove.top -= 1;
				if (boxToMove.top < 0)
					boxToMove.top = 0;
				setInternalBoxes(updateStateItem(internalBoxes, boxToMove, "name", name));
				if (onMove) onMove(boxToMove);
				break;
			}
			case "ArrowDown":
			{
				e.preventDefault();
				const boxToMove = _.find(internalBoxes, i => i.name === name);
				boxToMove.top += 1;
				if (boxToMove.top + elBounds.height + 2 > height)
					boxToMove.top = height - elBounds.height - 2;
					setInternalBoxes(updateStateItem(internalBoxes, boxToMove, "name", name));
				if (onMove) onMove(boxToMove);
				break;
			}
			case "ArrowLeft":
			{
				e.preventDefault();
				const boxToMove = _.find(internalBoxes, i => i.name === name);
				boxToMove.left -= 1;
				if (boxToMove.left < 0)
					boxToMove.left = 0;
					setInternalBoxes(updateStateItem(internalBoxes, boxToMove, "name", name));
				if (onMove) onMove(boxToMove);
				break;
			}
			case "ArrowRight":
			{
				e.preventDefault();
				const boxToMove = _.find(internalBoxes, i => i.name === name);
				boxToMove.left += 1;
				if (boxToMove.left + elBounds.width + 2 >= width) {
					boxToMove.left = width - elBounds.width - 3;
				}
				setInternalBoxes(updateStateItem(internalBoxes, boxToMove, "name", name));
				if (onMove) onMove(boxToMove);
				break;
			}
			default:
				break;
		}
	};

	const getBox = (box) => {
		const propsForBox = _.find(itemProperties, i => i.name === box.name);
		if (propsForBox && propsForBox.value && propsForBox.value.length > 0 && box.acceptsValue && box.displayValue) {
			return {...box, children: propsForBox.value };
		} else if(propsForBox && !propsForBox.children && propsForBox.name) {
			return {...box, children: getChildrenByName(propsForBox.name) };
		} else {
			return box;
		}
	};

	const getMargins = (margin) => {
		const margins = [0, 0, 0, 0];
		let marginDef = margin?.split(' ') || [];
	
		for(let i = 0; i < marginDef.length; i++)
			margins[i] = parseInt(marginDef[i]);
	
		if (marginDef.length === 1) {
			margins[1] = margins[2] = margins[3] = margins[0];
		} else if (marginDef.length === 2) {
			margins[2] = margins[0];
			margins[3] = margins[1];
		}
		return margins;
	};

	const margins = getMargins(margin);
  return (
    <div ref={drop} id="container" style={stylesToApply}>
			{/* draw margin box */}
			{(margins[0] > 0 || margins[1] > 0 || margins[2] > 0 || margins[3] > 0) 
			? <div style={{
				border: '1px dotted #c00', 
				position: 'absolute', 
				left: margins[3] + 'px', 
				top: margins[0] + 'px', 
				width: (width - margins[1] - margins[3] - 1) + 'px', 
				height: (height - margins[0] - margins[2] - 1) + 'px'
			}} /> : ''}
      {internalBoxes.map((box, key) => (
        <DraggableBox 
					key={key} 
					id={`${box.id}-${box.name}`}
					{...getBox(box)} 
					style={getItemPropertyStyle(box.name)} 
					absolute 
					onClick={e => handleSelectedPart(e, box)} 
					onKeyDown={handleKeyDown}
					resize={box.resize}
				/>
      ))}
    </div>
  );
}

DropArea.propTypes = {
  onSelectedItemChanged: PropTypes.func,
	onDrop: PropTypes.func,
	onMove: PropTypes.func,
	onRemove: PropTypes.func,
	itemProperties: PropTypes.array,
	clearSelectedItem: PropTypes.any,
	boxes: PropTypes.array,
	margin: PropTypes.string,
	width: PropTypes.number,
	height: PropTypes.number,
};

DropArea.defaultProps = {
	boxes: [],
	margin: "0 0 0 0"
};
