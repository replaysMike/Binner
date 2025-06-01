import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Kemet part label */
export default function Kemet(value) {

  const execute = () => {
    const sections = value.replaceAll('\r', '').split(Kemet.FieldDelimiter);
    console.debug(`Detect ${Kemet.Name} sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Kemet.Name,
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
  
  return execute();
};
Kemet.Name = 'Kemet';
Kemet.LabelType = BarcodeLabelTypes.Delimited;
Kemet.FieldDelimiter = ' ';