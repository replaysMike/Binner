import { useState, useEffect, useMemo, useRef } from "react";
import { Trans } from 'react-i18next';
import { Link } from "react-router-dom";
import { Popup, Image } from "semantic-ui-react";
import { BarcodeProfiles } from "../common/Types";
import PropTypes from "prop-types";
import { dynamicDebouncer } from "../common/dynamicDebouncer";
import { AppEvents, Events } from "../common/events";
import { fetchApi } from '../common/fetchApi';
import { copyString } from "../common/Utils";
import { detectLabel } from "../common/labelDetection";
import { parse, format } from "date-fns";
import _ from "underscore";
const soundSuccess = new Audio('/audio/scan-success.mp3');
const soundFailure = new Audio('/audio/scan-failure.mp3');

// this value will be replaced by the Barcode config. Lower values might fail to detect scans
const DefaultDebounceIntervalMs = 80;
const DefaultIsDebug = false;
// lower values will falsely detect scans, higher may fail on short barcodes
const MinBufferLengthToAccept = 5;
const AbortBufferTimerMs = 2000;
// if any keystrokes have a delay between them greater than this value, the buffer will be dropped
const DefaultMaxKeystrokeThresholdMs = 300;
const MinKeystrokesToConsiderScanningEvent = 10;

/**
 * Handles generic barcode scanning input by listening for batches of key presses
 */
export function BarcodeScannerInput({ listening = true, minInputLength = MinBufferLengthToAccept, onReceived, helpUrl = "/help/scanning", swallowKeyEvent = true, passThrough, enableSound = true, config, onSetConfig, id, onDisabled, onReadStarted, onReadStopped }) {
  const [barcodeConfig, setBarcodeConfig] = useState(config || {
    enabled: true,
    isDebug: DefaultIsDebug,
    maxKeystrokeThresholdMs: DefaultMaxKeystrokeThresholdMs,
    bufferTime: DefaultDebounceIntervalMs,
    prefix2D: "[)>",
    profile: BarcodeProfiles.Default
  });
  const [isKeyboardListening, setIsKeyboardListening] = useState(listening || true);
  const [previousIsKeyboardListeningState, setPreviousIsKeyboardListeningState] = useState(listening || true);
  const [isReceiving, setIsReceiving] = useState(false);
  const isStartedReading = useRef(false);
  const sourceElementRef = useRef(null);
  const isReadingComplete = useRef(false);
  const timerRef = useRef(null);
  const listeningRef = useRef(isKeyboardListening);
  const keyBufferRef = useRef([]);
  const keyTimes = useRef([]);
  const lastKeyTime = useRef(0);
  const debugBuffer = [];

  const onReceivedBarcodeInput = (e, buffer) => {
    if (barcodeConfig.isDebug) {
      const mapped = debugBuffer.map((val) => ({
        key: val.key,
        keyCode: val.keyCode,
        code: val.code,
        altKey: val.altKey,
        shiftKey: val.shiftKey,
        type: val.type
      }));
      const keypressHistoryText = JSON.stringify(mapped);
      navigator.clipboard.writeText(keypressHistoryText);
      console.log('onReceivedBarcodeInput keypress history copied to clipboard'/*, debugBuffer*/);
    }
    if (buffer.length < minInputLength && processKeyBuffer(buffer, barcodeConfig.prefix2D.length) !== barcodeConfig.prefix2D) {
      keyBufferRef.current.length = 0;
      if (barcodeConfig.isDebug) console.debug('BSI: timeout: barcode dropped input', buffer);
      const maxTime = getMaxValueFast(keyTimes.current, 1);
      const sum = getSumFast(keyTimes.current, 1);
      if (barcodeConfig.isDebug) console.debug(`BSI: keytimes maxtime1 '${maxTime}' sum: ${sum}`, keyTimes.current);
      keyTimes.current = [];
      return; // drop and ignore input
    } else {
      // if keytimes has any times over a max threshold, drop input
      const maxTime = getMaxValueFast(keyTimes.current, 1);
      if (maxTime > barcodeConfig.maxKeystrokeThresholdMs) {
        if (barcodeConfig.isDebug) console.debug(`BSI: dropped buffer due to maxtime '${maxTime}'`, keyTimes.current);
        keyTimes.current = [];
        return; // drop and ignore input
      }
      if (barcodeConfig.isDebug) console.debug('BSI: accepted buffer', buffer.length);
    }

    const result = processKeyBuffer(buffer);
    // reset key buffer
    keyBufferRef.current.length = 0;

    if (result) {
      processStringInput(e, result);
      const maxTime = getMaxValueFast(keyTimes.current, 1);
      const sum = getSumFast(keyTimes.current, 1);
      if (barcodeConfig.isDebug)
        console.debug(`BSI: keytimes maxtime2 '${maxTime}' sum: ${sum}`, keyTimes.current);
      else
        console.debug(`BSI: Barcode event processed in ${sum}ms`);
      keyTimes.current = [];
  }
  };

  const processStringInput = (e, result) => {
    if (!result) return;
    const barcodeText = result.barcodeText;
    const text = result.text;
    // process raw value into an input object with decoded information
    if (barcodeText && barcodeText.length > 0) {
      const input = processBarcodeInformation(barcodeText);

      if (enableSound) {
        if (input.invalidBarcodeDetected)
          soundFailure.play();
        else
          soundSuccess.play();
      }
      isReadingComplete.current = true;

      // fire an mounted event handler that we received data
      onReceived(e, input);
      // fire a domain event
      AppEvents.sendEvent(Events.BarcodeReceived, { barcode: input, text: text }, id || "BarcodeScannerInput", sourceElementRef.current);
      if (onReadStopped) onReadStopped({ barcode: input, text: text }, id || "BarcodeScannerInput", sourceElementRef.current);
      sourceElementRef.current = null;
    } else {
      console.warn('BSI: no scan found, filtered.');
    }
    setIsReceiving(false);
  };

  /**
   * Process an array of key input objects into a string buffer
   * @param {array} buffer The array of Key input objects
   * @param {array} length If provided, will only process the length specified (useful for peeking at data)
   * @returns 
   */
  const processKeyBuffer = (buffer, length = 99999) => {
    let str = "";
    let noControlCodesStr = "";
    let specialCharBuffer = [];
    let modifierKeyCount = 0;
    for (let i = 0; i < Math.min(buffer.length, length); i++) {
      let key = buffer[i];
      if (key.altKey || key.shiftKey || key.ctrlKey)
        modifierKeyCount++;

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
        if (!key.altKey && !key.ctrlKey && key.keyCode !== 13 && key.keyCode !== 10 && key.keyCode !== 9)
          noControlCodesStr += char;
      }
    }

    if (buffer.length === modifierKeyCount) {
      return null;
    }
    return { barcodeText: str, text: noControlCodesStr };
  };

  const processBarcodeInformation = (value) => {
    let barcodeType = "code128";
    let labelType = "Unknown";
    let parsedValue = {};
    let correctedValue = value;
    let gsDetected = false;
    let rsDetected = false;
    let eotDetected = false;
    let invalidBarcodeDetected = false;
    let vendor = "Unknown";

    if (value.startsWith(barcodeConfig.prefix2D)) {
      // 2D DotMatrix barcode. Process into value.
      barcodeType = "datamatrix";
      const parseResult = parseDataMatrix(value);
      vendor = parseResult.vendor;
      parsedValue = parseResult.value;
      gsDetected = parseResult.gsDetected;
      rsDetected = parseResult.rsDetected;
      eotDetected = parseResult.eotDetected;
      invalidBarcodeDetected = parseResult.invalidBarcodeDetected;
      correctedValue = parseResult.correctedValue;
      labelType = parseResult.labelType;
    } else {
      // possible 1D barcode
      console.debug('possible 1D or 2D code...');
      parsedValue = value.replace("\n", "").replace("\r", "");
      // try to detect if we know what kind of label it is
      let detectedValues = { success: false };
      console.debug('try detect barcode...');
      detectedValues = tryDetectBarcode(value);
      console.debug('detect result', detectedValues);

      if(detectedValues.success) {
        if (detectedValues.type)
          barcodeType = detectedValues.type;
        if (detectedValues.labelType)
          labelType = detectedValues.labelType;
        vendor = detectedValues.vendor;
        parsedValue = detectedValues.parsedValue;
      } else {
        // try another type
      }
    }

    return {
      type: barcodeType,
      labelType: labelType,
      vendor: vendor,
      value: parsedValue,
      correctedValue: correctedValue,
      rawValue: value,
      length: value?.length || 0,
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
    const header = barcodeConfig.prefix2D;
    const expectedFormatNumber = 6; /** digikey barcode */
    const expectedBinnerFormatNumber = 9; /** binner barcode */
    let labelType = "Unknown";
    let vendor = "Unknown";
    const controlChars = [
      // binner specific labels
      'BS', 'BN', 'BL', 'B1', 'B2', 'BV',
      // digikey specific labels
      'P', '1P', '30P', 'P1', '1K', '10K', '11K', '4L', 'Q', '11Z', '12Z', '13Z', '20Z', '9D', '1T', '20Z', '16D',
      // non-digikey labels
      'K', '4K', '2Q', '3Q', '16K', '42P', '17D', '11D', '10L', '13K', '2E', '11N', '20T', '10V', '14D', '16D', '6D', '31P', 'V', '3S', 'T', '31T', 'D', '20L', '21L', '22L', '23L', '2P', '4W', 'E', '3Z', 'L', 'S', '6P',
    ];

    let gsCodePresent = false;
    let rsCodePresent = false;
    let eotCodePresent = false;
    let formatNumber = "";
    let buffer = "";
    let i;
    let formatNumberIndex = 0;
    let correctedValue = value.toString();
    // normalize the control codes so we don't have multiple values to worry about
    console.debug('originalValue', correctedValue);
    correctedValue = normalizeControlCharacters(correctedValue);
    console.debug('correctedValue', correctedValue);

    gsCodePresent = gsCharCodes.some(v => correctedValue.includes(v));
    rsCodePresent = rsCharCodes.some(v => correctedValue.includes(v));
    eotCodePresent = eotCharCodes.some(v => correctedValue.includes(v));
    console.debug('presence', gsCodePresent, rsCodePresent, eotCodePresent);
    //console.debug('codePresent', gsCodePresent, rsCodePresent, eotCodePresent);

    // read in the format number first. For Digikey 2d barcodes, this should be 6 (expectedFormatNumber) for DigiKey, 9 (expectedBinnerFormatNumber) for Binner
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
    const expectedFormatNumbers = [ expectedFormatNumber, expectedBinnerFormatNumber ];
    let hasNoFormatNumber = false;
    if (!expectedFormatNumbers.includes(formatNumber)) {
      // error
      console.error(`BSI: Expected the 2D barcode format number of any [${expectedFormatNumbers.join()}]  but was ${formatNumber}`);
      //return {};
      hasNoFormatNumber = true;
    }
    const isBinnerBarcode = formatNumber === expectedBinnerFormatNumber;
    const isDigiKeyBarcode = formatNumber === expectedFormatNumber;
    
    const wurthVendorName = "Würth_Elektronik";
    const taiyoYudenVendorName = "Taiyo Yuden";
    const texasInstrumentsVendorName = "Texas Instruments";
    const nxpVendorName = "NXP Semiconductors";
    const kyoceraVendorName = "Kyocera AVX";
    const murataVendorName = "Murata";
    if (isBinnerBarcode) vendor = "Binner";
    if (isDigiKeyBarcode) vendor = "DigiKey";
    if (hasNoFormatNumber) {
      vendor = "Other";
      // seen on Susumu, and some german suppliers
      gsCharCodes.push('@');
    }

    let lastPosition = i - 1;
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
    console.log('gsLines', gsLines);

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
        // skip CR and LF characters completely
        if (lfCharCodes.includes(line[c]) || crCharCodes.includes(line[c])) continue;

        if (readControlChars) readCommandType += line[c];
        else readValue += line[c];

        if (readControlChars === header || readControlChars === formatNumber) readValue = "";
        if (controlChars.includes(readCommandType)) {
          // start reading value
          readControlChars = false;
        }
      }

      /** NOTE: supported commands below must be present in the controlChars array */
      // https://static.spiceworks.com/attachments/post/0016/2204/data_dictionary.pdf
      switch (readCommandType) {
        case "BS":
          // Binner short id
          if (isBinnerBarcode)
            parsedValue["shortId"] = readValue;
          labelType = "part";
          break;
        case "BN":
          // Binner part number
          if (isBinnerBarcode)
            parsedValue["partNumber"] = readValue;
          labelType = "part";
          break;
        case "BL":
          // Binner location
          if (isBinnerBarcode)
            parsedValue["location"] = readValue;
          break;
        case "B1":
          // Binner bin number 1
          if (isBinnerBarcode)
            parsedValue["binNumber1"] = readValue;
          break;
        case "B2":
          // Binner bin number 2
          if (isBinnerBarcode)
            parsedValue["binNumber2"] = readValue;
          break;
        case "BV":
          // Binner label version
          if (isBinnerBarcode)
            parsedValue["version"] = readValue;
          labelType = "part";
          break;
        case "P":
          // could be DigiKey part number, or customer reference value
          parsedValue["description"] = readValue;
          break;
        case "30P":
          // DigiKey part number
          parsedValue["supplierPartNumber"] = readValue;
          labelType = "part";
          break;
        case "1P":
          // manufacturer part number
          parsedValue["mfgPartNumber"] = readValue;
          break;
        case "6P":
          // internal manufacturer part number (Murata)
          parsedValue["internalMfgPartNumber"] = readValue;
          break;
        case "1K":
          // Salesorder#
          parsedValue["salesOrder"] = readValue;
          labelType = "order";
          break;
        case "10K":
          // invoice#
          parsedValue["invoice"] = readValue;
          labelType = "order";
          break;
        case "11K":
          // delivery note, seen on Texas Instruments
          parsedValue["deliveryNote"] = readValue;
          break;
        case "4L":
          // country of origin
          parsedValue["countryOfOrigin"] = readValue;
          break;
        case "10D": // Murata
        case "9D": // Murata
        case "D": // Texas Instruments
          // date code
          parsedValue["dateCode"] = readValue;
          break;
        case "1T":
          // lot code
          parsedValue["lotCode"] = readValue;
          labelType = "part";
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
          labelType = "part";
          break;
        case "12Z":
          // internal id of some kind
          parsedValue["mid"] = readValue;
          labelType = "part";
          break;
        case "13Z":
          // shipment load id
          parsedValue["loadId"] = readValue;
          labelType = "part";
          break;
        case "20Z":
          // reserved
          parsedValue["reserved"] = readValue;
          break;
        
        // non-DigiKey label values
        case "K":
          // order number
          parsedValue["orderNumber"] = readValue;
          break;
        case "4K":
          // order number
          parsedValue["orderItem"] = readValue;
          break;
        case "2Q":
          // quantity other
          parsedValue["quantityOther"] = readValue;
          break;
        case "3Q":
          // quantity units (PCS)
          parsedValue["quantityUnits"] = readValue;
          break;
        case "16K":
          // delivery number
          parsedValue["deliveryNo"] = readValue;
          break;
        case "42P":
          // manufacturer
          parsedValue["manufacturer"] = readValue;
          break;
        case "17D":
        case "11D": // seen on Taiyo Yuden
          // manufacture date
          parsedValue["manufactureDate"] = readValue;
          break;
        case "10L":
        case "10V":
          // manufacturer country
          parsedValue["manufacturerCountry"] = readValue;
          break;
        case "13K":
          // tariffNo
          parsedValue["tariffNo"] = readValue;
          break;
        case "2E":
          // ROHS classification code
          parsedValue["rohsCode"] = readValue;
          break;
        case "11N":
          // UL listed
          parsedValue["ulListed"] = readValue;
          break;
        case "20T":
          // moisture level
          parsedValue["moistureLevel"] = readValue;
          break;
        case "14D":
          // expire date
          parsedValue["expireDate"] = readValue;
          break;
        case "16D":
          // formatted date code or manufactureDate
          try {
            parsedValue["manufactureDate"] = format(parse(readValue, 'yyyyMMdd', new Date()), 'yyyy-MM-dd');
          } catch (err) {
            parsedValue["manufactureDate"] = readValue;
          }
          break;
        case "6D":
          // date code
          parsedValue["dateCode"] = readValue;
          break;
        case "31P":
          // order code
          parsedValue["orderCode"] = readValue;
          break;
        case "V":
          // supplier id, seen on Texas Instruments
          parsedValue["supplierId"] = readValue;
          break;
        case "3S":
          // package id
          parsedValue["packageId"] = readValue;
          break;
        case "T":
          // batch number, seen on Taiyo Yuden
          parsedValue["batchNo"] = readValue;
          break;
        case "31T":
          // lot number, seen on Texas Instruments
          parsedValue["lotNumber"] = readValue;
          break;
        case "4W":
          // turnkey ('TKY'="Full Turnkey processing",'NTY'="Non-Turnkey",'SWR'="Special Work Request"), seen on Texas Instruments
          parsedValue["turnkey"] = readValue;
          break;
        case "2P":
          // revision, seen on Texas Instruments
          parsedValue["revision"] = readValue;
          break;
        case "20L":
          // Chip Source origin (CSO) 'MH8', seen on Texas Instruments
          // specific fabrication facility location
          parsedValue["chipSourceOrigin"] = readValue;
          break;
        case "21L":
          // Chip country of origin (CCO) 'JPN', seen on Texas Instruments
          // country of the fabrication facility
          parsedValue["chipCountryOrigin"] = readValue;
          break;
        case "22L":
          // Assembly source origin (ASO) 'QAB', seen on Texas Instruments
          // specific assembly factory
          parsedValue["assemblySourceOrigin"] = readValue;
          break;
        case "23L":
          // Assembly country of origin (ACO) 'PHL', seen on Texas Instruments
          // country the assembly factory is in
          parsedValue["assemblyCountryOrigin"] = readValue;
          break;
        case "E":
          // ROHS clasification code (4), seen on Texas Instruments
          parsedValue["rohsCode"] = readValue;
          break;
        case "3Z":
          // Expiry date code, delimited (2/260C/1YEAR;//;022319), seen on Texas Instruments
          parsedValue["expireDate"] = readValue;
          break;
        case "L":
          // destination warehouse, (1518), seen on Texas Instruments
          parsedValue["destinationWarehouse"] = readValue;
          break;
        case "S":
          // material, (1518), seen on Kyocera
          parsedValue["materialId"] = readValue;
          break;
        default:
          break;
      }
    }

    // special case for NXP labels - they are encoded the same as DigiKey but use different short codes
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.mfgPartNumber?.length > 0 && parsedValue.quantity >= 0 && parsedValue.lotCode?.length > 0 && parsedValue.manufactureDate?.length > 0 && parsedValue.materialId?.length > 0 && parsedValue.expireDate?.length > 0 && parsedValue.manufactureDate?.length > 0) {
      vendor = kyoceraVendorName;
      parsedValue["partNumber"] = parsedValue.mfgPartNumber;
    }

    // special case for Murata labels - they are encoded the same as DigiKey but use MORE information
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.mfgPartNumber?.length > 0 && 'internalMfgPartNumber' in parsedValue && parsedValue.quantity >= 0 && parsedValue.lotCode?.length > 0 && parsedValue.dateCode?.length > 0 && parsedValue.countryOfOrigin?.length > 0) {
      vendor = murataVendorName;
      parsedValue["partNumber"] = parsedValue.mfgPartNumber;
    }

    // special case for Wurth labels - they are encoded the same as DigiKey but use less information
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.mfgPartNumber?.length > 0 && parsedValue.quantity >= 0 && parsedValue.lotCode?.length > 0 && parsedValue.manufactureDate?.length>0) {
      vendor = wurthVendorName;
      parsedValue["partNumber"] = parsedValue.mfgPartNumber;
    }

    // special case for Taiyio Yuden labels - they are encoded the same as DigiKey but use less information
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.description?.length && parsedValue.manufactureDate?.length > 0 && parsedValue.quantity >= 0 && parsedValue.batchNo?.length > 0 && parsedValue.countryOfOrigin?.length > 0) {
      vendor = taiyoYudenVendorName;
      parsedValue["partNumber"] = parsedValue.description;
    }

    // special case for Texas Instruments labels - they are encoded the same as DigiKey but use different short codes
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.mfgPartNumber?.length && parsedValue.lotCode?.length > 0 && parsedValue.quantity >= 0 && parsedValue.tky?.length > 0 && parsedValue.cco?.length > 0 && parsedValue.aco?.length > 0 && parsedValue.supplierId?.length > 0) {
      //https://www.ersaelectronics.com/blog/a-360-degree-view-of-ti-labels
      vendor = texasInstrumentsVendorName;
      parsedValue["partNumber"] = parsedValue.mfgPartNumber;
      parsedValue["description"] = parsedValue.mfgPartNumber;
    }

    // special case for NXP labels - they are encoded the same as DigiKey but use different short codes
    if (isDigiKeyBarcode && !parsedValue.partNumber?.length && parsedValue.mfgPartNumber?.length && parsedValue.lotCode?.length > 0 && parsedValue.quantity >= 0 && parsedValue.dateCode?.length > 0 && parsedValue.orderCode?.length > 0 && parsedValue.lotNumber?.length > 0 && parsedValue.supplierPartNumber?.length > 0) {
      //https://www.ersaelectronics.com/blog/a-360-degree-view-of-ti-labels
      vendor = nxpVendorName;
      parsedValue["partNumber"] = parsedValue.supplierPartNumber;
      parsedValue["description"] = parsedValue.supplierPartNumber;
    }

    const useFormatNumber = isBinnerBarcode ? expectedBinnerFormatNumber : expectedFormatNumber;
    correctedValue = buildBarcode(useFormatNumber, gsLines);
    return {
      rawValue: value,
      vendor: vendor,
      value: parsedValue,
      correctedValue: correctedValue,
      gsDetected: gsCodePresent,
      rsDetected: rsCodePresent,
      eotDetected: eotCodePresent,
      gsLines: gsLines,
      invalidBarcodeDetected,
      labelType
    };
  };

  const buildBarcode = (formatNumber, gsLines) => {
    let barcode = `${barcodeConfig.prefix2D}\u241e${formatNumber.toString().padStart(2, '0')}`; // Header + RS + formatNumber
    for (let i = 0; i < gsLines.length; i++) {
      barcode = barcode + "\u241d" + gsLines[i]; // GS
    }
    barcode = barcode + "\u2404\r"; // EOT + CR
    return barcode;
  };

  const tryDetectBarcode = (value) => {
    /*let detected = {
      type: 'code128',
      labelType: 'Unknown',
      success: false,
      containsSpaces: false,
      containsPlus: false,
      spacesCount: 0,
      sections: [],
      length: value.length,
      vendor: '',
      parsedValue: {},
    };*/
    // does it contain spaces?
    let detected = detectLabel(value);
    return detected;
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
    for (let i = 0; i < gsLines.length; i++) {
      if (gsLines[i].includes("\u241e")) { // RS
        return true;
      }
    }
    return false;
  };

  const fixInvalidBarcode = (gsLines) => {
    const newGsLines = [];
    for (let i = 0; i < gsLines.length; i++) {
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

  // create a debouncer, but with the ability to update it's interval as needed
  const scannerDebounced = useMemo(() => dynamicDebouncer(onReceivedBarcodeInput, () => BarcodeScannerInput.debounceIntervalMs), []);

  const disableBarcodeInput = (e) => {
    if (barcodeConfig.isDebug) console.debug('BSI: disabled barcode input on request');
    setPreviousIsKeyboardListeningState(isKeyboardListening);
    setIsKeyboardListening(false);
    removeKeyboardHandler();
  };

  const restoreBarcodeInput = (e) => {
    if (barcodeConfig.isDebug) console.debug('BSI: enabled barcode input on request');
    setIsKeyboardListening(previousIsKeyboardListeningState);
    addKeyboardHandler();
  };

  useEffect(() => {
    const enableListening = () => {
      // start listening for all key presses on page
      addKeyboardHandler();
      // add event listeners to receive requests to disable/enable barcode capture
      document.body.addEventListener(Events.DisableBarcodeInput, disableBarcodeInput);
      document.body.addEventListener(Events.RestoreBarcodeInput, restoreBarcodeInput);
      document.body.addEventListener(Events.BarcodeInput, (event) => processStringInput(event, { barcodeText: event.detail, text: event.detail }));
    };

    if (!config) {
      fetchApi("/api/system/settings").then((response) => {
        const { data } = response;
        const barcodeConfig = data.barcode;
        setBarcodeConfig(barcodeConfig);
        if (onSetConfig)
          onSetConfig(barcodeConfig);

        // update the static debounce interval
        BarcodeScannerInput.debounceIntervalMs = parseInt(barcodeConfig.bufferTime);
        if (barcodeConfig.enabled) enableListening();
        else if (onDisabled) onDisabled();
      });
    } else {
      setBarcodeConfig(config);
      if (config.enabled) enableListening();
      else if (onDisabled) onDisabled();
    }
    return () => {
      // stop listening for key presses
      removeKeyboardHandler();
      // remove event listeners
      document.body.removeEventListener(Events.DisableBarcodeInput, disableBarcodeInput);
      document.body.removeEventListener(Events.RestoreBarcodeInput, restoreBarcodeInput);
    };
  }, []);

  useEffect(() => {
    if (config) {
      setBarcodeConfig({ ...config });
      // update the static debounce interval
      BarcodeScannerInput.debounceIntervalMs = parseInt(config.bufferTime);
    }
  }, [config])

  useEffect(() => {
    // handle changes to the incoming listening prop
    setIsKeyboardListening(listening);
    listeningRef.current = listening;
  }, [listening]);

  useEffect(() => {
    // handle changes to keyboard input passed directly to the component.
    // this is used to inject data to the keypress buffer
    if (passThrough && passThrough.length > 0) {
      for (let i = 0; i < passThrough.length; i++) {
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
    if (barcodeConfig.isDebug) debugBuffer.push(e);
    //console.log('kd', e, listeningRef.current, isStartedReading.current);
    if (listeningRef.current === true) {
      // listening for keypresses

      // swallowing certain keypresses when listening
      if (swallowKeyEvent
        // dont swallow function keys
        && !(e.keyCode >= 112 && e.keyCode <= 123)
        // dont swallow copy/paste
        && !(e.ctrlKey && (e.key === "c" || e.key === "v" || e.key === "x"))
        && !(e.shiftKey && (e.key === "Insert"))
      ) {
        if (barcodeConfig.isDebug) console.debug("swallowed", e.keyCode);
        e.preventDefault();
        e.stopPropagation();
      }
      // special case, swallow CTRL-SHIFT-D which changes the inspector dock window position
      if (e.code === "KeyD" && e.shiftKey && e.ctrlKey) {
        e.preventDefault();
        e.stopPropagation();
        return;
      }
      // when a barcode has started, prevent propagation of all keypresses
      if (isStartedReading.current === true) {
        e.preventDefault();
        e.stopPropagation();
      }

      keyBufferRef.current.push(e);

      const maxTime = getMaxValueFast(keyTimes.current, 1);
      if (keyBufferRef.current.length > MinKeystrokesToConsiderScanningEvent && maxTime < barcodeConfig.maxKeystrokeThresholdMs) {
        //console.log('BSI: Detected start of barcode scan', isStartedReading.current, keyBufferRef.current.length, MinKeystrokesToConsiderScanningEvent, barcodeConfig.maxKeystrokeThresholdMs, maxTime);
        if (allValuesAreEqual(keyBufferRef.current)) {
          // if a user holds down a key on the keyboard, we don't want to detect as a barcode event
          return;
        }
        setIsReceiving(true);
        isReadingComplete.current = false;
        // only send the event once when we've determined we are capturing
        if (!isStartedReading.current) {
          //console.log('BSI: Starting event', keyBufferRef.current.length, maxTime);
          sourceElementRef.current = document.activeElement;
          AppEvents.sendEvent(Events.BarcodeReading, keyBufferRef.current, id || "BarcodeScannerInput", sourceElementRef.current);
          if (onReadStarted) onReadStarted(keyBufferRef.current, id || "BarcodeScannerInput", sourceElementRef.current);
        }
        isStartedReading.current = true;
      }

      // visual indicator of input received
      if (timerRef.current) clearTimeout(timerRef.current);
      timerRef.current = setTimeout(() => {
        // barcode scan stopped
        setIsReceiving(false);
        if (isStartedReading.current && !isReadingComplete.current) {
          AppEvents.sendEvent(Events.BarcodeReadingCancelled, keyBufferRef.current, id || "BarcodeScannerInput", sourceElementRef.current);
          if (onReadStopped) onReadStopped(keyBufferRef.current, id || "BarcodeScannerInput", sourceElementRef.current);
          sourceElementRef.current = null;
        }
        isStartedReading.current = false;
      }, AbortBufferTimerMs);

      keyTimes.current.push(new Date().getTime() - lastKeyTime.current);
      lastKeyTime.current = new Date().getTime();
      scannerDebounced(e, keyBufferRef.current);
    } else {
      // dropped key, not listening
      if (barcodeConfig.isDebug) console.debug('BSI: input ignored, not listening');
    }
    return e;
  };

  // helpers

  const allValuesAreEqual = (arr) => {
    if (arr.length < 2) return false;
    let lastVal = arr[0].keyCode;
    for (let i = 1; i < arr.length; i++) {
      if (arr[i].keyCode !== lastVal) return false;
      lastVal = arr[i].keyCode;
    }
    return true;
  };

  const getMaxValueFast = (arr, startAt = 0) => {
    // fastest performing solution of getting the max value in an array
    if (startAt >= arr.length) return arr.length > 0 ? arr[0] : -1;
    let max = arr[startAt];
    for (let i = startAt + 1; i < arr.length; ++i) {
      if (arr[i] > max) {
        max = arr[i];
      }
    }
    return max;
  }

  const getSumFast = (arr, startAt = 0) => {
    // fastest performing solution of getting the sum
    if (startAt >= arr.length) return arr.length > 0 ? arr[0] : -1;
    let sum = arr[startAt];
    for (let i = startAt + 1; i < arr.length; ++i) {
      sum += arr[i];
    }
    return sum;
  }

  if (!barcodeConfig.enabled)
    return (<></>);

  //renderCount.current = renderCount.current + 1;
  //if (IsDebug) console.debug('render', renderCount.current);

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
  /** help url when clicking on the scanner icon */
  helpUrl: PropTypes.string,
  /** true to swallow key events */
  swallowKeyEvent: PropTypes.bool,
  /** keyboard passthrough, for passing data directly to component */
  passThrough: PropTypes.string,
  /** True to enable beep sound when an item is scanned */
  enableSound: PropTypes.bool,
  /** Set the barcode config */
  config: PropTypes.object,
  /** Fired when the configuration is updated */
  onSetConfig: PropTypes.func,
  /** Fired when barcode support is disabled */
  onDisabled: PropTypes.func,
  /** Fired when the barcode reading has started */
  onReadStarted: PropTypes.func,
  /** Fired when the barcode reading has stopped */
  onReadStopped: PropTypes.func,
};

// store the debounce interval statically, so it can be modified and used by a memoized debounce function
BarcodeScannerInput.debounceIntervalMs = DefaultDebounceIntervalMs;
