import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";
import _ from "underscore";

/** Detect a 1D Panasonic part label */
export default function Panasonic(value) {
  const execute = () => {
    // sometimes 1D labels contain an id before the actual value, but this is largely custom by manufacturer
    // 1PCC0402CRNPO9BN3R0\r
    console.debug(`Detect ${Panasonic.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Panasonic.Name,
      reason: null
    };
    if (!value?.length > 0) return detectValue;

    const knownIdentifiers = ['3N'];
    for (let i = 0; i < knownIdentifiers.length; i++) {
      let valueStr;
      let knownIdentifier = knownIdentifiers[i];
      if (value.startsWith(knownIdentifier)) {
        switch (knownIdentifier) {
          case '3N':
            // for Panasonic reels, this is the number type (1st char) then part number (with spaces) and LAST value after a space is quantity
            // the number type and part number doesn't always have a space between them
            // example: 3N1ERJ-3EKF1502V 5000\r
            // example: 3N2 000F06N9F255 108010\r
            if (value.length > 5) {
              valueStr = value.substring(2, value.length - 1);
              const labelTypeStr = valueStr.substring(0, 1);
              if (!isNumeric(labelTypeStr)) return { ...detectValue, reason: `Label type '${labelTypeStr}' not a number` };
              const rest = valueStr.substring(1, valueStr.length);
              const sections = rest.split(' ');
              if (sections.length > 1) {
                const labelType = parseInt(labelTypeStr);
                const partNumber = getAllButLast(sections).trim();
                if (partNumber.length < 3) return { ...detectValue, reason: `Invalid part number '${partNumber}'` };
                detectValue.type = 'code128';
                detectValue.labelType = 'part';
                const quantityStr = sections[sections.length - 1];
                switch(labelType) {
                  case 1:
                    if (!isNumeric(quantityStr)) return { ...detectValue, reason: `Quantity '${quantityStr}' not a number` };
                    detectValue.success = true;
                    detectValue.parsedValue = {
                      partNumber: partNumber,
                      quantity: parseInt(quantityStr),
                      labelType: labelType,
                      barcodeType: 'part'
                    };
                    break;
                  case 2:
                    detectValue.success = true;
                    detectValue.parsedValue = {
                      serial: partNumber,
                      valueCode: quantityStr,
                      labelType: labelType,
                      barcodeType: 'serial'
                    };
                    break;
                  default:
                    detectValue.success = true;
                    detectValue.parsedValue = {
                      partNumber: partNumber,
                      quantity: parseInt(quantityStr),
                      labelType: labelType,
                      barcodeType: 'unknown'
                    };
                    break;
                }
                if (labelType === 1) {
                }
                return detectValue;
              } else {
                return { ...detectValue, reason: `Not enough sections '${sections.length}'` };
              }
            } else {
              return { ...detectValue, reason: `Value '${value}' too short` };
            }
            break;
          default:
            break;
        }
      }
    }

    return detectValue;
  };

  const getAllButLast = (arr, delimiter = ' ') => {
    const retArr = [];
    for(let i = 0; i < arr.length - 1; i++)
      retArr.push(arr[i]);
    return retArr.join(delimiter);
  };

  return execute();
};
Panasonic.Name = 'Panasonic Electronic Components';
Panasonic.LabelType = BarcodeLabelTypes.Tokenized;