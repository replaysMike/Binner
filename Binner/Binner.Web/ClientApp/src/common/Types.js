import _ from "underscore";
import { ResistorIcon } from "./icons";

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

export const BarcodeProfiles = {
  Default: 0
};

export const DEFAULT_FONT = "Segoe UI";

export const AccountTypes = {
  Normal: false,
  Admin: true
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
      const matchingType = _.findWhere(typeValues, { value: value });
      return matchingType.name;
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
      let searchObj = {};
      searchObj[valueProperty] = value;
      const matchingType = _.find(typeValues, searchObj);
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

  return typeKeys.map(t => {
    return {
      ...extraFields,
      key: type[t].value + keyIndex,
      value: type[t].value,
      icon: type[t].icon,
      text: type[t].text || type[t].name,
      description: showDescription ? type[t].description : null,
      ...(type[t].flag && {flag: type[t].flag.toLowerCase() })
    };
  });
};