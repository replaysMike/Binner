import { useCallback, useState, useEffect } from 'react';
import PropTypes from "prop-types";
import { useDrop } from 'react-dnd';
import { DraggableBox } from './DraggableBox.js';
import { ItemTypes } from './ItemTypes.js';
import { snapToGrid as doSnapToGrid } from './snapToGrid.js';
import { updateStateItem } from '../../common/reactHelpers.js';
import _ from 'underscore';

const style = {
  border: '1px solid black',
  position: 'relative',
};

export const DropArea = ({ snapToGrid, onDrop, onMove, onRemove, width = 300, height = 300, margin, padding, onSelectedItemChanged, itemProperties, clearSelectedItem }) => {
	const [boxes, setBoxes] = useState([]);

	const deselectAll = useCallback(() => {
		if (boxes.length > 0) {
			for(var i = 0; i < boxes.length; i++) {
				boxes[i].selected = false;
			}
		}
	}, [boxes]);

	useEffect(() => {
		deselectAll();
		setBoxes([...boxes]);
		if (onSelectedItemChanged) onSelectedItemChanged(null, null);
	}, [clearSelectedItem]);

	const setSelectedItem = (item) => {
		const originalSelected = item.selected;
		deselectAll();
		item.selected = true;
		if (onSelectedItemChanged && originalSelected !== item.selected) onSelectedItemChanged(null, item);
	};

  const moveBox = useCallback(
    (boxes, id, left, top) => {
			const newBoxes = [...boxes];
			const item = _.find(newBoxes, i => i.id === id);
			if (item) {
				item.left = left;
				item.top = top;
				setSelectedItem(item);
				setBoxes(newBoxes);
			} else {
				console.log('err could not find item with id ', id, boxes);
			}
    },
    [],
  );
  const [{ isOver, draggingColor, canDrop }, drop] = useDrop(
    () => ({
      accept: [ItemTypes.DraggableBox, ItemTypes.Box],
      drop(item, monitor) {
				// fired when a new box has been dropped on the drop area
				let newBoxes = boxes;
				let newItem = item;
				let name = newItem.name;
				let resize = newItem.resize;
				let acceptsValue = newItem.acceptsValue;
				let displayValue = newItem.displayValue;
				let left = newItem.left;
				let top = newItem.top;
				const boundingBox = document.getElementById('container').getBoundingClientRect();

				const exists = _.find(boxes, i => i.id === item.id);
				if (!exists) {
					// add new item to the drop area
					const offset = monitor.getSourceClientOffset();
					const deltaX = offset.x - boundingBox.x;
					const deltaY = offset.y - boundingBox.y;
					newBoxes = [...boxes];
					const id = newBoxes.length > 0 ? newBoxes[newBoxes.length - 1].id + 1 : 1;
					newItem = {
						id: id,
						name: name,
						top: deltaY, 
						left: deltaX, 
						children: item.children || (<span>No label set!</span>),
						selected: false,
						resize: resize,
						acceptsValue: acceptsValue,
						displayValue: displayValue
					};
					top = newItem.top;
					left = newItem.left;
					newBoxes.push(newItem);
					setBoxes([...newBoxes]);
					setSelectedItem(newItem);
					if (onDrop) onDrop(newItem);
				} else {
					// move an existing item on the drop area
					const delta = monitor.getDifferenceFromInitialOffset();
					left = Math.round(newItem.left + delta.x);
					top = Math.round(newItem.top + delta.y);
					if (snapToGrid) {
						[left, top] = doSnapToGrid(left, top);
					}
					moveBox(newBoxes, newItem.id, left, top);
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
    [boxes, moveBox],
  );

	const handleSelectedPart = (e, box, key) => {
		deselectAll();
		setSelectedItem(box);
		setBoxes([...boxes]);
	};

	const getItemPropertyStyle = (name) => {
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
				fontSize='0.5em'
				break;
				case 1:
					fontSize='0.6em'
					break;
				case 2:
					fontSize='0.7em'
					break;
				case 3:
					fontSize='0.8em'
				break;
				case 4:
					fontSize='0.9em'
					break;
				case 5:
					fontSize='1.0em'
					break;
				case 6:
				fontSize='1.2em'
				break;
		}
		let fontWeight='300';
		switch(p.fontWeight){
			default:
			case 0:
				fontWeight='100'
				break;
				case 1:
					fontWeight='300'
					break;
				case 2:
					fontWeight='500'
					break;
					case 3:
					fontWeight='700'
					break;
		}
		return {
			color: color,
			textAlign: align,
			fontSize: fontSize,
			fontWeight: fontWeight
		};
	};

	const stylesToApply = {...style, width: width, height: height };

	const handleKeyDown = (e) => {
		const name = e.target.getAttribute("name");
		switch(e.key) {
			case "Delete":
				// delete the focused box
				const itemToRemove = _.find(boxes, i => i.name === name);
				if (onRemove) onRemove(itemToRemove);
				const newBoxes = _.filter(boxes, i => i.name !== name);
				setBoxes(newBoxes);
				break;
			case "ArrowUp": 
			{
				const boxToMove = _.find(boxes, i => i.name === name);
				boxToMove.top -= 1;
				setBoxes(updateStateItem(boxes, boxToMove, "name", name));
				break;
			}
			case "ArrowDown":
			{
				const boxToMove = _.find(boxes, i => i.name === name);
				boxToMove.top += 1;
				setBoxes(updateStateItem(boxes, boxToMove, "name", name));
				break;
			}
			case "ArrowLeft":
			{
				const boxToMove = _.find(boxes, i => i.name === name);
				boxToMove.left -= 1;
				setBoxes(updateStateItem(boxes, boxToMove, "name", name));
				break;
			}
			case "ArrowRight":
			{
				const boxToMove = _.find(boxes, i => i.name === name);
				boxToMove.left += 1;
				setBoxes(updateStateItem(boxes, boxToMove, "name", name));
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
		} else{
			return box;
		}
	};
	
  return (
    <div ref={drop} id="container" style={stylesToApply}>
			<div style={{border: '1px dotted #c00', position: 'absolute', top: margin, left: margin, width: (width - (margin * 2) - 1) + 'px', height: (height - (margin * 2) - 1) + 'px'}} />
      {boxes.map((box, key) => (
        <DraggableBox 
					key={key} 
					id={key} 
					{...getBox(box)} 
					style={getItemPropertyStyle(box.name)} 
					absolute 
					onClick={e => handleSelectedPart(e, box, key)} 
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
	clearSelectedItem: PropTypes.any
};