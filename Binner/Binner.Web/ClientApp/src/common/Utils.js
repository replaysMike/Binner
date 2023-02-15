import { MD5 as encryptMD5 } from "crypto-js";

/**
 * Encode a number to resistance value
 * @param {any} number the ohms value
 * @param {any} decimals the number of decimal places to display
 */
export const encodeResistance = (number, decimals = 0) => {
  const ohms = Number.parseFloat(number) || 0;
  if (ohms >= 1000 * 1000 * 1000)
    return `${(ohms / (1000 * 1000 * 1000)).toFixed(decimals)}GΩ`;
  else if (ohms >= 1000 * 1000)
    return `${(ohms / (1000 * 1000)).toFixed(decimals)}MΩ`;
  else if (ohms >= 1000)
    return `${(ohms / (1000)).toFixed(decimals)}kΩ`;
  return `${ohms.toFixed(decimals)}Ω`;
};

/**
 * Decode a resistor value into ohms
 * @param {any} str the resistance value, such as 4.7k
 */
export const decodeResistance = (str) => {
  str = str.toString().toLowerCase();
  if (str.indexOf('k') > 0) {
    return (Number.parseInt(str.replace('k', '')) * 1000);
  } else if (str.indexOf('m') > 0) {
    return (Number.parseInt(str.replace('m', '')) * 1000 * 1000);
  } else if (str.indexOf('g') > 0) {
    return (Number.parseInt(str.replace('g', '')) * 1000 * 1000 * 1000);
  }
  return Number.parseInt(str);
};

/**
 * Format a number with local currency format
 * @param {number} number Number to format
 * @param {number} maxDecimals The maximum number of decimals to return
 * @returns formatted number string
 */
export const formatCurrency = (number, maxDecimals = 5) => {
  return number.toLocaleString('en-US', {style: 'currency', currency: 'USD', maximumFractionDigits: maxDecimals});
}

/**
 * Format a number with thousand separators
 * @param {number} number Number to format
 * @returns formatted number string
 */
export const formatNumber = (number) => {
  // return number.toString().replace(/\B(?<!\.\d*)(?=(\d{3})+(?!\d))/g, ",");
  return number.toLocaleString();
}

/**
 * Compute an MD5 value
 * @param {string} val value to encode
 * @returns hash value
 */
export const MD5 = (val) => {
  return encryptMD5(val);
};

/**
 * Escapes a string as a regex
 * @param {string} string string expression to escape
 * @returns 
 */
export const escapeRegex = (string) => {
  return string.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
};

/**
 * Check if a string is numeric
 * @param {string} str String to parse
 * @returns true if string is a number
 */
export const isNumeric = (str) => {
  if (typeof str != "string") return false // we only process strings!  
  return !isNaN(str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
         !isNaN(parseFloat(str)) // ...and ensure strings of whitespace fail
};