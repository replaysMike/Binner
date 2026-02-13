import { MD5 as encryptMD5 } from "crypto-js";

/**
 * Decode url safe base64 encoded string
 */
export const decodeBase64 = (base64Url) => {
  var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  return decodeURIComponent(atob(base64));
};

/**
 * Copy/clone a string
 * @param {string} str input string
 * @returns copied string
 */
export const copyString = (str) => {
  return (' ' + str).slice(1); // force clone string
};

/**
 * Encode a number to resistance value
 * @param {any} number the ohms value
 * @param {any} decimals the number of decimal places to display
 */
export const encodeResistance = (number, decimals = 0) => {
  const ohms = Number.parseFloat(number) || 0;
  if (ohms >= 1000 * 1000 * 1000) return `${(ohms / (1000 * 1000 * 1000)).toFixed(decimals)}GΩ`;
  else if (ohms >= 1000 * 1000) return `${(ohms / (1000 * 1000)).toFixed(decimals)}MΩ`;
  else if (ohms >= 1000) return `${(ohms / 1000).toFixed(decimals)}kΩ`;
  return `${ohms.toFixed(decimals)}Ω`;
};

/**
 * Decode a resistor value into ohms
 * @param {any} str the resistance value, such as 4.7k
 */
export const decodeResistance = (str) => {
  str = str.toString().toLowerCase();
  if (str.indexOf("k") > 0) {
    return Number.parseInt(str.replace("k", "")) * 1000;
  } else if (str.indexOf("m") > 0) {
    return Number.parseInt(str.replace("m", "")) * 1000 * 1000;
  } else if (str.indexOf("g") > 0) {
    return Number.parseInt(str.replace("g", "")) * 1000 * 1000 * 1000;
  }
  return Number.parseInt(str);
};

/**
 * Format a number with local currency format
 * @param {number} number Number to format
 * @param {string} currency Currency to use, default: 'USD'
 * @param {number} maxDecimals The maximum number of decimals to return, default: 5
 * @returns formatted number string
 */
export const formatCurrency = (number, currency = 'USD', maxDecimals = 5) => {
  if (!number || typeof number !== "number") number = 0;
  const locale = getLocaleLanguage();
  return number.toLocaleString(locale, { style: "currency", currency: currency, maximumFractionDigits: maxDecimals });
};

/**
 * Get the currency symbol
 * @param {string} currency Currency to use, default: 'USD'
 * @returns formatted number string
 */
export const getCurrencySymbol = (currency = 'USD') => {
  const locale = getLocaleLanguage();
  if (!locale) return '';
  return (0).toLocaleString(locale, { style: 'currency', currency, minimumFractionDigits: 0, maximumFractionDigits: 0 }).replace(/\d/g, '').trim();
};
  

/**
 * Get the locale language of the browser
 * @returns language string
 */
export const getLocaleLanguage = () => { 
  const defaultLanguage = "en-US";
  try {
    const uiLanguage = localStorage.getItem('i18nextLng') || defaultLanguage;
    // if there is a ui language specified, and it's not the default english use it.
    if (uiLanguage !== 'en') return uiLanguage;
    
    // use the browser locale
    return (navigator.languages && navigator.languages.length) ? navigator.languages[0] : navigator.language;
  } catch {
    return defaultLanguage;
  }
};

/**
 * Format a number with thousand separators
 * @param {number} number Number to format
 * @returns formatted number string
 */
export const formatNumber = (number) => {
  return number.toLocaleString();
};

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
  return string.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&");
};

/**
 * Check if a string is numeric
 * @param {string} str String to parse
 * @returns true if string is a number
 */
export const isNumeric = (str) => {
  if (typeof str != "string") return false; // we only process strings!
  return (
    !isNaN(str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
    !isNaN(parseFloat(str))
  ); // ...and ensure strings of whitespace fail
};

/**
 * Generate a random password
 * @returns password string
 */
export const generatePassword = () => {
  var length = 8,
      charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$",
      retVal = "";
  for (var i = 0, n = charset.length; i < length; ++i) {
      retVal += charset.charAt(Math.floor(Math.random() * n));
  }
  return retVal;
};

/** Simple async sleep */
export const sleep = (ms) => {
  return new Promise((resolve) => setTimeout(resolve, ms));
};

/**
 * Returns true of strings are equal, with optional case insensitive support
 * @param {string} a string a
 * @param {string} b string b
 * @param {boolean} caseInsensitive True to perform a case insensitive compare
 * @returns true if strings are the same
 */
export const equals = (a, b, caseInsensitive = false) => {
  if (caseInsensitive === true)
    return typeof a === 'string' && typeof b === 'string'
      ? a.localeCompare(b, undefined, { sensitivity: 'accent' }) === 0
      : a === b;
  return a === b;
};