import { useCallback, useState, useEffect } from 'react';
import PropTypes from "prop-types";
import { DropArea } from './DropArea';
import { CustomDragLayer } from './CustomDragLayer';

export const LabelDropContainer = ({width, height, margin, padding, onSelectedItemChanged, onDrop, onMove, onRemove, itemProperties, resetSelectedItem, updateState, boxes}) => {
  const [snapToGridAfterDrop, setSnapToGridAfterDrop] = useState(false);
  const [snapToGridWhileDragging, setSnapToGridWhileDragging] = useState(false);
  const [clearSelectedItem, setClearSelectedItem] = useState(null);

  const handleSnapToGridAfterDropChange = useCallback(() => {
    setSnapToGridAfterDrop(!snapToGridAfterDrop)
  }, [snapToGridAfterDrop]);

  const handleSnapToGridWhileDraggingChange = useCallback(() => {
    setSnapToGridWhileDragging(!snapToGridWhileDragging)
  }, [snapToGridWhileDragging]);

  useEffect(() => {
    // using this as a cheap hack to reset the selected item
    setClearSelectedItem(Math.random());
  }, [resetSelectedItem]);

  const handleOnClick = (e) => {
    // using this as a cheap hack to reset the selected item
    setClearSelectedItem(Math.random());
  };

  return (
    <div onClick={handleOnClick}>
      <DropArea 
        snapToGrid={snapToGridAfterDrop} 
        width={width} 
        height={height} 
        margin={margin}
        padding={padding}
        onSelectedItemChanged={onSelectedItemChanged} 
        onDrop={onDrop}
        onMove={onMove}
        onRemove={onRemove}
        itemProperties={itemProperties}
        clearSelectedItem={clearSelectedItem}
        updateState={updateState}
        boxes={boxes}
      />
      <CustomDragLayer snapToGrid={snapToGridWhileDragging} />
    </div>
  );
}

DropArea.propTypes = {
  onSelectedItemChanged: PropTypes.func,
  onDrop: PropTypes.func,
  onMove: PropTypes.func,
  onRemove: PropTypes.func,
  width: PropTypes.number,
  height: PropTypes.number,
  margin: PropTypes.number,
  padding: PropTypes.number,
  itemProperties: PropTypes.array,
  resetSelectedItem: PropTypes.any,
  updateState: PropTypes.any,
  boxes: PropTypes.array
};

DropArea.defaultProps = {
  width: 500,
  height: 300,
  margin: 0,
  padding: 0,
  boxes: []
};