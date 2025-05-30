import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Stackpole part label */
export default function Stackpole(value) {
  const execute = () => {
    const sections = value.replaceAll('\r', '').split(Stackpole.FieldDelimiter);
    // stackpole uses a simple delimited format the same as GenericTokenized, but not all of its fields are properly tokenized.

    // code: 1PRMCF2010ZT0R00,Q4000,1TT170419155,17/16,JP\r
    console.debug(`Detect ${Stackpole.Name} sections=${sections.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: {
        partNumber: null,
        quantity: null,
        lot: null,
        dateCode: null,
        country: null
      },
      vendor: Stackpole.Name,
      reason: null
    };
    if (!value?.length > 0) return detectValue;

    for(let s = 0; s < sections.length; s++) {
      const section = sections[s];
      detectValue = processSection(detectValue, section, s, sections.length);
    }

    if (detectValue.parsedValue.partNumber?.length > 0 && detectValue.parsedValue.quantity >= 0) {
      detectValue.success = true;
      detectValue.type = 'pdf417';
      detectValue.labelType = 'part';
    }

    return detectValue;
  };
  
  const processSection = (detectValue, section, sectionIndex, sectionsLength) => {
    const knownIdentifiers = ['1P', 'Q', 'P', '1T', '9D', '4L'];
    for (let i = 0; i < knownIdentifiers.length; i++) {
      let knownIdentifier = knownIdentifiers[i];
      let valueStr = section;
      if (section.startsWith(knownIdentifier)) {
        switch (knownIdentifier) {
          case '1P':
            // part number
            valueStr = section.substring(2, section.length);
            detectValue.parsedValue.partNumber = valueStr;
            break;
          case 'Q':
            // value must be a number
            valueStr = section.substring(1, section.length);
            if (isNumeric(valueStr)) {
              detectValue.parsedValue.quantity = parseInt(valueStr);
            }
            break;
          case 'P':
            // alternate part number
            valueStr = section.substring(1, section.length);
            if (!detectValue.parsedValue.partNumber?.length)
              detectValue.parsedValue.partNumber = valueStr;
            else
              detectValue.parsedValue.otherPartNumber= valueStr;
            break;
          case '1T':
            // lot number
            valueStr = section.substring(2, section.length);
            detectValue.parsedValue.lot = valueStr;
            break;
          case '9D':
            // date code
              valueStr = section.substring(2, section.length);
              detectValue.parsedValue.dateCode = valueStr;
            break;
          case '4L':
            // country of origin (COO)
            valueStr = section.substring(2, section.length);
            detectValue.parsedValue.country = valueStr;
            break;
          default:
            break;
        }
      } else {
        if (sectionIndex === sectionsLength - 1) {
          // last item is COO, not tokenized
          if (!detectValue.parsedValue.country?.length) // don't include it if we already wrote a value
            detectValue.parsedValue.country = valueStr;
        }
        if (sectionIndex === sectionsLength - 2) {
          if (!detectValue.parsedValue.dateCode?.length) // don't include it if we already wrote a value
            detectValue.parsedValue.dateCode = valueStr;
        }
      }
    }
    return detectValue;
  };
  
  return execute();
};
Stackpole.Name = 'Stackpole Electronics Inc';
Stackpole.LabelType = BarcodeLabelTypes.Delimited;
Stackpole.FieldDelimiter = ',';