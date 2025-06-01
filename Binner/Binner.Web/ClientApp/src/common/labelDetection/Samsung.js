import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Samsung part label */
export default function Samsung(value) {
  const execute = () => {
    let detectValue = handleQrCode();
    if (detectValue.success)
      return detectValue;
    detectValue = handleCode128();
    return detectValue;
  };

  const handleQrCode = () => {
    const sections = value.replaceAll('\r', '').split(Samsung.FieldDelimiter);
    // code: AJ1FP7G/CL10C101JB8NNNC/1687/0004000\r
    console.debug(`Detect ${Samsung.Name} sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Samsung.Name,
      reason: null,
    };
    if (sections.length === 4) {
      // could be samsung label, check the data to see if it looks right
      let lotCode = sections[0]; // lot code, alphanumeric, 7 digits
      let partNumber = sections[1]; // part number, alphanumeric
      let dateCodeStr = sections[2]; // date code, numeric, 4 digits
      let qtyStr = sections[3]; // quantity, numeric and left padded with 0, 7 digits
      if (lotCode.length != 7) return { ...detectValue, reason: `LotCode should be 7 digits.` };
      if (dateCodeStr.length != 4) return { ...detectValue, reason: `DateCode should be 4 digits.` };
      if (qtyStr.length != 7) return { ...detectValue, reason: `Qty should be 7 digits.` };
      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      let quantity = parseInt(qtyStr);
      if (!isNumeric(dateCodeStr)) return { ...detectValue, reason: `Date code '${dateCodeStr}' not a number` };
      let dateCode = parseInt(dateCodeStr);

      detectValue.success = true;
      detectValue.type = 'qrcode';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        partNumber,
        quantity,
        dateCode,
        lotCode
      };
      return detectValue;
    }
    return { ...detectValue, reason: `Sections != 4 (${sections.length})` };
  };

  const handleCode128 = () => {
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Samsung.Name,
      reason: null,
    };
    if (value.length === 19) {
      let lotCode = value.substring(0, 7); // lot code, alphanumeric, 7 digits
      let dateCodeStr = value.substring(7, 11); // date code, numeric, 4 digits
      let qtyStr = value.substring(11, 18); // quantity, numeric and left padded with 0, 7 digits

      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      let quantity = parseInt(qtyStr);
      if (!isNumeric(dateCodeStr)) return { ...detectValue, reason: `Date code '${dateCodeStr}' not a number` };
      let dateCode = parseInt(dateCodeStr);
      detectValue.success = true;
      detectValue.type = 'code128';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        quantity,
        dateCode,
        lotCode
      };
    }
    return { ...detectValue, reason: `Length != 19 (${value.length})` };
  };

  return execute();
};
Samsung.Name = 'Samsung Electronics';
Samsung.LabelType = BarcodeLabelTypes.Multiple;
Samsung.FieldDelimiter = '/';