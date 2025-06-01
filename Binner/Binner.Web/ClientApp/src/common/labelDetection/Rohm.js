import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Rohm part label */
export default function Rohm(value) {
  const execute = () => {
    // fixed width, using ' ' character to fill empty space
    console.debug(`Detect ${Rohm.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Rohm.Name,
      reason: null
    };

    detectValue = handleVersion1(detectValue);
    if (detectValue.success)
      return detectValue;
    return detectValue;
  }

  const handleVersion1 = (detectValue) => {
    if (value.length === 115) {
      // first 19 chars are the part number, with right padding ' '
      let partNumber = value.substring(0, 19).trimEnd();
      // next 6 are quantity, with left zero padding. Example: 003000
      let quantityStr = value.substring(19, 25);
      if (!isNumeric(quantityStr)) return { ...detectValue, reason: `Quantity '${quantityStr}' not a number` };
      let quantity = parseInt(quantityStr)

      // next 4 chars are date code
      let dateCode = value.substring(25, 29);
      // next 6 are lot number. Example: E1019K, or A7E06H
      let lot = value.substring(29, 35);

      // next 3 are marking lot number. Example: 017, or 001
      let pbFree = value.substring(35, 38).trimEnd();

      // remainder is white space
      if (partNumber && quantity >= 0) {
        detectValue.success = true;
        detectValue.type = 'qrcode';
        detectValue.labelType = 'part';
        detectValue.parsedValue = {
          partNumber,
          quantity,
          dateCode,
          lot,
          pbFree
        };
      }
      return detectValue;
    }
    return { ...detectValue, reason: `Too short (${value.length})` };
  }

  return execute();
};
Rohm.Name = 'Rohm Semiconductor';
Rohm.LabelType = BarcodeLabelTypes.FixedWidth;