import { isNumeric } from "./Utils";
import _ from "underscore";

export const detectLabel = (value) => {
  let detectValue;

  detectValue = detectFixedWidthLabels(value, ' ');
  
  if (!detectValue.success)
    detectValue = detectDelimitedLabels(value, ' ');
  if (!detectValue.success)
    detectValue = detectDelimitedLabels(value, '+');
  if (!detectValue.success)
    detectValue = detectDelimitedLabels(value, ':');
  if (!detectValue.success)
    detectValue = detectDelimitedLabels(value, '\r');
  if(!detectValue.success)
    detectValue = detectTokenizedLabels(value);

  if (!detectValue.success)
    detectValue = detect1DLabels(value);

  return detectValue;
};

export const detect1DLabels = (value) => {
  // sometimes 1D labels contain an id before the actual value, but this is largely custom by manufacturer
  // 1PCC0402CRNPO9BN3R0\r
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };
  if (!value?.length > 0) return detectValue;
  const knownIdentifiers = ['1P', 'Q', 'K', 'P', 'J', '1T', '9D', '4L'];
  for(let i = 0; i < knownIdentifiers.length; i++) {
    let valueStr;
    let knownIdentifier = knownIdentifiers[i];
    if (value.startsWith(knownIdentifier)) {
      console.log('starts with', knownIdentifier);
      switch(knownIdentifier) {
        case '1P':
          // for Yageo reels, this is the part number
          if (value.length > 5) {
            valueStr = value.substring(2, value.length - 1);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              partNumber: valueStr
            };
            return detectValue;
          }
          break;
        case 'K':
          if (value.length > 5) {
            valueStr = value.substring(1, value.length - 1);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              lineNumber: valueStr
            };
            return detectValue;
          }
          break;
        case 'Q':
          // value must be a number
          valueStr = value.substring(1, value.length - 1);
          if (isNumeric(valueStr)) {
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.9;
            detectValue.parsedValue = {
              quantity: parseInt(valueStr)
            };
          }
          return detectValue;
        case 'P':
          // for Yageo reels, this is the customer part number (not useful)
          if (value.length > 5) {
            valueStr = value.substring(1, value.length - 1);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              partNumber: valueStr
            };
            return detectValue;
          }
          break;
        case 'J':
          // a unique item number
          valueStr = value.substring(1, value.length - 1);
          detectValue.success = true;
          detectValue.type = 'code128';
          detectValue.vendor = 'Generic';
          detectValue.labelType = 'part';
          detectValue.certainty = 0.75;
          detectValue.parsedValue = {
            uniqueNumber: valueStr
          };
          return detectValue;
        case '1T':
          // lot number
          valueStr = value.substring(2, value.length - 1);
          detectValue.success = true;
          detectValue.type = 'code128';
          detectValue.vendor = 'Generic';
          detectValue.labelType = 'part';
          detectValue.certainty = 0.75;
          detectValue.parsedValue = {
            lot: valueStr,
          };
          return detectValue;
        case '9D':
          // date code
          if (value.length > 3 && value.length < 6) {
            valueStr = value.substring(2, value.length - 1);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.9;
            detectValue.parsedValue = {
              dateCode: valueStr,
            };
            return detectValue;
          }
          break;
        case '4L':
          // country of origin (COO)
          if (value.length > 3 && value.length < 6) {
            valueStr = value.substring(2, value.length - 1);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.vendor = 'Generic';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.9;
            detectValue.parsedValue = {
              country: valueStr,
            };
            return detectValue;
          }
          break;
        default:
          break;
      }
    }
  }

  // could be a raw quantity value
  if (isNumeric(value)) {
    const valueNumber = parseInt(value);
    if (valueNumber > 0 && valueNumber < 25000) {
      detectValue.success = true;
      detectValue.type = 'code128';
      detectValue.vendor = 'Generic';
      detectValue.labelType = 'part';
      detectValue.certainty = 0.5;
      detectValue.parsedValue = {
        quantity: valueNumber,
      };
      return detectValue;
    }
  }

  return detectValue;
};

export const detectFixedWidthLabels = (value, emptySpaceChar = ' ') => {
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };
  console.debug('detectFixedWidthLabels', emptySpaceChar, value);
  if (value.includes(emptySpaceChar) && emptySpaceChar === ' ') {
    detectValue = detectVishayLabel(value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'Vishay' };
  }

  return { ...detectValue, reason: `No label detected.` };
};

/** Detect generic part label */
export const detectDelimitedLabels = (value, delimiter = ' ') => {
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };
  console.debug('detectDelimitedLabel', delimiter, value);

  if (value.includes(delimiter) && delimiter === ' ') {
    const sections = value.replaceAll('\r', '').split(delimiter);
    detectValue = detectKemetLabel(sections, value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'Kemet' };
    else
      console.log('reason', detectValue.reason);
  }
  if (value.includes(delimiter) && delimiter === '+') {
    const sections = value.replaceAll('\r', '').split(delimiter);
    detectValue = detectBournsLabel(sections, value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'Bourns' };
    else
      console.log('reason', detectValue.reason);
  }
  if (value.includes(delimiter) && delimiter === ':') {
    const sections = value.replaceAll('\r', '').split(delimiter);
    detectValue = detectKoaPartLabel(sections, value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'KOA' };
    else
      console.log('reason', detectValue.reason);
    detectValue = detectKoaCustomerLabel(sections, value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'KOA' };
    else
      console.log('reason', detectValue.reason);
  }
  if (value.includes(delimiter) && delimiter === '\r') {
    const sections = value.split(delimiter);
    detectValue = detectYageoLabel(sections, value);
    if (detectValue.success)
      return { ...detectValue, vendor: 'Yageo' };
    else
      console.log('reason', detectValue.reason);
  }

  return { ...detectValue, reason: `No label detected.` };
}

/** Detect generic part label */
export const detectTokenizedLabels = (value) => {
  // Supplier: Yageo, private assembly label
  // code: UID19179844PID110561550TIM8/30/2013 3:16:06 PMSPL0Y033LIDSPNCC0402JRNPO9BN150QTY8992DCD1145MSD0COOCNESDROHC6O6SER19179844
  return detectTokenizedBarcode(value);
}

export const detectTokenizedBarcode = (value) => {
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  const properties = ['UID', 'PID', 'TIM', 'SPL', 'LID', 'SPN', 'QTY', 'DCD', 'MSD', 'COO', 'ESD', 'SER'];
  // scan left to right. if we find a token, continue until we find another token or end of string
  let scanStr = '';
  const detectedValues = [];
  for (let i = 0; i < value.length; i++) {
    scanStr += value[i];
    for (let startPropertyCursor = 0; startPropertyCursor < properties.length; startPropertyCursor++) {
      const result = scanForProperty(value, i, scanStr, startPropertyCursor, properties);
      if (result.success) {
        i = result.ptr; // reset the ptr
        detectedValues.push(result);
        scanStr = '';
        break;
      }
    } // loop 2 (start property cursor)
  } // loop 1 (cursor start)
  if (detectedValues.length > 0) {
    detectValue.success = true;
    detectValue.type = 'datamatrix';
    detectValue.vendor = 'Generic';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      pcn: _.find(detectedValues, i => i.section === 'UID')?.value,
      itemNumber: _.find(detectedValues, i => i.section === 'PID')?.value,
      receivedDate: _.find(detectedValues, i => i.section === 'TIM')?.value,
      supplier: _.find(detectedValues, i => i.section === 'SPL')?.value, // Supplier. Yageo = 0Y033 this may be specific to the label origin
      lotId: _.find(detectedValues, i => i.section === 'LID')?.value,
      partNumber: _.find(detectedValues, i => i.section === 'SPN')?.value,
      dateCode: _.find(detectedValues, i => i.section === 'DCD')?.value,
      quantity: parseInt(_.find(detectedValues, i => i.section === 'QTY')?.value ?? 0),
      msd: _.find(detectedValues, i => i.section === 'MSD')?.value,
      country: _.find(detectedValues, i => i.section === 'COO')?.value,
      esd: _.find(detectedValues, i => i.section === 'ESD')?.value,
      serial: _.find(detectedValues, i => i.section === 'SER')?.value,
    };
    return detectValue;
  }
  return { ...detectValue, reason: `No tokens detected.` };
};

const scanForProperty = (value, i, scanStr, startPropertyCursor, properties) => {
  if (scanStr.startsWith(properties[startPropertyCursor])) {
    let startProperty = properties[startPropertyCursor];
    // found a token, keep going until we find another token or end of string
    for (let j = i + 2; j < value.length; j++) {
      for (let endPropertyCursor = startPropertyCursor; endPropertyCursor < properties.length; endPropertyCursor++) {
        let section = value.substring(i + 1, j);
        if (section.endsWith(properties[endPropertyCursor])) {
          let endProperty = properties[endPropertyCursor];
          // found the ending token
          const sectionValue = value.substring(i + 1, j - endProperty.length);
          const newI = j - endProperty.length - 1; // reset the ptr
          return { success: true, ptr: newI, section: startProperty, value: sectionValue }; // scan for new property
        }
      } // loop4 (end property cursor)
    } // loop 3 (cursor end)
  }
  return { success: false, ptr: i + 1, section: null, value: null };
};

/** Detect a Kemet part label */
export const detectVishayLabel = (value, delimiter = ' ') => {
  // fixed width, using ' ' character to fill empty space
  console.debug('detectVishayLabel', value.length);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };

  if (value.length === 326) {
    // first 18 chars are the part number
    let partNumber = value.substring(0, 18).trimStart();
    // next 18 chars are lot1 (10) + date (8). Example: 000527030403/16/12 or R9002DL.0401/13/20
    let lot1 = value.substring(18, 28).trimStart();
    let lotDate = value.substring(28, 36);
    // next 10 chars are lot2. Example: 1920031A91
    let lot2 = value.substring(36, 46).trimStart();
    // next 53 chars are batch. Example: 202003PH19
    let batch = value.substring(46, 99).trimStart();

    // next 13 are quantity as a decimal. Example: 2500.000
    let quantityStr = value.substring(99, 112).trimStart();
    if (!isNumeric(quantityStr)) return { ...detectValue, reason: `Quantity '${quantityStr}' not a number` };
    let quantity = parseInt(quantityStr);

    // 14 after quantity is MFG PO
    let mfgPo = value.substring(112, 126).trimStart();
    // next 15 are unknown
    let unknown = value.substring(126, 141).trimStart();

    // next 4 chars are date code
    let dateCode = value.substring(141, 145).trimStart();
    // next 4 chars are date code 2
    let dateCode2 = value.substring(145, 149).trimStart();
    // next 4 are region. Example: 2410
    let regionStr = value.substring(149, 153).trimStart();
    if (!isNumeric(regionStr)) return { ...detectValue, reason: `Region '${regionStr}' not a number` };
    let region = parseInt(regionStr);

    // next 4 are SL. Example: 0010
    let sl = value.substring(153, 157).trimStart();
    // next 4 are ChkD. Example: A41 or 100
    let chkD = value.substring(157, 161).trimStart();
    // next 30 are operator. Example: SH903
    let operator = value.substring(161, 191).trimStart();

    // hazy details
    // lot1 30 chars
    let lot1Again = value.substring(191, 221).trimStart();
    // quantity 6 chars
    let quantityAgainStr = value.substring(221, 227).trimStart();
    let quantityAgain = parseInt(quantityAgainStr);
    // part number 45 chars
    let partNumberAgain = value.substring(227, 272).trimStart();
    // series 25 chars. Example: WSL-2010  0.0069 1  EA E3
    let alternateName = value.substring(272, 297).trimStart();
    // next 28 are serial. Example: 000000OMM400300970
    let serial = value.substring(297, 325).trimStart();
  
    detectValue.success = true;
    detectValue.type = 'pdf417';
    detectValue.vendor = 'Vishay';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      lot1,
      lot2,
      lotDate,
      batch,
      partNumber,
      quantity,
      mfgPo,
      unknown,
      dateCode,
      dateCode2,
      region,
      sl,
      chkD,
      operator,
      lot1Again,
      quantityAgain,
      partNumberAgain,
      alternateName,
      serial
    };
    return detectValue;
  }
  return { ...detectValue, reason: `Too short (${value.length})` };
};

/** Detect a Kemet part label */
export const detectKemetLabel = (sections, value) => {
  console.debug('detectKemetLabel', sections, value);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };
  if (sections.length === 6 || sections.length === 10) {
    // could be kemet label, check the data to see if it looks right
    let batch = 0;
    let batchStr = null; // batch number or ID, must be a number
    batchStr = sections[0];
    if (!isNumeric(batchStr)) return { ...detectValue, reason: `Batch '${batchStr}' not a number` };
    batch = parseInt(batchStr);
    let station = ''; // station which it was produced
    station = sections[1].substring(0, 2);
    let pkg = 0;
    let pkgStr = ''; // package number, must be a number
    pkgStr = sections[1].substring(2, 5);
    if (!isNumeric(pkgStr)) return { ...detectValue, reason: `Pkg '${pkgStr}' not a number` };
    pkg = parseInt(pkgStr);
    let partNumber = ''; // part number, alphanumeric
    if (sections[1].length - station.length - pkgStr.length > 0)
      partNumber = sections[1].substring(station.length + pkgStr.length, sections[1].length);
    let qty = 0;
    let country = null;
    if (sections.length > 5) {
      const qtyStr = sections[5].substring(0, 7); // must be a number
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      qty = parseInt(qtyStr);
      if (sections[5].length - qtyStr.length > 0)
        country = sections[5].substring(qtyStr.length, sections[5].length);
    }
    let countryCode = null;
    if (sections.length === 10)
      countryCode = sections[9];

    detectValue.success = true;
    detectValue.type = 'pdf417';
    detectValue.vendor = 'Kemet';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      batch,
      station,
      package: pkg,
      partNumber,
      quantity: qty,
      country,
      countryCode
    };
    return detectValue;
  }
  return { ...detectValue, reason: `Sections != 6 or 10 (${sections.length})` };
};

/** Detect a Kemet part label */
export const detectBournsLabel = (sections, value) => {
  // code: 042390059 B2014C1206C475K3PAC7800 0002000
  // code: 232802636 B1008C1206C103K1RAC7800    0004000MEXICO    MX
  console.debug('detectBournsLabel', sections, value);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  if (detected.sections.length === 4) {
    // could be kemet label, check the data to see if it looks right
    let partNumber = ''; // part number, alphanumeric
    if (sections[0].substring(0, 1) === 'P')
      partNumber = sections[0].substring(1, sections[0].length - 1);
    let qty = 0;
    if (sections[1].substring(0, 1) === 'Q') {
      const qtyStr = sections[1].substring(1, sections[1].length - 1); // must be a number
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      qty = parseInt(qtyStr);
    }
    let dc = ''; // date code, alphanumeric
    if (sections[2].substring(0, 1) === '9' && sections[2].substring(1, 2) === 'D') {
      dc = sections[2].substring(2, sections[2].length);
    }

    let lot = ''; // lot number, alphanumeric
    if (sections[3].substring(0, 1) === '1' && sections[3].substring(1, 2) === 'T') {
      lot = sections[3].substring(2, sections[3].length);
    }

    detectValue.success = true;
    detectValue.type = 'datamatrix';
    detectValue.vendor = 'Bourns';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      partNumber,
      quantity: qty,
      dateCode: dc,
      lot
    };
    return detectValue;
  }
  return { ...detectValue, reason: `Sections != 6 or 10 (${sections.length})` };
};

/** Detect a KOA part label */
export const detectKoaPartLabel = (sections, value) => {
  // code: KHK:1:RK73B1ETTP201J:10000:X:5018C394:623:KHK-A01
  console.debug('detectKoaPartLabel', sections, value);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null
  };
  if (sections.length === 8) {
    // could be koa label, check the data to see if it looks right
    let warehouse = ''; // part number, alphanumeric
    let unknown1 = 0;
    if (sections[0] === 'KHK') {
      warehouse = sections[0];
      let unknownStr = sections[1]; // must be a number
      if (!isNumeric(unknownStr)) return { ...detectValue, reason: `Section 1 not a number` };
      unknown1 = parseInt(unknownStr);
      let partNumber = ''; // partNumber, alphanumeric
      partNumber = sections[2];
      let qty = 0;
      const qtyStr = sections[3]; // must be a number
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      qty = parseInt(qtyStr);
      let warehouseLetter = sections[4]; // warehouse letter, alphanumeric
      let lot = sections[5]; // warehouse letter, alphanumeric
      let unknown2 = sections[6]; // warehouse letter, alphanumeric
      let warehouseId = sections[7]; // warehouse id, alphanumeric.
      console.log('success2!');
      detectValue.success = true;
      detectValue.type = 'pdf417';
      detectValue.vendor = 'KOA';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        warehouse,
        unknown1,
        partNumber,
        quantity: qty,
        warehouseLetter,
        lot,
        unknown2,
        warehouseId
      };
      return detectValue; 
    }
    return { ...detectValue, reason: `No start condition 'KHK'` };
  }
  return { ...detectValue, reason: `Sections != 8 (${sections.length})` };
};

/** Detect a KOA part label */
export const detectKoaCustomerLabel = (sections, value) => {
  // code: :RK73B1ETTP201J::14:X:5018C394:0001::::SOL-A25
  console.debug('detectKoaCustomerLabel', sections, value);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  if (sections.length === 11 || sections.length === 12) { // 12 when it has an extra empty with \r
    // could be koa label, check the data to see if it looks right
    let empty1 = sections[0];
    let warehouseLetter = sections[4];
    if (empty1.length === 0) {
      if (warehouseLetter === 'X') {
        let partNumber = sections[1]; // partNumber, alphanumeric
        let empty2 = sections[2]; // unknown
        let unknownNumber1Str = sections[3];
        if (!isNumeric(unknownNumber1Str)) return { ...detectValue, reason: `UnknownNumber '${unknownNumber1Str}' not a number` };
        let unknownNumber1 = parseInt(unknownNumber1Str); // unknown
        let traceCode = sections[5]; // trace code, alphanumeric
        let unknownNumber2Str = sections[6];
        if (!isNumeric(unknownNumber2Str)) return { ...detectValue, reason: `UnknownNumber2 '${unknownNumber2Str}' not a number` };
        let unknownNumber2 = parseInt(unknownNumber2Str); // unknown
        let supplierPartNumber = sections[7]; // partNumber, alphanumeric used when sections = 12

        let warehouseId = sections[10]; // some kind of warehouse id, alphanumeric

        detectValue.success = true;
        detectValue.type = 'pdf417';
        detectValue.vendor = 'KOA';
        detectValue.labelType = 'part';
        detectValue.parsedValue = {
          empty1,
          empty2,
          warehouseId,
          unknownNumber1,
          unknownNumber2,
          partNumber,
          supplierPartNumber,
          warehouseLetter,
          traceCode
        };
        return detectValue;
      }
      return { ...detectValue, reason: `Unexpected warehouse letter '${warehouseLetter}'` };
    }
    return { ...detectValue, reason: `First value should be empty '${empty1}'` };
  }
  console.log('made it here', sections.length);
  return { ...detectValue, reason: `Sections != 11 or 12 (${sections.length})` };
};

/** Detect a Yageo part label */
export const detectYageoLabel = (sections, value) => {
  // delimited AND prefixed
  // code: 31PCC0603KRX7R8BB392\r32P223891615631\r1T31E512061159F1056753\rQ0004000\r9D15511610\r26 505\r0002\r
  console.debug('detectKoaCustomerLabel', sections, value);
  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  if (sections.length === 8) {
    // could be yageo label, check the data to see if it looks right
    let partNumber;
    let internalCode;
    let internalCode2;
    let internalCode3;
    let internalCode4;
    let internalCode5;
    let internalCode6;
    let quantity = 0;
    if (sections[0].substring(0, 3) === '31P'){
      partNumber = sections[0].substring(3, sections[0].length);
    }
    if (sections[1].substring(0, 3) === '32P') {
      internalCode = sections[1].substring(3, sections[1].length);
    }
    if (sections[2].substring(0, 2) === '1T') {
      internalCode2 = sections[2].substring(2, sections[2].length);
    }
    if (sections[3].substring(0, 1) === 'Q') {
      let qtyStr = sections[3].substring(1, sections[3].length);
      if (isNumeric(qtyStr))
        quantity = parseInt(qtyStr);
    }
    if (sections[4].substring(0, 2) === '9D') {
      internalCode3 = sections[4].substring(2, sections[4].length);
    }
    if (sections[5].substring(0, 2) === '26') {
      internalCode4 = sections[6].substring(0, 2);
      internalCode5 = sections[5].substring(3, sections[5].length);
    }
    internalCode6 = sections[6].substring(0, sections[6].length);

    detectValue.success = true;
    detectValue.type = 'datamatrix';
    detectValue.vendor = 'Yageo';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      internalCode,
      internalCode2,
      internalCode3,
      internalCode4,
      internalCode5,
      internalCode6,
      partNumber,
      quantity,
    };
    return detectValue;
  }
  console.log('made it here', sections.length);
  return { ...detectValue, reason: `Sections != 8 (${sections.length})` };
};