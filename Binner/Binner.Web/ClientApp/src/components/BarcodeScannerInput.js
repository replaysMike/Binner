import { useState, useEffect, useMemo, useRef } from "react";
import { Link } from "react-router-dom";
import { Popup, Image } from "semantic-ui-react";
import PropTypes from "prop-types";
import debounce from "lodash.debounce";

/**
 * Handles generic barcode scanning input by listening for batches of key presses
 */
export function BarcodeScannerInput({listening, minInputLength, onReceived, helpUrl}) {
  const BufferTimeMs = 500;
	const [keyBuffer, setKeyBuffer] = useState([]);
  const [isKeyboardListening, setIsKeyboardListening] = useState(listening || true);
	const listeningRef = useRef();
	listeningRef.current = isKeyboardListening;
	const keyBufferRef = useRef();
	keyBufferRef.current = keyBuffer;

  const onReceivedBarcodeInput = (e, buffer) => {
    if (buffer.length < minInputLength) {
			keyBufferRef.current.length = 0;
    	return; // drop and ignore input
		}

    const value = processKeyBuffer(buffer);
		// reset key buffer
		keyBufferRef.current.length = 0;
		// process raw value into an input object with decoded information
    const input = processBarcodeInformation(e, value);
		// fire an event that we received data
		onReceived(e, input);
  };

  const processKeyBuffer = (buffer) => {
    let str = "";
    let specialCharBuffer = [];
    for (let i = 0; i < buffer.length; i++) {
      let key = buffer[i];

      // check for alt key
      if (key.keyCode === 18) {
        // it's a special character, read until alt is no longer pressed
        specialCharBuffer = [];
				continue;
      } else if (key.altKey) {
        // add special character
        specialCharBuffer.push(key.key);
				continue;
      } else if (specialCharBuffer.length > 0) {
        // process special character string into the actual ASCII character
        const charStr = specialCharBuffer.join("");
        const charCode = parseInt(charStr);
        const char = String.fromCharCode(charCode);
        str += char;
        specialCharBuffer = [];
      }  
			
			// normal character
			let char = String.fromCharCode(96 <= key.keyCode && key.keyCode <= 105 ? key.keyCode - 48 : key.keyCode);
			
			if (key.shiftKey) char = key.key;
			if ((key.keyCode >= 186 && key.keyCode <= 192) || (key.keyCode >= 219 && key.keyCode <= 222)) char = key.key;
			if (
				key.keyCode === 13 ||
				key.keyCode === 32 ||
				key.keyCode === 9 ||
				(key.keyCode >= 48 && key.keyCode <= 90) ||
				(key.keyCode >= 107 && key.keyCode <= 111) ||
				(key.keyCode >= 186 && key.keyCode <= 222)
			) {
				str += char;
			}
    }
    return str;
  };

  const processBarcodeInformation = (e, value) => {
    let barcodeType = "code128";
    let parsedValue = {};
		let gsDetected = false;
		let rsDetected = false;
		let eotDetected = false;
    if (value.startsWith("[)>")) {
      // 2D DotMatrix barcode. Process into value.
      barcodeType = "datamatrix";
      const parseResult = parseDataMatrix(value);
			parsedValue = parseResult.value;
			gsDetected = parseResult.gsDetected;
			rsDetected = parseResult.rsDetected;
			eotDetected = parseResult.eotDetected;
    } else {
      // 1D barcode
      parsedValue = value.replace("\n", "").replace("\r", "");
    }

		return {
			type: barcodeType,
			value: parsedValue,
			rawValue: value,
			rsDetected,
			gsDetected,
			eotDetected
		};
  };

  const parseDataMatrix = (value) => {
    let parsedValue = {};
    const gsCharCode = 29;
    const rsCharCode = 30;
    const eotCharCode = 4;
    const header = "[)>";
    const expectedFormatNumber = 6; /** 22z22 barcode */
    const controlChars = ["P", "1P", "P1", "K", "1K", "10K", "11K", "4L", "Q", "11Z", "12Z", "13Z", "20Z"];

		let gsCodePresent = false;
		let rsCodePresent = false;
		let eotCodePresent = false;
    let formatNumber = "";
    let buffer = "";
    let i;
    let formatNumberIndex = 0;
    for (i = 0; i < value.length; i++) {
      buffer += value[i];
      if (buffer === header) {
        if (value.charCodeAt(i + 1) === rsCharCode) {
          // read the character after the RS token (sometimes not present)
					rsCodePresent = true;
          formatNumberIndex = i + 2;
        } else {
          formatNumberIndex = i + 1;
        }
        formatNumber = parseInt(value[formatNumberIndex] + value[formatNumberIndex + 1]);
				i += formatNumberIndex + 1;
        break;
      }
    }
    if (formatNumber !== expectedFormatNumber) {
      // error
			console.error(`Expected the 2D barcode format number of ${expectedFormatNumber} but was ${formatNumber}`);
      return {};
    }

    let lastPosition = i;
		const gsLines = [];
		let gsLine = '';
		for (i = lastPosition; i < value.length; i++) {
			const ch = value[i];
			if (ch.charCodeAt(0) === gsCharCode) {
				gsCodePresent = true;
				// start of a new line. read until next gsCharCode or EOT
				if (gsLine.length > 0)
					gsLines.push(gsLine);
				gsLine = '';
			} else {
				gsLine += ch;
			}
			if (ch.charCodeAt(0) === eotCharCode) {
				eotCodePresent = true;
			}
		}
		if (gsLine.length > 0)
			gsLines.push(gsLine);

		if (!gsCodePresent) {
			// no gs codes present
			console.log('no gs code present');
			let readCommandType = "";
			let readValue = "";
			let readControlChars = true;
			for (i = lastPosition - 1; i < value.length; i++) {
				if (readControlChars) readCommandType += value[i];
				else readValue += value[i];

				//if (readControlChars === header || readControlChars === formatNumber) readValue = "";
				if (readControlChars && controlChars.includes(readCommandType)) {
					// start reading value
					readControlChars = false;
					console.log('found command', readCommandType);
				}
				// have we finished reading the value?
				if(!readControlChars && readValue.length > 0) {
					console.log('readValue', readValue);
					for(let c = 0; c < controlChars.length; c++) {
						if (controlChars[c] !== readCommandType && readValue.endsWith(controlChars[c])) {
							// process the command + value
							console.log('end readValue raw', readValue);
							readValue = readValue.substring(0, readValue.length - controlChars[c].length);
							console.log('end readValue', readValue, readCommandType);
							
							switch (readCommandType) {
								case "P":
									// could be DigiKey part number, or customer reference value
									console.log('description', readCommandType, readValue);
									parsedValue["description"] = readValue;
									break;
								case "1P":
									// manufacturer part number
									console.log('mfgPartNumber', readCommandType, readValue);
									parsedValue["mfgPartNumber"] = readValue;
									break;
								case "1K":
									// Salesorder#
									console.log('salesOrder', readCommandType, readValue);
									parsedValue["salesOrder"] = readValue;
									break;
								case "10K":
									// invoice#
									parsedValue["invoice"] = readValue;
									break;
								case "11K":
									// don't know
									parsedValue["unknown1"] = readValue;
									break;
								case "4L":
									// country of origin
									parsedValue["countryOfOrigin"] = readValue;
									break;
								case "Q":
									// quantity
									parsedValue["quantity"] = readValue;
									break;
								case "11Z":
									// the value PICK
									parsedValue["pick"] = readValue;
									break;
								case "12Z":
									// internal part id
									parsedValue["partId"] = readValue;
									break;
								case "13Z":
									// shipment load id
									parsedValue["loadId"] = readValue;
									break;
								default:
									break;
							}
							// set the next read command type
							readCommandType = controlChars[c];
							readValue = "";
							readControlChars = false;
							console.log('found command', readCommandType);
							break;
						}
					}
				}
			}
		}else{

			for (i = 0; i < gsLines.length; i++) {
				// read until we see a control char
				const line = gsLines[i];
				let readCommandType = "";
				let readValue = "";
				let readControlChars = true;
				for (var c = 0; c < line.length; c++) {
					if (readControlChars) readCommandType += line[c];
					else readValue += line[c];

					if (readControlChars === header || readControlChars === formatNumber) readValue = "";
					if (controlChars.includes(readCommandType)) {
						// start reading value
						readControlChars = false;
					}
				}
				switch (readCommandType) {
					case "P":
						// could be DigiKey part number, or customer reference value
						parsedValue["description"] = readValue;
						break;
					case "1P":
						// manufacturer part number
						parsedValue["mfgPartNumber"] = readValue;
						break;
					case "1K":
						// Salesorder#
						parsedValue["salesOrder"] = readValue;
						break;
					case "10K":
						// invoice#
						parsedValue["invoice"] = readValue;
						break;
					case "11K":
						// don't know
						parsedValue["unknown1"] = readValue;
						break;
					case "4L":
						// country of origin
						parsedValue["countryOfOrigin"] = readValue;
						break;
					case "Q":
						// quantity
						parsedValue["quantity"] = readValue;
						break;
					case "11Z":
						// the value PICK
						parsedValue["pick"] = readValue;
						break;
					case "12Z":
						// internal part id
						parsedValue["partId"] = readValue;
						break;
					case "13Z":
						// shipment load id
						parsedValue["loadId"] = readValue;
						break;
					default:
						break;
				}
			}
		}
		console.log('parsedValue', parsedValue);
    return {
			value: parsedValue,
			gsDetected: gsCodePresent,
			rsDetected: rsCodePresent,
			eotDetected: eotCodePresent
		};
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
		// handle changes to the incoming listening prop
    setIsKeyboardListening(listening);
  }, [listening]);

  const addKeyboardHandler = () => {
    if (document) {
      document.addEventListener("keydown", onKeydown);
    }
  };

  const removeKeyboardHandler = () => {
    if (document) {
      document.removeEventListener("keydown", onKeydown);
    }
  };

  // listens for document keydown events, used for barcode scanner input
  const onKeydown = (e) => {
    if (listeningRef.current === true) {
			e.preventDefault();
			e.stopPropagation();
			keyBufferRef.current.push(e);
      scannerDebounced(e, keyBufferRef.current);
    } else {
			// dropped key, not listening
		}
  };

  return (
    <div style={{ float: "right" }}>
      <Popup
        position="bottom right"
        hoverable
        content={
          <p>
            This page supports barcode scanning. <Link to={helpUrl}>More Info</Link>
          </p>
        }
        trigger={<Image src="/image/barcode.png" width={35} height={35} className="barcode-support" />}
      />
    </div>
  );
}

BarcodeScannerInput.propTypes = {
  /** Event handler when scanning input has been received */
  onReceived: PropTypes.func.isRequired,
  /** Set this to true to listen for barcode input */
  listening: PropTypes.bool,
  /** keyboard buffer smaller than this length will ignore input */
  minInputLength: PropTypes.number,
  helpUrl: PropTypes.string
};

BarcodeScannerInput.defaultProps = {
  listening: true,
  minInputLength: 4,
  helpUrl: "/help/scanning"
};
