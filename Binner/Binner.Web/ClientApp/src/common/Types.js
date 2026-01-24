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
  SMD201: { name: '201', description: '0.6mm x 0.3mm' },
  SMD402: { name: '402', description: '1.0mm x 0.5mm' },
  SMD603: { name: '603', description: '1.6mm x 0.8mm' },
  SMD805: { name: '805', description: '2.0mm x 1.25mm' },
  SMD1206: { name: '1206', description: '3.2mm x 1.6mm' },
  SMD1210: { name: '1210', description: '3.2mm x 2.5mm' },
  SMD1812: { name: '1812', description: '4.5mm x 3.2mm' },
  SMD2010: { name: '2010', description: '5.0mm x 2.5mm' },
  SMD2512: { name: '2512', description: '6.3mm x 3.2mm' },
  SMD102: { name: '102', description: '2.2mm x 1.1mm' },
  SMD204: { name: '204', description: '3.6mm x 1.4mm' },
  SMD207: { name: '207', description: '5.8mm x 2.2mm' },
  SMD1008: { name: '1008', description: '2.5mm x 2.0mm' },
  SMD1806: { name: '1806', description: '4.5mm x 1.6mm' },
  SMD1825: { name: '1825', description: '4.5mm x 6.4mm' },
  SMD2920: { name: '2920', description: '7.4mm x 5.1mm' },
  DIP4: { name: 'DIP4', description: '4.6mm x 7.62mm' },
  DIP8: { name: 'DIP8', description: '9.66mm x 7.62mm' },
  DIP14: { name: 'DIP14', description: '19.75mm x 7.62mm' },
  DIP16: { name: 'DIP16', description: '19.75mm x 7.62mm' },
  DIP20: { name: 'DIP20', description: '25.1mm x 7.62mm' },
  DIP24: { name: 'DIP24', description: '32.0mm x 15.24mm' },
  DIP28: { name: 'DIP28', description: '39.75mm x 15.24mm' },
  DIP40: { name: 'DIP40', description: '52.07mm x 15.24mm' },
  DIP64: { name: 'DIP64', description: '82.5mm x 22.86mm' },
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