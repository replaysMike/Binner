import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";
import _ from "underscore";
import { format, parse } from "date-fns";

/** Detect a Walsin part label */
export default function Walsin(value) {
  const execute = () => {
    // fixed width, using ' ' character to fill empty space
    console.debug(`Detect ${Walsin.Name} data length=${value.length}`, value);
    let detectValue = handleQrCode();
    if (detectValue.success)
      return detectValue;
    detectValue = handleCode128(detectValue);
    return detectValue;
  }

  const handleQrCode = () => {
    // code: P1206B473K501CTQ2000L130I263678D181029\r
    console.debug(`Detect ${Walsin.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: {
        partNumber: null,
        quantity: null,
        lot: null,
        dateStr: null,
        date: null
      },
      vendor: Walsin.Name,
      reason: null,
    };
    if (value.length === 39) {
      // could be Walsin label, check the data to see if it looks right
      // qrcode is encoded as a simplistic 1D barcode ?!?
      
      const properties = ['P', 'Q', 'L', 'D'];
      // scan left to right. if we find a token, continue until we find another token or end of string
      let scanStr = '';
      const detectedValues = [];
      const cleanValue = value.replaceAll('\r', '');
      for (let i = 0; i < cleanValue.length; i++) {
        scanStr += cleanValue[i];
        for (let startPropertyCursor = 0; startPropertyCursor < properties.length; startPropertyCursor++) {
          const result = scanForProperty(cleanValue, i, scanStr, startPropertyCursor, properties);
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
        detectValue.type = 'qrcode';
        detectValue.labelType = 'part';
        const dateStr = _.find(detectedValues, i => i.section === 'D')?.value;
        detectValue.parsedValue = {
          partNumber: _.find(detectedValues, i => i.section === 'P')?.value,
          quantity: parseInt(_.find(detectedValues, i => i.section === 'Q')?.value ?? 0),
          lot: _.find(detectedValues, i => i.section === 'L')?.value,
          date: dateStr,
          dateFormatted: format(parse(dateStr, 'yyMMdd', new Date()), 'yy-MM-dd')
        };
        return detectValue;
      }

      return { ...detectValue, reason: `No values detected.` };
    }
    return { ...detectValue, reason: `Data length != 39 (${value.length})` };
  };

  const handleCode128 = (detectValue) => {
    if (value.length === 22) {
      // first 16 chars are the part number, with right padding ' '
      let partNumber = value.substring(0, 16).trimEnd();
      // next 5 are quantity, with left zero padding. Example: 003000
      let quantityStr = value.substring(16, 21);
      if (!isNumeric(quantityStr)) return { ...detectValue, reason: `Quantity '${quantityStr}' not a number` };
      let quantity = parseInt(quantityStr)

      // remainder is white space
      if (partNumber && quantity >= 0) {
        detectValue.success = true;
        detectValue.type = 'code128';
        detectValue.labelType = 'part';
        detectValue.parsedValue = {
          partNumber,
          quantity
        };
      }
      return detectValue;
    }
    return { ...detectValue, reason: `Too short (${value.length})` };
  }

  const scanForProperty = (value, i, scanStr, startPropertyCursor, properties) => {
    if (scanStr.startsWith(properties[startPropertyCursor])) {
      let startProperty = properties[startPropertyCursor];
      // found a token, keep going until we find another token or end of string
      for (let j = i + 2; j <= value.length; j++) {
        for (let endPropertyCursor = startPropertyCursor; endPropertyCursor < properties.length; endPropertyCursor++) {
          let section = value.substring(i + 1, j);
          if (section.endsWith(properties[endPropertyCursor]) || j >= value.length) {
            let endProperty = properties[endPropertyCursor];
            // found the ending token
            const sectionValue = value.substring(i + 1, j - endProperty.length + 1);
            const newI = j - endProperty.length - 1; // reset the ptr
            return { success: true, ptr: newI, section: startProperty, value: sectionValue }; // scan for new property
          }
        } // loop4 (end property cursor)
      } // loop 3 (cursor end)
    }
    return { success: false, ptr: i + 1, section: null, value: null };
  };

  return execute();
};
Walsin.Name = 'Walsin Technology Corporation';
Walsin.LabelType = BarcodeLabelTypes.Multiple;