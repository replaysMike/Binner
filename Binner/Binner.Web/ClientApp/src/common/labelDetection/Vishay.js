import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Vishay part label */
export default function Vishay(value) {
  const execute = () => {
    // fixed width, using ' ' character to fill empty space
    console.debug(`Detect ${Stackpole.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Vishay.Name,
      reason: null
    };

    if (value.length === 326) {
      // first 18 chars are the part number
      let partNumber = value.substring(0, 18).trimStart();
      // next 18 chars are lot1 (10) + date (8). Example: 000527030403/16/12 or R9002DL.0401/13/20
      let lot1 = value.substring(18, 28).trimStart();
      let lotDate = value.substring(28, 36);
      // next 10 chars are lot2. Example: 1920031A91
      let lot2 = value.substring(36, 46).trimStart();
      // next 53 chars are batch. Example: 202003PH19
      let batch = value.substring(46, 99).trimStart();

      // next 13 are quantity as a decimal. Example: 2500.000
      let quantityStr = value.substring(99, 112).trimStart();
      if (!isNumeric(quantityStr)) return { ...detectValue, reason: `Quantity '${quantityStr}' not a number` };
      let quantity = parseInt(quantityStr);

      // 14 after quantity is MFG PO
      let mfgPo = value.substring(112, 126).trimStart();
      // next 15 are unknown
      let unknown = value.substring(126, 141).trimStart();

      // next 4 chars are date code
      let dateCode = value.substring(141, 145).trimStart();
      // next 4 chars are date code 2
      let dateCode2 = value.substring(145, 149).trimStart();
      // next 4 are region. Example: 2410
      let regionStr = value.substring(149, 153).trimStart();
      if (!isNumeric(regionStr)) return { ...detectValue, reason: `Region '${regionStr}' not a number` };
      let region = parseInt(regionStr);

      // next 4 are SL. Example: 0010
      let sl = value.substring(153, 157).trimStart();
      // next 4 are ChkD. Example: A41 or 100
      let chkD = value.substring(157, 161).trimStart();
      // next 30 are operator. Example: SH903
      let operator = value.substring(161, 191).trimStart();

      // hazy details
      // lot1 30 chars
      let lot1Again = value.substring(191, 221).trimStart();
      // quantity 6 chars
      let quantityAgainStr = value.substring(221, 227).trimStart();
      let quantityAgain = parseInt(quantityAgainStr);
      // part number 45 chars
      let partNumberAgain = value.substring(227, 272).trimStart();
      // series 25 chars. Example: WSL-2010  0.0069 1  EA E3
      let alternateName = value.substring(272, 297).trimStart();
      // next 28 are serial. Example: 000000OMM400300970
      let serial = value.substring(297, 325).trimStart();

      detectValue.success = true;
      detectValue.type = 'pdf417';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        lot1,
        lot2,
        lotDate,
        batch,
        partNumber,
        quantity,
        mfgPo,
        unknown,
        dateCode,
        dateCode2,
        region,
        sl,
        chkD,
        operator,
        lot1Again,
        quantityAgain,
        partNumberAgain,
        alternateName,
        serial
      };
      return detectValue;
    }
    return { ...detectValue, reason: `Too short (${value.length})` };
  }

  return execute();
};
Vishay.Name = 'Vishay';
Vishay.LabelType = BarcodeLabelTypes.FixedWidth;