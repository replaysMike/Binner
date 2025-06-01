import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a LiteOn part label */
export default function LiteOn(value) {
  const execute = () => {
    const sections = value.replaceAll('\r', '').split(LiteOn.FieldDelimiter);
    // code: ;;LTST-C191KGKT;2108404746;5000;5/P/D;RLC21032787551;2113;MADE IN CHINA\r
    console.debug(`Detect ${LiteOn.Name} sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: LiteOn.Name,
      reason: null,
    };
    if (sections.length === 9) {
      // could be LiteOn label, check the data to see if it looks right
      let customer = sections[0]; // customer, can be blank
      let customerPartNumber = sections[1]; // customer provided part number, can be blank
      let partNumber = sections[2]; // part number, alphanumeric
      let lotCode = sections[3]; // lot number, alphanumeric
      let qtyStr = sections[4]; // quantity, numeric
      let batch = sections[5]; // batch id, alphanumeric
      let lpn = sections[6]; // lpn number, alphanumeric
      let dateCodeStr = sections[7]; // lot number, numeric
      let comment = sections[8]; // comment, alphanumeric

      if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
      let quantity = parseInt(qtyStr);
      if (!isNumeric(dateCodeStr)) return { ...detectValue, reason: `Date code '${dateCodeStr}' not a number` };
      let dateCode = parseInt(dateCodeStr);

      detectValue.success = true;
      detectValue.type = 'datamatrix';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        customer,
        customerPartNumber,
        partNumber,
        lotCode,
        quantity,
        batch,
        lpn,
        dateCode,
        comment
      };
      return detectValue;
    }
    return { ...detectValue, reason: `Sections != 9 (${sections.length})` };
  };

  return execute();
};
LiteOn.Name = 'Lite-On Electronics';
LiteOn.LabelType = BarcodeLabelTypes.Delimited;
LiteOn.FieldDelimiter = ';';