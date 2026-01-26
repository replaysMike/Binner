export const resistorTerminators = ['o', 'k', 'K', 'n', 'm', 'M', 'g', 'G', 'Ω'];
export const capacitorTerminators = ['F', 'mF', 'mf', 'uF', 'uf', 'μf', 'μF', 'nF', 'nf', 'pF', 'pf'];

/**
 * Format a value as a resistor value
 * @param {string} value value to format
 * @returns formatted resistor value
 */
export const formatResistorValue = (value) => {
  const terminators = resistorTerminators;
  for (let t = 0; t < terminators.length; t++) {
    const terminator = terminators[t];
    if (value.endsWith(terminator) && !isNaN(value.substr(0, value.length - terminator.length))) {
      switch (terminator) {
        case 'n':
          return value = value.replace('n', 'nΩ');
        case 'm':
          return value = value.replace('m', 'mΩ');
        case 'o':
          return value = value.replace('o', 'Ω');
        case 'k':
          return value = value.replace('k', 'kΩ');
        case 'K':
          return value = value.replace('K', 'kΩ');
        case 'M':
          return value = value.replace('M', 'MΩ');
        case 'g':
          return value = value.replace('g', 'GΩ');
        case 'G':
          return value = value.replace('G', 'GΩ');
      }
    }
  }
  // no terminator provided, if it's a number append the ohms symbol
  if (!isNaN(value)) {
    return value + 'Ω';
  }
  // not a number, return original value
  return value;
};

/**
 * Format a value as a capacitor value
 * @param {string} value value to format
 * @returns formatted capacitor value
 */
export const formatCapacitorValue = (value) => {
  const terminators = capacitorTerminators;
  for (let t = 0; t < terminators.length; t++) {
    const terminator = terminators[t];
    if (value.endsWith(terminator) && !isNaN(value.substr(0, value.length - terminator.length))) {
      switch (terminator) {
        case 'F':
          return value;
        case 'mF':
          return value;
        case 'mf':
          return value = value.replace('mf', 'mF');
        case 'uF':
          return value = value.replace('uF', 'μF');
        case 'uf':
          return value = value.replace('uf', 'μF');
        case 'μf':
          return value = value.replace('μf', 'μF');
        case 'μF':
          return value;
        case 'nF':
          return value;
        case 'nf':
          return value = value.replace('nf', 'nF');
        case 'pF':
          return value;
        case 'pf':
          return value = value.replace('pf', 'Pf');
      }
    }
  }
  // no terminator provided, if it's a number append the ohms symbol
  if (!isNaN(value))
    return value + 'μf';
  // not a number, return original value
  return value;
};