import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";
import _ from "underscore";

/** Detect a generic tokenized part label */
export default function GenericTokenized(value) {
  const execute = () => {
    // Supplier: Yageo, private assembly label
    // code: UID19179844PID110561550TIM8/30/2013 3:16:06 PMSPL0Y033LIDSPNCC0402JRNPO9BN150QTY8992DCD1145MSD0COOCNESDROHC6O6SER19179844
    console.debug(`Detect ${GenericTokenized.Name} data length=${value.length}`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: GenericTokenized.Name,
      reason: null,
    };
    const properties = ['UID', 'PID', 'TIM', 'SPL', 'LID', 'SPN', 'QTY', 'DCD', 'MSD', 'COO', 'ESD', 'SER'];
    // scan left to right. if we find a token, continue until we find another token or end of string
    let scanStr = '';
    const detectedValues = [];
    for (let i = 0; i < value.length; i++) {
      scanStr += value[i];
      for (let startPropertyCursor = 0; startPropertyCursor < properties.length; startPropertyCursor++) {
        const result = scanForProperty(value, i, scanStr, startPropertyCursor, properties);
        if (result.success) {
          i = result.ptr; // reset the ptr
          detectedValues.push(result);
          scanStr = '';
          break;
        }
      } // loop 2 (start property cursor)
    } // loop 1 (cursor start)
    if (detectedValues.length > 0) {
      detectValue.success = true;
      detectValue.type = 'datamatrix';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        pcn: _.find(detectedValues, i => i.section === 'UID')?.value,
        itemNumber: _.find(detectedValues, i => i.section === 'PID')?.value,
        receivedDate: _.find(detectedValues, i => i.section === 'TIM')?.value,
        supplier: _.find(detectedValues, i => i.section === 'SPL')?.value, // Supplier. Yageo = 0Y033 this may be specific to the label origin
        lotId: _.find(detectedValues, i => i.section === 'LID')?.value,
        partNumber: _.find(detectedValues, i => i.section === 'SPN')?.value,
        dateCode: _.find(detectedValues, i => i.section === 'DCD')?.value,
        quantity: parseInt(_.find(detectedValues, i => i.section === 'QTY')?.value ?? 0),
        msd: _.find(detectedValues, i => i.section === 'MSD')?.value,
        country: _.find(detectedValues, i => i.section === 'COO')?.value,
        esd: _.find(detectedValues, i => i.section === 'ESD')?.value,
        serial: _.find(detectedValues, i => i.section === 'SER')?.value,
      };
      return detectValue;
    }
    return { ...detectValue, reason: `No tokens detected.` };
  };

  const scanForProperty = (value, i, scanStr, startPropertyCursor, properties) => {
    if (scanStr.startsWith(properties[startPropertyCursor])) {
      let startProperty = properties[startPropertyCursor];
      // found a token, keep going until we find another token or end of string
      for (let j = i + 2; j < value.length; j++) {
        for (let endPropertyCursor = startPropertyCursor; endPropertyCursor < properties.length; endPropertyCursor++) {
          let section = value.substring(i + 1, j);
          if (section.endsWith(properties[endPropertyCursor])) {
            let endProperty = properties[endPropertyCursor];
            // found the ending token
            const sectionValue = value.substring(i + 1, j - endProperty.length);
            const newI = j - endProperty.length - 1; // reset the ptr
            return { success: true, ptr: newI, section: startProperty, value: sectionValue }; // scan for new property
          }
        } // loop4 (end property cursor)
      } // loop 3 (cursor end)
    }
    return { success: false, ptr: i + 1, section: null, value: null };
  };

  return execute();
};
GenericTokenized.Name = 'Generic';
GenericTokenized.LabelType = BarcodeLabelTypes.Tokenized;