import _ from "underscore";
import { ResistorIcon } from "./icons";
import { Icon } from "semantic-ui-react";

export const ProjectColors = [
  { name: '', value: 0 },
  { name: 'red', value: 1 },
  { name: 'blue', value: 2 },
  { name: 'black', value: 3 },
  { name: 'green', value: 4 },
  { name: 'orange', value: 5 },
  { name: 'purple', value: 6 },
  { name: 'yellow', value: 7 },
  { name: 'pink', value: 8 },
  { name: 'grey', value: 9 },
  { name: 'violet', value: 10 },
];

export const MountingTypes = {
  None: { value: 0, name: 'None', icon: 'cancel', description: '' },
  ThroughHole: { value: 1, name: 'Through Hole', icon: <ResistorIcon />, description: 'Part leads run through the PCB' },
  SurfaceMount: { value: 2, name: 'Surface Mount', icon: 'microchip', description: 'Part is soldered to the surface of the PCB' },
};

export const PackageTypes = {
  // package types are freeform, no value id required
  None: { name: 'None', description: '' },
  
  SMD201: { name: '201', description: '0.6mm x 0.3mm (metric 603)' },
  SMD402: { name: '402', description: '1.0mm x 0.5mm (metric 1005)' },
  SMD603: { name: '603', description: '1.6mm x 0.8mm (metric 1608)' },
  SMD805: { name: '805', description: '2.0mm x 1.25mm (metric 2012)' },
  SMD1206: { name: '1206', description: '3.2mm x 1.6mm (metric 3216)' },
  SMD1210: { name: '1210', description: '3.2mm x 2.5mm (metric 3225)' },
  SMD1812: { name: '1812', description: '4.5mm x 3.2mm (metric 4532)' },
  SMD2010: { name: '2010', description: '5.0mm x 2.5mm (metric 5025)' },
  SMD2512: { name: '2512', description: '6.3mm x 3.2mm (metric 6332)' },
  SMD102: { name: '102', description: '2.2mm x 1.1mm' },
  SMD204: { name: '204', description: '3.6mm x 1.4mm' },
  SMD207: { name: '207', description: '5.8mm x 2.2mm' },
  SMD1008: { name: '1008', description: '2.5mm x 2.0mm (metric 2520)' },
  SMD1806: { name: '1806', description: '4.5mm x 1.6mm (metric 4516)' },
  SMD1825: { name: '1825', description: '4.5mm x 6.4mm (metric 4564)' },
  SMD2920: { name: '2920', description: '7.4mm x 5.1mm (metric 7451)' },
  SMD9005: { name: '9005', description: '0.3mm x 0.15mm (metric 3015)' },
  SMD1005: { name: '1005', description: '0.4mm x 0.2mm (metric 402)' },
  SMD2725: { name: '2725', description: '6.9mm x 6.3mm (metric 6833)' },
  
  DIP: { name: 'DIP', description: 'Dual inline package', type: 'through-hole' },
  DIP4: { name: 'DIP4', description: '4.6mm x 7.62mm', type: 'through-hole' },
  DIP8: { name: 'DIP8', description: '9.66mm x 7.62mm', type: 'through-hole' },
  DIP14: { name: 'DIP14', description: '19.75mm x 7.62mm', type: 'through-hole' },
  DIP16: { name: 'DIP16', description: '19.75mm x 7.62mm', type: 'through-hole' },
  DIP20: { name: 'DIP20', description: '25.1mm x 7.62mm', type: 'through-hole' },
  DIP24: { name: 'DIP24', description: '32.0mm x 15.24mm', type: 'through-hole' },
  DIP28: { name: 'DIP28', description: '39.75mm x 15.24mm', type: 'through-hole' },
  DIP40: { name: 'DIP40', description: '52.07mm x 15.24mm', type: 'through-hole' },
  DIP64: { name: 'DIP64', description: '82.5mm x 22.86mm', type: 'through-hole' },
  SIP: { name: 'SIP', description: 'Single inline package', type: 'through-hole' },
  SSIP: { name: 'SSIP', description: 'Single inline package', type: 'through-hole' },
  SDIP: { name: 'SDIP', description: 'Shrink DIP', type: 'through-hole' },
  SKDIP: { name: 'SKDIP', description: 'Skinny DIP', type: 'through-hole' },
  CDIP: { name: 'CDIP', description: 'Ceramic DIP', type: 'through-hole' },
  CERDIP: { name: 'CERDIP', description: 'Glass-sealed ceramic DIP', type: 'through-hole' },
  PDIP: { name: 'PDIP', description: 'Plastic DIP', type: 'through-hole' },
  MDIP: { name: 'MDIP', description: 'Molded DIP', type: 'through-hole' },
  ZIP: { name: 'ZIP', description: 'Zig-zag inline package', type: 'through-hole' },
  QIP: { name: 'QIP', description: 'Quad inline package (staggered)', type: 'through-hole' },

  SOIC: { name: 'SOIC', description: 'Small-outline integrated circuit' },
  SOIC4: { name: 'SOIC-4', description: 'Small-outline integrated circuit, 4-pin' },
  SOIC8: { name: 'SOIC-8', description: 'Small-outline integrated circuit, 8-pin' },
  SOIC10: { name: 'SOIC-10', description: 'Small-outline integrated circuit, 10-pin' },
  SOIC12: { name: 'SOIC-12', description: 'Small-outline integrated circuit, 12-pin' },
  SOIC14: { name: 'SOIC-14', description: 'Small-outline integrated circuit, 14-pin' },
  SOIC16: { name: 'SOIC-16', description: 'Small-outline integrated circuit, 16-pin' },
  SOIC20: { name: 'SOIC-20', description: 'Small-outline integrated circuit, 20-pin' },
  SOIC24: { name: 'SOIC-24', description: 'Small-outline integrated circuit, 24-pin' },
  SOIC28: { name: 'SOIC-28', description: 'Small-outline integrated circuit, 28-pin' },
  SOIC32: { name: 'SOIC-32', description: 'Small-outline integrated circuit, 32-pin' },
  SOIC36: { name: 'SOIC-36', description: 'Small-outline integrated circuit, 36-pin' },
  SOIC40: { name: 'SOIC-40', description: 'Small-outline integrated circuit, 40-pin' },
  SOIC48: { name: 'SOIC-48', description: 'Small-outline integrated circuit, 48-pin' },
  SOIC64: { name: 'SOIC-64', description: 'Small-outline integrated circuit, 64-pin' },

  SOP: { name: 'SOP', description: 'Small-outline package' },
  SSOP: { name: 'SSOP', description: 'Shrink small-outline package' },
  TSOP: { name: 'TSOP', description: 'Thin small-outline package' },
  DSOP: { name: 'DSOP', description: 'Dual small-outline package' },
  CSOP: { name: 'CSOP', description: 'Ceramic small-outline package' },
  MSOP: { name: 'MSOP', description: 'Mini small-outline package' },
  TSSOP: { name: 'TSSOP', description: 'Thin shrink small-outline package' },
  VSOP: { name: 'VSOP', description: 'Very-thin small-outline package' },
  
  DPAK: { name: 'DPAK', description: 'High power mosfet. 3 to 5-pin' },
  D2PAK: { name: 'D2PAK', description: 'Higher power mosfet. 3,5,6,7,8,9 pins' },
  D3PAK: { name: 'D3PAK', description: 'Higher power mosfet' },
  QDPAK: { name: 'QDPAK', description: 'Larger DPAK package. 3 to 5-pin' },
  DPAKPlus: { name: 'DPAK+', description: 'Improved efficiency DPAK package. 3 to 5-pin' },

  SOT: { name: 'SOT', description: 'Small-outline transistor' },
  SOT233: { name: 'SOT-23-3', description: 'Small-outline transistor, 3-pin' },
  SOT89: { name: 'SOT-89', description: 'Small-outline transistor, 4-pin' },
  SOT143: { name: 'SOT-143', description: 'Small-outline transistor, 4-pin' },
  SOT223: { name: 'SOT-223', description: 'Small-outline transistor, 4-pin' },
  SOT323: { name: 'SOT-323', description: 'Small-outline transistor, 3-pin' },
  SOT416: { name: 'SOT-516', description: 'Small-outline transistor, 3-pin' },
  SOT663: { name: 'SOT-663', description: 'Small-outline transistor, 3-pin' },
  SOT723: { name: 'SOT-723', description: 'Small-outline transistor, 3-pin' },
  SOT883: { name: 'SOT-883', description: 'Small-outline transistor, 3-pin' },

  SOT428: { name: 'SOT-428', description: 'Small-outline transistor (DPAK, TO-252), 3 to 5-pin' },
  SOT236: { name: 'SOT-23-6', description: 'Small-outline transistor, 6-pin' },
  SOT26: { name: 'SOT-26', description: 'Small-outline transistor, 6-pin' },
  SOT353: { name: 'SOT-353', description: 'Small-outline transistor, 5-pin' },
  SOT363: { name: 'SOT-363', description: 'Small-outline transistor, 6-pin' },
  SOT563: { name: 'SOT-563', description: 'Small-outline transistor, 6-pin' },
  SOT665: { name: 'SOT-665', description: 'Small-outline transistor, 5-pin' },
  SOT666: { name: 'SOT-666', description: 'Small-outline transistor, 6-pin' },
  SOT886: { name: 'SOT-886', description: 'Small-outline transistor, 6-pin' },
  SOT891: { name: 'SOT-891', description: 'Small-outline transistor, 6-pin' },
  SOT953: { name: 'SOT-953', description: 'Small-outline transistor, 5-pin' },
  SOT963: { name: 'SOT-963', description: 'Small-outline transistor, 6-pin' },
  SOT1115: { name: 'SOT-1115', description: 'Small-outline transistor, 6-pin' },
  SOT1202: { name: 'SOT-1202', description: 'Small-outline transistor, 6-pin' },

  SOD80C: { name: 'SOD-80C', description: 'Small-outline diode, 3.5mm x 1.5mm' },
  SOD123: { name: 'SOD-123', description: 'Small-outline diode, 2.65mm x 1.6mm x 1.35mm' },
  SOD128: { name: 'SOD-128', description: 'Small-outline diode, 3.8mm x 2.5mm x 1.1mm' },
  SOD323: { name: 'SOD-323', description: 'Small-outline diode (SC-76), 1.7mm x 1.25mm x 1.1mm' },
  SOD523: { name: 'SOD-523', description: 'Small-outline diode (SC-79), 1.2mm x 0.8mm x 0.65mm' },
  SOD723: { name: 'SOD-723', description: 'Small-outline diode, 1.0mm x 0.6mm x 0.65mm' },
  SOD923: { name: 'SOD-923', description: 'Small-outline diode, 0.8mm x 0.6mm x 0.4mm' },
  
  DO214AA: { name: 'DO-214AA', description: 'Rectifier package, 5.4mm x 3.6mm x 2.65mm' },
  DO214AB: { name: 'DO-214AB', description: 'Rectifier package, 7.95mm x 5.9mm x 2.25mm' },
  DO214AC: { name: 'DO-214AC', description: 'Rectifier package, 5.2mm x 2.6mm x 2.15mm' },

  BGA: { name: 'BGA', description: 'Ball grid array' },
  LGA: { name: 'LGA', description: 'Land grid array' },
  CGA: { name: 'CGA', description: 'Column-grid array' },
  LLP: { name: 'LLP', description: 'Leadless lead-frame package' },
  CCGA: { name: 'CCGA', description: 'Ceramic column-grid array' },
  CQGP: { name: 'CQGP', description: 'Ceramic quad grid array package' },
  PGA: { name: 'PGA', description: 'Pin-grid array' },
  CPGA: { name: 'CPGA', description: 'Ceramic pin-grid array' },
  CSP: { name: 'CSP', description: '' },
  MCM: { name: 'MCM', description: 'Multi-chip module' },
  LCC: { name: 'LCC', description: 'Leadless chip carrier' },
  PLCC: { name: 'PLCC', description: 'Plastic leaded chip carrier' },
  DLCC: { name: 'DLCC', description: 'Dual leadless chip carrier' },
  CLCC: { name: 'CLCC', description: 'Ceramic leadless chip carrier' },
  CBGA: { name: 'CBGA', description: '' },
  CFP: { name: 'CFP', description: 'Ceramic flat-pack' },
  CQFP: { name: 'CQFP', description: 'Ceramic quad flat-pack' },
  BCFP: { name: 'BCFP', description: 'Bumpered quad flat-pack' },
  DFN: { name: 'DFN', description: 'Dual flat-pack' },
  QFN: { name: 'QFN', description: 'Quad flat-pack no-leads' },
  QFP: { name: 'QFP', description: 'Quad flat-pack' },
  BQFP: { name: 'BQFP', description: 'Bumpered quad flat-pack' },
  LQFP: { name: 'LQFP', description: 'Low-profile quad flat-pack' },
  TQFP: { name: 'TQFP', description: 'Thin quad flat-pack' },
  VQFP: { name: 'VQFP', description: 'Very-thin quad flat-pack' },
  TQFN: { name: 'TQFN', description: 'Thin quad flat-pack no-leads' },
  VQFN: { name: 'VQFN', description: 'Very-thin quad flat-pack no-leads' },
  WQFN: { name: 'WQFN', description: 'Very-very-thin quad flat-pack no-leads' },
  UQFN: { name: 'UQFN', description: 'Ultra-thin quad flat-pack no-leads' },
  ODFN: { name: 'ODFN', description: 'Optical dual flat no-leads' },
  WLCSP: { name: 'WLCSP', description: 'Wafer level chip scale package' },

  TO3: { name: 'TO-3', description: 'Transistor outline package' },
  TO5: { name: 'TO-5', description: 'Transistor outline package, metal/radial' },
  TO18: { name: 'TO-18', description: 'Transistor outline package, metal/radial' },
  TO39: { name: 'TO-39', description: 'Transistor outline package, short leads' },
  TO46: { name: 'TO-46', description: 'Transistor outline package, low height' },
  TO66: { name: 'TO-66', description: 'Transistor outline package, small' },
  TO92: { name: 'TO-92', description: 'Transistor outline package, plastic, 3-pin' },
  TO99: { name: 'TO-99', description: 'Transistor outline package, metal, 8-pin' },
  TO100: { name: 'TO-100', description: 'Transistor outline package, metal, 10-pin' },
  TO126: { name: 'TO-126', description: 'Transistor outline package, plastic, 3-pin' },
  TO220: { name: 'TO-220', description: 'Transistor outline package, 3-pin' },
  TO226: { name: 'TO-226', description: 'Transistor outline package' },
  TO247: { name: 'TO-247', description: 'Transistor outline package, plastic, 3-pin' },
  TO251: { name: 'TO-251', description: 'Transistor outline package (IPAK)' },
  TO252: { name: 'TO-252', description: 'Transistor outline package (SOT428, DPAK)' },
  TO262: { name: 'TO-262', description: 'Transistor outline package (I2PAK)' },
  TO263: { name: 'TO-263', description: 'Transistor outline package (D2PAK)' },
  TO274: { name: 'TO-274', description: 'Transistor outline package (Super-247)' },
};

export const BarcodeProfiles = {
  Default: 0
};

export const DEFAULT_FONT = "Segoe UI";

export const AccountTypes = {
  Normal: 0,
  Admin: 1,
  SuperAdmin: 2
};

export const SubscriptionLevels = {
  Free: 0,
  Maker: 1,
  Professional: 2,
  Commercial: 3
};

export const getAccountTypesLabel = (user) => {
  const accountTypes = [];
  if (user.isSuperAdmin) accountTypes.push('Super Admin');
  if (user.isAdmin) accountTypes.push('Admin');
  if (accountTypes.length === 0)
    accountTypes.push('Basic');
  return accountTypes.join(', ');
};

export const getAccountTypeIcon = (user) => {
  if (user.isSuperAdmin) return (<Icon name="user secret" title="Super Admin" color="red" size="large" />);
  if (user.isAdmin) return (<Icon name="user secret" title="Admin" color="blue" size="large" />);
  return (<Icon name="user" title="Basic Account" />);
};

export const BooleanTypes = {
  False: false,
  True: true
};

/**
 * Gets the name of a type by value
 * @param {object} type Type to get
 * @param {number} value Value of the type
 * @returns The name of the type definition
 */
 export const GetTypeName = (type, value) => {
  const typeKeys = Object.keys(type);
  const typeValues = Object.values(type);
  const valueType = typeValues && typeValues.length > 0 ? typeValues[0] : "number";
  
  switch(typeof valueType){
    case "object":
      return _.findWhere(typeValues, { value: value })?.name;
    default:
      return typeKeys[value];
  }
};

/**
 * Gets the value of a type by name
 * @param {object} type Type to get
 * @param {string} name Value of the type
 * @returns The value of the type definition
 */
export const GetTypeValue = (type, name) => {
  const typeKeys = Object.keys(type);
  if (typeKeys.includes(name))
    return type[name];
  return -1;
};

/**
 * Gets the specified property of a type by value
 * @param {object} type Type to get
 * @param {number} value Value to match against
 * @param {string} propertyToGet The property to get
 * @param {string} valueProperty The name of the value property to match
 * @returns The value of parameter propertyToGet
 */
 export const GetTypeProperty = (type, value, propertyToGet = "name", valueProperty = "value") => {
  const typeKeys = Object.keys(type);
  const typeValues = Object.values(type);
  const valueType = typeValues && typeValues.length > 0 ? typeValues[0] : "number";
  switch(typeof valueType){
    case "object":
      let searchObj = {};
      searchObj[valueProperty] = value;
      const matchingType = _.find(typeValues, searchObj);
      if (matchingType)
        return matchingType[propertyToGet];
      return null;
    default:
      return typeKeys[value];
  }
};

/**
 * Gets the full object of a type by value
 * @param {object} type Type to get
 * @param {number} value Value to match against
 * @param {string} valueProperty The name of the value property to match
 * @returns {object} The type object that matched
 */
 export const GetTypeMeta = (type, value, valueProperty = "value") => {
  const typeKeys = Object.keys(type);
  const typeValues = Object.values(type);
  const valueType = typeValues && typeValues.length > 0 ? typeValues[0] : "number";
  switch(typeof valueType){
    case "object":
      const matchingType = _.find(typeValues, t => t[valueProperty] === value);
      if (matchingType)
        return matchingType;
      return null;
    default:
      return typeKeys[value];
  }
};

/**
 * Generate a dropdown from a type
 * @param {object} type The type to generate a dropdown for
 * @param {bool} includeEmptyOption True to generate a blank empty option
 * @param {number} keyIndex The key index to start at
 * @param {object} extraFields Extra fields to add
 * @returns Dropdown items array
 */
export const GetTypeDropdown = (type, includeEmptyOption = false, keyIndex = 1, extraFields = {}) => {
  const typeKeys = Object.keys(type);
  let options = [];
  if (includeEmptyOption)
    options.push({
      key: -1,
      value: -1,
      text: ''
    });
  const newOptions = typeKeys.map(t => {
    return {
      ...extraFields,
      key: type[t] + keyIndex,
      value: type[t],
      text: t
    };
  });
  for(let i = 0; i < newOptions.length; i++)
    options.push(newOptions[i]);
  return options;
};

/**
 * Generate a dropdown from a type
 * @param {object} type The type to generate a dropdown for
 * @param {bool} showDescription True to include the description if available
 * @param {number} keyIndex The key index to start at
 * @param {object} extraFields Extra fields to add
 * @returns Dropdown items array
 */
export const GetAdvancedTypeDropdown = (type, showDescription = false, keyIndex = 1, extraFields = {}) => {
  const typeKeys = Object.keys(type);

  return typeKeys.map((t, tkey) => {
    return {
      ...extraFields,
      key: (type[t].value || type[t].text || type[t].name || tkey) + keyIndex,
      value: (type[t].value || type[t].text || type[t].name),
      icon: type[t].icon,
      text: type[t].text || type[t].name,
      description: showDescription ? type[t].description : null,
      ...(type[t].flag && {flag: type[t].flag.toLowerCase() })
    };
  });
};