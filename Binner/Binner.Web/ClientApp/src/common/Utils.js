
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