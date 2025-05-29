import { isNumeric } from "./Utils";

/** Detect a Kemet part label */
export const detectKemetLabel = (detected, value) => {
  console.debug('detectKemetLabel', detected, value);
  const detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  if (detected.sections.length === 6 || detected.sections.length === 10) {
    // could be kemet label, check the data to see if it looks right
    let batch = 0;
    let batchStr = null; // batch number or ID, must be a number
    batchStr = detected.sections[0];
    if (!isNumeric(batchStr)) return { ...detectValue, reason: `Batch '${batchStr}' not a number` };
    batch = parseInt(batchStr);
    let station = ''; // station which it was produced
    station = detected.sections[1].substring(0, 2);
    let pkg = 0;
    let pkgStr = ''; // package number, must be a number
    pkgStr = detected.sections[1].substring(2, 5);
    if (!isNumeric(pkgStr)) return { ...detectValue, reason: `Pkg '${pkgStr}' not a number` };
    pkg = parseInt(pkgStr);
    let partNumber = ''; // part number, alphanumeric
    if (detected.sections[1].length - station.length - pkgStr.length > 0)
      partNumber = detected.sections[1].substring(station.length + pkgStr.length, detected.sections[1].length);
    let qty = 0;
    let country = null;
    if (detected.sections.length > 5) {
      const qtyStr = detected.sections[5].substring(0, 7); // must be a number
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      qty = parseInt(qtyStr);
      if (detected.sections[5].length - qtyStr.length > 0)
        country = detected.sections[5].substring(qtyStr.length, detected.sections[5].length);
    }
    let countryCode = null;
    if (detected.sections.length === 10)
      countryCode = detected.sections[9];

    detectValue.success = true;
    detectValue.type = 'pdf417';
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
  return { ...detectValue, reason: `Sections != 6 or 10 (${detected.sections.length})` };
};

/** Detect a Kemet part label */
export const detectBournsLabel = (detected, value) => {
  console.debug('detectBournsLabel', detected, value);
  const detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    reason: null,
  };
  if (detected.sections.length === 4) {
    // could be kemet label, check the data to see if it looks right
    let partNumber = ''; // part number, alphanumeric
    if (detected.sections[0].substring(0, 1) === 'P')
      partNumber = detected.sections[0].substring(1, detected.sections[0].length - 1);
    let qty = 0;
    if (detected.sections[1].substring(0, 1) === 'Q') {
      const qtyStr = detected.sections[1].substring(1, detected.sections[1].length - 1); // must be a number
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      qty = parseInt(qtyStr);
    }
    let dc = ''; // date code, alphanumeric
    if (detected.sections[2].substring(0, 1) === '9' && detected.sections[2].substring(1, 2) === 'D') {
      dc = detected.sections[2].substring(2, detected.sections[2].length);
    }

    let lot = ''; // lot number, alphanumeric
    console.log('test', detected.sections[3].substring(0, 1), detected.sections[3].substring(1, 2));
    if (detected.sections[3].substring(0, 1) === '1' && detected.sections[3].substring(1, 2) === 'T') {
      lot = detected.sections[3].substring(2, detected.sections[3].length);
    }

    detectValue.success = true;
    detectValue.type = 'datamatrix';
    detectValue.labelType = 'part';
    detectValue.parsedValue = {
      partNumber,
      quantity: qty,
      dateCode: dc,
      lot
    };
    return detectValue;
  }
  return { ...detectValue, reason: `Sections != 6 or 10 (${detected.sections.length})` };
};