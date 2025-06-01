import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";
import _ from "underscore";

/** Detect a KOA part label */
export default function Koa(value) {
  const execute = () => {
    const sections = value.replaceAll('\r', '').split(Koa.FieldDelimiter);
    let detectValue = handlePartLabel(sections);
    if (detectValue.success)
      return detectValue;
    detectValue = handleCustomerLabel(sections);
    return detectValue;
  };

  const handlePartLabel = (sections) => {
    // code: KHK:1:RK73B1ETTP201J:10000:X:5018C394:623:KHK-A01
    console.debug(`Detect ${Koa.Name} part label sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendorName: Koa.Name,
      reason: null
    };
    if (sections.length === 8) {
      // could be koa label, check the data to see if it looks right
      let warehouse = ''; // part number, alphanumeric
      let unknown1 = 0;
      if (sections[0] === 'KHK') {
        warehouse = sections[0];
        let unknownStr = sections[1]; // must be a number
        if (!isNumeric(unknownStr)) return { ...detectValue, reason: `Section 1 not a number` };
        unknown1 = parseInt(unknownStr);
        let partNumber = ''; // partNumber, alphanumeric
        partNumber = sections[2];
        let qty = 0;
        const qtyStr = sections[3]; // must be a number
        if (!isNumeric(qtyStr)) return { ...detectValue, reason: `Qty '${qtyStr}' not a number` };
        qty = parseInt(qtyStr);
        let warehouseLetter = sections[4]; // warehouse letter, alphanumeric
        let lot = sections[5]; // warehouse letter, alphanumeric
        let unknown2 = sections[6]; // warehouse letter, alphanumeric
        let warehouseId = sections[7]; // warehouse id, alphanumeric.

        detectValue.success = true;
        detectValue.type = 'pdf417';
        detectValue.labelType = 'part';
        detectValue.parsedValue = {
          warehouse,
          unknown1,
          partNumber,
          quantity: qty,
          warehouseLetter,
          lot,
          unknown2,
          warehouseId
        };
        return detectValue;
      }
      return { ...detectValue, reason: `No start condition 'KHK'` };
    }
    return { ...detectValue, reason: `Sections != 8 (${sections.length})` };
  };

  const handleCustomerLabel = (sections) => {
    // code: :RK73B1ETTP201J::14:X:5018C394:0001::::SOL-A25
    console.debug(`Detect ${Koa.Name} customer label sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Koa.Name,
      reason: null,
    };
    if (sections.length === 11 || sections.length === 12) { // 12 when it has an extra empty with \r
      // could be koa label, check the data to see if it looks right
      let empty1 = sections[0];
      let warehouseLetter = sections[4];
      if (empty1.length === 0) {
        if (warehouseLetter === 'X') {
          let partNumber = sections[1]; // partNumber, alphanumeric
          let empty2 = sections[2]; // unknown
          let unknownNumber1Str = sections[3];
          if (!isNumeric(unknownNumber1Str)) return { ...detectValue, reason: `UnknownNumber '${unknownNumber1Str}' not a number` };
          let unknownNumber1 = parseInt(unknownNumber1Str); // unknown
          let traceCode = sections[5]; // trace code, alphanumeric
          let unknownNumber2Str = sections[6];
          if (!isNumeric(unknownNumber2Str)) return { ...detectValue, reason: `UnknownNumber2 '${unknownNumber2Str}' not a number` };
          let unknownNumber2 = parseInt(unknownNumber2Str); // unknown
          let supplierPartNumber = sections[7]; // partNumber, alphanumeric used when sections = 12

          let warehouseId = sections[10]; // some kind of warehouse id, alphanumeric

          detectValue.success = true;
          detectValue.type = 'pdf417';
          detectValue.vendor = 'KOA';
          detectValue.labelType = 'part';
          detectValue.parsedValue = {
            empty1,
            empty2,
            warehouseId,
            unknownNumber1,
            unknownNumber2,
            partNumber,
            supplierPartNumber,
            warehouseLetter,
            traceCode
          };
          return detectValue;
        }
        return { ...detectValue, reason: `Unexpected warehouse letter '${warehouseLetter}'` };
      }
      return { ...detectValue, reason: `First value should be empty '${empty1}'` };
    }
    return { ...detectValue, reason: `Sections != 11 or 12 (${sections.length})` };
  };

  return execute();
};
Koa.Name = 'KOA Speer Electronics, Inc';
Koa.LabelType = BarcodeLabelTypes.Delimited;
Koa.FieldDelimiter = ':';