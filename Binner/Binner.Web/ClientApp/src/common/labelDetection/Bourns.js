import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Bourns part label */
export default function Bourns(value) {
  const execute = () => {
    const sections = value.replaceAll('\r', '').split(Bourns.FieldDelimiter);
    // code: 042390059 B2014C1206C475K3PAC7800 0002000
    // code: 232802636 B1008C1206C103K1RAC7800    0004000MEXICO    MX
    console.debug(`Detect ${Bourns.Name} sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Bourns.Name,
      reason: null,
    };
    if (sections.length === 4) {
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

  return execute();
};
Bourns.Name = 'Bourns';
Bourns.LabelType = BarcodeLabelTypes.Delimited;
Bourns.FieldDelimiter = '+';