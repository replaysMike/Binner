import { useCallback, useState } from 'react';
import PropTypes from "prop-types";
import { DropArea } from './DropArea';
import { CustomDragLayer } from './CustomDragLayer';

export const LabelDropContainer = ({width, height, onSelectedItemChanged, itemProperties}) => {
  const [snapToGridAfterDrop, setSnapToGridAfterDrop] = useState(false);
  const [snapToGridWhileDragging, setSnapToGridWhileDragging] = useState(false);
  const [clearSelectedItem, setClearSelectedItem] = useState(null);

  const handleSnapToGridAfterDropChange = useCallback(() => {
    setSnapToGridAfterDrop(!snapToGridAfterDrop)
  }, [snapToGridAfterDrop]);

  const handleSnapToGridWhileDraggingChange = useCallback(() => {
    setSnapToGridWhileDragging(!snapToGridWhileDragging)
  }, [snapToGridWhileDragging]);

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
        onSelectedItemChanged={onSelectedItemChanged} 
        itemProperties={itemProperties}
        clearSelectedItem={clearSelectedItem}
      />
      <CustomDragLayer snapToGrid={snapToGridWhileDragging} />
    </div>
  );
}

DropArea.propTypes = {
  onSelectedItemChanged: PropTypes.func,
  width: PropTypes.number,
  height: PropTypes.number,
  itemProperties: PropTypes.array
};

DropArea.defaultProps = {
  width: 500,
  height: 300
};