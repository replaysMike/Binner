import { useState, useEffect, useMemo } from "react";
import PropTypes from "prop-types";
import debounce from "lodash.debounce";

/**
 * Handles generic barcode scanning input by listening for batches of key presses
 */
export function BarcodeScannerInput(props) {
	const BufferTimeMs = 100;
	let barcodeBuffer = "";
	const [isKeyboardListening, setIsKeyboardListening] = useState(props.listening || true);

	const onReceivedBarcodeInput = (e, value) => {
		barcodeBuffer = "";
		if (value.length < props.minInputLength)
			return; // drop and ignore input
		if (props.onReceived) props.onReceived(e, value);
  };

  const scannerDebounced = useMemo(() => debounce(onReceivedBarcodeInput, BufferTimeMs), []);

	useEffect(() => {
		// start listening for all key presses on page
    addKeyboardHandler();
    return () => {
			// stop listening for key presses
      removeKeyboardHandler();
    };
  }, []);

	useEffect(() => {
    setIsKeyboardListening(props.listening);
  }, [props.listening]);

	const addKeyboardHandler = () => {
    if (document) document.addEventListener("keydown", onKeydown);
  };

  const removeKeyboardHandler = () => {
    if (document) document.removeEventListener("keydown");
  };

	// listens for document keydown events, used for barcode scanner input
  const onKeydown = (e) => {
    if (isKeyboardListening) {
      let char = String.fromCharCode(96 <= e.keyCode && e.keyCode <= 105 ? e.keyCode - 48 : e.keyCode);
      // map proper value when shift is used
      if (e.shiftKey) char = e.key;
      // map numlock extra keys
      if ((e.keyCode >= 186 && e.keyCode <= 192) || (e.keyCode >= 219 && e.keyCode <= 222)) char = e.key;
      if (
        e.keyCode === 13 ||
        e.keyCode === 32 ||
        e.keyCode === 9 ||
        (e.keyCode >= 48 && e.keyCode <= 90) ||
        (e.keyCode >= 107 && e.keyCode <= 111) ||
        (e.keyCode >= 186 && e.keyCode <= 222)
      ) {
        barcodeBuffer += char;
        scannerDebounced(e, barcodeBuffer);
      }
    }
  };
};

BarcodeScannerInput.propTypes = {
  /** Event handler when scanning input has been received */
  onReceived: PropTypes.func.isRequired,
	/** Set this to true to listen for barcode input */
	listening: PropTypes.bool,
	/** keyboard buffer smaller than this length will ignore input */
	minInputLength: PropTypes.number
};

BarcodeScannerInput.defaultProps = {
	listening: true,
	minInputLength: 4
};