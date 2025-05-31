import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";
import Samsung from "./Samsung";

/** Detect a 1D part label */
// https://static.spiceworks.com/attachments/post/0016/2204/data_dictionary.pdf
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
    const knownIdentifiers = ['1P', '30P', '31P', '32P', '42P', '50P', '51P', 'T', 'Q', '2Q', '3Q', 'K', '4K', '13K', '16K', 'P', 'J', '1T', '9D', '16D', '17D', 'D', '4L', 'L', '10L', 'V', 'R'];
    for (let i = 0; i < knownIdentifiers.length; i++) {
      let cleanValue = value.replaceAll('\r', '');
      let valueStr;
      let knownIdentifier = knownIdentifiers[i];
      if (cleanValue.startsWith(knownIdentifier)) {
        switch (knownIdentifier) {
          case '1P':
            // for Yageo reels, this is the part number
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(2, cleanValue.length);
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
          case '30P':
          case '31P':
            // for random reels, this is the part number
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(3, cleanValue.length);
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
          case '32P':
            // for random reels, this is the internal part number
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(3, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                internalPartNumber: valueStr
              };
              return detectValue;
            }
            break;
          case '42P':
            // for random reels, this is the manufacturer
            if (cleanValue.length > 2) {
              valueStr = cleanValue.substring(3, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                manufacturer: valueStr
              };
              return detectValue;
            }
            break;
          case '50P':
          case '51P':
            // manufacturer part number
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(3, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.9;
              detectValue.parsedValue = {
                manufacturerPartNumber: valueStr
              };
              return detectValue;
            }
            break;
          case 'T':
            // for some Samsung reels, this is the part number
            if (cleanValue.length > 5) {
              valueStr = value.substring(1, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.vendor = Samsung.Name;
              detectValue.certainty = 0.9;
              detectValue.parsedValue = {
                partNumber: valueStr
              };
              return detectValue;
            }
            break;
          case 'K':
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
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
          case '4K':
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                orderNumber: valueStr
              };
              return detectValue;
            }
            break;
          case '13K':
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                tariffNo: valueStr
              };
              return detectValue;
            }
            break;
          case '16K':
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                deliveryNumber: valueStr
              };
              return detectValue;
            }
            break;
          case 'Q':
          case '2Q':
            // value must be a number
            valueStr = cleanValue.substring(1, cleanValue.length);
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
          case '3Q':
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.75;
              detectValue.parsedValue = {
                quantityUnits: valueStr
              };
              return detectValue;
            }
            break;
          case 'P':
            // for Yageo reels, this is the customer part number (not useful)
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(1, cleanValue.length);
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
            valueStr = cleanValue.substring(1, cleanValue.length);
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
            valueStr = cleanValue.substring(2, cleanValue.length);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              lot: valueStr,
            };
            return detectValue;
          case 'L':
            // line number
            valueStr = cleanValue.substring(2, cleanValue.length);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              lineNumber: valueStr,
            };
            return detectValue;
          case '10L':
            // manufacturer country
            valueStr = cleanValue.substring(2, cleanValue.length);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              manufacturerCountry: valueStr,
            };
            return detectValue;
          case 'V':
            // version number
            valueStr = cleanValue.substring(2, cleanValue.length);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.75;
            detectValue.parsedValue = {
              versionNumber: valueStr,
            };
            return detectValue;
          case '9D':
          case 'D':
          case '16D':
            // date code
            if (cleanValue.length === 2 + 4) {
              valueStr = cleanValue.substring(2, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.9;
              detectValue.parsedValue = {
                dateCode: valueStr,
              };
              return detectValue;
            }
            return { ...detectValue, reason: `Date code wrong length '${cleanValue.length}' should be '6'` };
          case '17D':
            // manufactured date
            if (cleanValue.length > 5) {
              valueStr = cleanValue.substring(2, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.9;
              detectValue.parsedValue = {
                manufactureDate: valueStr,
              };
              return detectValue;
            }
            return { ...detectValue, reason: `Manufacture date wrong length '${cleanValue.length}' should be > 5` };
          case '4L':
            // country of origin (COO)
            if (cleanValue.length > 3 && cleanValue.length < 7) {
              valueStr = cleanValue.substring(2, cleanValue.length);
              detectValue.success = true;
              detectValue.type = 'code128';
              detectValue.labelType = 'part';
              detectValue.certainty = 0.9;
              detectValue.parsedValue = {
                country: valueStr,
              };
              return detectValue;
            }
            return { ...detectValue, reason: `COO wrong length '${cleanValue.length}' should be between 4 and 6` };
          case 'R':
            // reel id 
            valueStr = cleanValue.substring(1, cleanValue.length);
            detectValue.success = true;
            detectValue.type = 'code128';
            detectValue.labelType = 'part';
            detectValue.certainty = 0.9;
            detectValue.parsedValue = {
              reelId: valueStr,
            };
            return detectValue;
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
GenericOneDimensional.Name = 'Generic 1D';
GenericOneDimensional.LabelType = BarcodeLabelTypes.Tokenized;