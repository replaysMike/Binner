import { useState, useEffect, useMemo, useRef } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Link } from "react-router-dom";
import { Popup, Image } from "semantic-ui-react";
import PropTypes from "prop-types";
import debounce from "lodash.debounce";
import { Events } from "../common/events";
import useSound from 'use-sound';
import boopSfx from '../audio/softbeep.mp3';
import { copyString } from "../common/Utils";

/**
 * Handles generic barcode scanning input by listening for batches of key presses
 */
export function BarcodeScannerInput({listening, minInputLength, onReceived, helpUrl, swallowKeyEvent, passThrough,enableSound}) {
  const BufferTimeMs = 500;
	const [keyBuffer, setKeyBuffer] = useState([]);
  const [isKeyboardListening, setIsKeyboardListening] = useState(listening || true);
	const [previousIsKeyboardListeningState, setPreviousIsKeyboardListeningState] = useState(listening || true);
	const [playScanSound] = useSound(boopSfx, { soundEnabled: true, volume: 1 });
	const [isReceiving, setIsReceiving] = useState(false);
	const listeningRef = useRef();
	listeningRef.current = isKeyboardListening;
	const keyBufferRef = useRef();
	keyBufferRef.current = keyBuffer;
	const playScanSoundRef = useRef();
  playScanSoundRef.current = playScanSound;

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

		if (enableSound) playScanSoundRef.current();
		// fire an event that we received data
		onReceived(e, input);
		setIsReceiving(false);
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
			let char = key.isFake ? key.key : String.fromCharCode(96 <= key.keyCode && key.keyCode <= 105 ? key.keyCode - 48 : key.keyCode);
			
			if (key.shiftKey) char = key.key;
			if ((key.keyCode >= 186 && key.keyCode <= 192) || (key.keyCode >= 219 && key.keyCode <= 222)) char = key.key;
			if (
				key.isFake ||
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
		let correctedValue = value;
		let gsDetected = false;
		let rsDetected = false;
		let eotDetected = false;
		let invalidBarcodeDetected = false;
    if (value.startsWith("[)>")) {
      // 2D DotMatrix barcode. Process into value.
      barcodeType = "datamatrix";
      const parseResult = parseDataMatrix(value);
			parsedValue = parseResult.value;
			gsDetected = parseResult.gsDetected;
			rsDetected = parseResult.rsDetected;
			eotDetected = parseResult.eotDetected;
			invalidBarcodeDetected = parseResult.invalidBarcodeDetected;
			correctedValue = parseResult.correctedValue;
    } else {
      // 1D barcode
      parsedValue = value.replace("\n", "").replace("\r", "");
    }

		return {
			type: barcodeType,
			value: parsedValue,
			correctedValue: correctedValue,
			rawValue: value,
			rsDetected,
			gsDetected,
			eotDetected,
			invalidBarcodeDetected
		};
  };

  const parseDataMatrix = (value) => {
    let parsedValue = {};
		// https://honeywellaidc.force.com/supportppr/s/article/What-do-Control-Characters-SOH-STX-etc-mean-when-scanning
		const gsCharCodes = ["\u001d", "\u005d", "\u241d"]; // CTRL-], \u001d, GROUP SEPARATOR
		const rsCharCodes = ["\u001e", "\u005e", "\u241e"]; // CTRL-^, \u001e, RECORD SEPARATOR
    const eotCharCodes = ["\u0004", "^\u0044", "\u2404"]; // CTRL-D, \u0004, END OF TRANSMISSION
		const crCharCodes = ["\r", "\u240d"]; // 13, line feed
		const lfCharCodes = ["\n", "\u240a"]; // 10, carriage return
		const fileSeparatorCharCodes = ["\u001c", "\u241c"]; // ctl-\, \u001c FILE SEPARATOR 
		const sohCharCodes = ["\u0001"]; // CTRL-A, \u0001 START OF HEADER
		const stxCharCodes = ["\u0002"]; // CTRL-B, \u0002 START OF TEXT
		const etxCharCodes = ["\u0003"]; // CTRL-C, \u0003 END OF TEXT
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
		let correctedValue = value.toString();
		// normalize the control codes so we don't have multiple values to worry about
		correctedValue = normalizeControlCharacters(correctedValue);

		correctedValue = correctedValue.replaceAll("\u001d", "\u241d"); // GS
		correctedValue = correctedValue.replaceAll("\u005d", "\u241d"); // GS

		correctedValue = correctedValue.replaceAll("\u001e", "\u241e"); // RS
		correctedValue = correctedValue.replaceAll("\u005e", "\u241e"); // RS
		correctedValue = correctedValue.replaceAll("\u0004", "\u2404"); // EOT
		correctedValue = correctedValue.replaceAll("^\u0044", "\u2404"); // EOT

		gsCodePresent = gsCharCodes.some(v => correctedValue.includes(v));
		rsCodePresent = rsCharCodes.some(v => correctedValue.includes(v));
		eotCodePresent = eotCharCodes.some(v => correctedValue.includes(v));

		// read in the format number first. For Digikey 2d barcodes, this should be 6 (expectedFormatNumber)
    for (i = 0; i < correctedValue.length; i++) {
      buffer += correctedValue[i];
      if (buffer === header) {
        if (rsCharCodes.includes(correctedValue[i + 1])) {
          // read the character after the RS token (sometimes not present)
          formatNumberIndex = i + 2;
        } else {
          formatNumberIndex = i + 1;
        }
        formatNumber = parseInt(correctedValue[formatNumberIndex] + correctedValue[formatNumberIndex + 1]);
				i += formatNumberIndex + 1;
        break;
      }
    }
		// assert expected barcode format number
    if (formatNumber !== expectedFormatNumber) {
      // error
			console.error(`Expected the 2D barcode format number of ${expectedFormatNumber} but was ${formatNumber}`);
      return {};
    }

    let lastPosition = i;
		let gsLines = [];
		let gsLine = '';
		// break each group separator into an array
		for (i = lastPosition; i < correctedValue.length; i++) {
			const ch = correctedValue[i];
			if (gsCharCodes.includes(ch)) {
				// start of a new line. read until next gsCharCode or EOT
				if (gsLine.length > 0)
					gsLines.push(gsLine);
				gsLine = '';
			} else {
				gsLine += ch;
			}
		}
		if (gsLine.length > 0)
			gsLines.push(gsLine);

		let invalidBarcodeDetected = false;
		// some older DigiKey barcodes are encoded incorrectly, and have a blank GSRS at the end. Filter them out.
		// https://github.com/replaysMike/Binner/issues/132
		if (isInvalidBarcode(gsLines)) {
			gsLines = fixInvalidBarcode(gsLines);
			invalidBarcodeDetected = true;
		}
		let readLength = gsLines.length;
		// read each group separator
		for (i = 0; i < readLength; i++) {
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
					const parsedIntValue = parseInt(readValue);
					if (isNaN(parsedIntValue))
						parsedValue["quantity"] = readValue;
					else
						parsedValue["quantity"] = parsedIntValue;
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

		correctedValue = buildBarcode(expectedFormatNumber, gsLines);
    return {
			rawValue: value,
			value: parsedValue,
			correctedValue: correctedValue,
			gsDetected: gsCodePresent,
			rsDetected: rsCodePresent,
			eotDetected: eotCodePresent,
			gsLines: gsLines,
			invalidBarcodeDetected
		};
  };

	const buildBarcode = (formatNumber, gsLines) => {
		let barcode = `[)>\u241e${formatNumber.toString().padStart(2, '0')}`; // Header + RS + formatNumber
		for(let i = 0; i < gsLines.length; i++){
			barcode = barcode + "\u241d" + gsLines[i]; // GS
		}
		barcode = barcode + "\u2404\r"; // EOT + CR
		return barcode;
	};

	const normalizeControlCharacters = (str) => {
		// convert all variations of the control code to their equiv unicode value
		let normalizedStr = copyString(str);
		normalizedStr = normalizedStr.replaceAll("\u001d", "\u241d"); // GS
		normalizedStr = normalizedStr.replaceAll("\u005d", "\u241d"); // GS

		normalizedStr = normalizedStr.replaceAll("\u001e", "\u241e"); // RS
		normalizedStr = normalizedStr.replaceAll("\u005e", "\u241e"); // RS
		normalizedStr = normalizedStr.replaceAll("\u0004", "\u2404"); // EOT
		normalizedStr = normalizedStr.replaceAll("^\u0044", "\u2404"); // EOT
		return normalizedStr;
	};

	const isInvalidBarcode = (gsLines) => {
		for(let i = 0; i < gsLines.length; i++){ 
			if (gsLines[i].includes("\u241e")) { // RS
				return true;
			}
		}
		return false;
	};

	const fixInvalidBarcode = (gsLines) => {
		const newGsLines = [];
		for(let i = 0; i < gsLines.length; i++){ 
			if (gsLines[i].includes("\u241e")) { // RS
				// is there data before the RS character?
				const rsIndex = gsLines[i].indexOf("\u241e");
				if (rsIndex > 0) {
					const data = gsLines[i].substring(0, rsIndex);
					newGsLines.push(data);
				}
				continue;
			}
			newGsLines.push(gsLines[i]);
		}
		return newGsLines;
	};

  const scannerDebounced = useMemo(() => debounce(onReceivedBarcodeInput, BufferTimeMs), []);

	const disableBarcodeInput = (e) => {
		setPreviousIsKeyboardListeningState(isKeyboardListening);
		setIsKeyboardListening(false);
	};

	const restoreBarcodeInput = (e) => {
		setIsKeyboardListening(previousIsKeyboardListeningState);
	};

  useEffect(() => {
    // start listening for all key presses on page
    addKeyboardHandler();
		// add event listeners to receive requests to disable/enable barcode capture
		document.body.addEventListener(Events.DisableBarcodeInput, disableBarcodeInput);
		document.body.addEventListener(Events.RestoreBarcodeInput, restoreBarcodeInput);
    return () => {
      // stop listening for key presses
      removeKeyboardHandler();
			// remove event listeners
			document.body.removeEventListener(Events.DisableBarcodeInput, disableBarcodeInput);
			document.body.removeEventListener(Events.RestoreBarcodeInput, restoreBarcodeInput);
    };
  }, []);

  useEffect(() => {
		// handle changes to the incoming listening prop
    setIsKeyboardListening(listening);
		listeningRef.current = listening;
  }, [listening]);

	useEffect(() => {
		// handle changes to keyboard input passed directly to the component.
		// this is used to inject data to the keypress buffer
		if (passThrough && passThrough.length > 0){
			for(let i = 0; i < passThrough.length; i++) {
				const fakeKeyPress = { key: passThrough[i], keyCode: passThrough[i].charCodeAt(0), altKey: false, ctrlKey: false, shiftKey: false, isFake: true };
				keyBufferRef.current.push(fakeKeyPress);
			}
		}
  }, [passThrough]);

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
			if (swallowKeyEvent 
					// dont swallow function keys
					&& !(e.keyCode >= 112 && e.keyCode <= 123)
					// dont swallow copy/paste
					&& !(e.ctrlKey && (e.key === "c" || e.key === "v" || e.key === "x"))
					&& !(e.shiftKey && (e.key === "Insert"))
					) {
				e.preventDefault();
				e.stopPropagation();
			}
			// special case, swallow CTRL-SHIFT-D which changes the inspector dock window position
			if (e.code === "KeyD" && e.shiftKey && e.ctrlKey) {
				e.preventDefault();
				e.stopPropagation();
				return;
			}
			keyBufferRef.current.push(e);
			// visual indicator of input received
			setTimeout(() => { 
				if (keyBufferRef.current.length > 5) {
					setIsReceiving(true);
		 		} else {
					setIsReceiving(false);
				}
			}, 500);
      scannerDebounced(e, keyBufferRef.current);
    } else {
			// dropped key, not listening
		}
		return e;
  };

  return (
    <div style={{ float: "right" }}>
      <Popup
        position="bottom right"
        hoverable
        content={
          <p>
						<Trans i18nKey="comp.barcodeScannerInput.supportsBarcodeScanning">
						This page supports barcode scanning. <Link to={helpUrl}>More Info</Link>
						</Trans>
          </p>
        }
        trigger={<Image src="/image/barcode.png" width={35} height={35} className={`barcode-support ${isReceiving ? "receiving" : ""}`} />}
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
  helpUrl: PropTypes.string,
	swallowKeyEvent: PropTypes.bool,
	/** keyboard passthrough, for passing data directly to component */
	passThrough: PropTypes.string,
	/** True to enable beep sound when an item is scanned */
	enableSound: PropTypes.bool
};

BarcodeScannerInput.defaultProps = {
  listening: true,
  minInputLength: 4,
  helpUrl: "/help/scanning",
	swallowKeyEvent: true,
	enableSound: true
};
