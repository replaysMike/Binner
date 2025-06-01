import Koa from "./labelDetection/Koa";
import Bourns from "./labelDetection/Bourns";
import Kemet from "./labelDetection/Kemet";
import Yageo from "./labelDetection/Yageo";
import Stackpole from "./labelDetection/Stackpole";
import Vishay from "./labelDetection/Vishay";
import Rohm from "./labelDetection/Rohm";
import LiteOn from "./labelDetection/LiteOn";
import Panasonic from "./labelDetection/Panasonic";
import Samsung from "./labelDetection/Samsung";
import Walsin from "./labelDetection/Walsin";
import GenericTokenized from "./labelDetection/GenericTokenized";
import GenericOneDimensional from "./labelDetection/GenericOneDimensional";
import _ from "underscore";

export const detectLabel = (value) => {
  // add detectors here (order matters, place generic ones at the end)
  let detectors = [Vishay, Rohm, Kemet, Bourns, Koa, Yageo, Stackpole, LiteOn, Panasonic, Samsung, Walsin, GenericTokenized, GenericOneDimensional];
  //let detectors = [Yageo];

  let detectValue = {
    type: null,
    labelType: null,
    success: false,
    parsedValue: null,
    vendor: 'Unknown',
    reason: null,
  };

  for(let i = 0; i < detectors.length; i++) {
    const detector = detectors[i];
    console.debug(`Processing detector`, detector.Name);
    try {
      detectValue = detector(value);
      if (detectValue.success)
        return detectValue;
      else
        console.debug(`Detector ${detector.Name} not detected. Reason: ${detectValue.reason}`, detectValue);
    } catch(err) {
      console.error(`Error occurred detecting label using ${detector.Name}!`, err);
    }
  }

  return { ...detectValue, reason: `No label detected.` };
};