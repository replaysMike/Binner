import { BarcodeLabelTypes } from "../barcodeLabelTypes";
import { isNumeric } from "../Utils";

/** Detect a Yageo part label */
export default function Yageo(value) {
  const execute = () => {
    let detectValue = handleDelimited();
    if (detectValue.success)
      return detectValue;
    detectValue = handleTokenized();
    return detectValue;
  };

  const handleDelimited = () => {
    const sections = value.split(Yageo.FieldDelimiter);
    // code: 042390059 B2014C1206C475K3PAC7800 0002000
    // code: 232802636 B1008C1206C103K1RAC7800    0004000MEXICO    MX
    // delimited AND prefixed
    // code: 31PCC0603KRX7R8BB392\r32P223891615631\r1T31E512061159F1056753\rQ0004000\r9D15511610\r26 505\r0002\r
    console.debug(`Detect ${Yageo.Name} delimited`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Yageo.Name,
      reason: null,
    };
    if (sections.length === 8) {
      // could be yageo label, check the data to see if it looks right
      let partNumber;
      let internalCode;
      let internalCode2;
      let internalCode3;
      let internalCode4;
      let internalCode5;
      let internalCode6;
      let quantity = 0;
      if (sections[0].substring(0, 3) === '31P') {
        partNumber = sections[0].substring(3, sections[0].length);
      }
      if (sections[1].substring(0, 3) === '32P') {
        internalCode = sections[1].substring(3, sections[1].length);
      }
      if (sections[2].substring(0, 2) === '1T') {
        internalCode2 = sections[2].substring(2, sections[2].length);
      }
      if (sections[3].substring(0, 1) === 'Q') {
        let qtyStr = sections[3].substring(1, sections[3].length);
        if (isNumeric(qtyStr))
          quantity = parseInt(qtyStr);
      }
      if (sections[4].substring(0, 2) === '9D') {
        internalCode3 = sections[4].substring(2, sections[4].length);
      }
      if (sections[5].substring(0, 2) === '26') {
        internalCode4 = sections[6].substring(0, 2);
        internalCode5 = sections[5].substring(3, sections[5].length);
      }
      internalCode6 = sections[6].substring(0, sections[6].length);

      detectValue.success = true;
      detectValue.type = 'datamatrix';
      detectValue.labelType = 'part';
      detectValue.parsedValue = {
        internalCode,
        internalCode2,
        internalCode3,
        internalCode4,
        internalCode5,
        internalCode6,
        partNumber,
        quantity,
      };
      return detectValue;
    }
    return { ...detectValue, reason: `Sections != 8 (${sections.length})` };
  };

  const handleTokenized = () => {
    // Supplier: Yageo, private assembly label
    // code: UID19179844PID110561550TIM8/30/2013 3:16:06 PMSPL0Y033LIDSPNCC0402JRNPO9BN150QTY8992DCD1145MSD0COOCNESDROHC6O6SER19179844
    console.debug(`Detect ${Yageo.Name} tokenized`, value);
    let detectValue = {
      type: null,
      labelType: null,
      success: false,
      parsedValue: null,
      vendor: Yageo.Name,
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
Yageo.Name = 'Yageo';
Yageo.LabelType = BarcodeLabelTypes.Delimited;
Yageo.FieldDelimiter = "\r";