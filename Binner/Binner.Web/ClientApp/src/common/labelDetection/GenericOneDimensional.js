import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a 1D part label */
export default function GenericOneDimensional(value) {
  const execute = () => {
    // sometimes 1D labels contain an id before the actual value, but this is largely custom by manufacturer
    // 1PCC0402CRNPO9BN3R0\r
    console.debug(`Detect ${GenericOneDimensional.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: GenericOneDimensional.Name,
      reason: null
    };
    if (!value?.length > 0) return detectValue;
    const knownIdentifiers = ['1P', 'Q', 'K', 'P', 'J', '1T', '9D', '4L'];
    for (let i = 0; i < knownIdentifiers.length; i++) {
      let valueStr;
      let knownIdentifier = knownIdentifiers[i];
      if (value.startsWith(knownIdentifier)) {
        switch (knownIdentifier) {
          case '1P':
            // for Yageo reels, this is the part number
            if (value.length > 5) {
              valueStr = value.substring(2, value.length - 1);
              detectValue.success = true;
              detectValue.type = 'code128';
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

  return execute();
};
GenericOneDimensional.Name = 'Generic';
GenericOneDimensional.LabelType = BarcodeLabelTypes.Tokenized;