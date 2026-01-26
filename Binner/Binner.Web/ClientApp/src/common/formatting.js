export const resistorTerminators = ['o', 'k', 'K', 'n', 'm', 'M', 'g', 'G', 'Ω'];
export const capacitorTerminators = ['F', 'f', 'mF', 'mf', 'uF', 'uf', 'μf', 'μF', 'UF', 'nF', 'nf', 'pF', 'pf'];
export const inductorTerminators = ['H', 'h', 'mH', 'mh', 'uh', 'uH', 'µH', 'UH', 'nH', 'nh', 'pH', 'ph'];

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
        case 'f':
          return value.replace('f', 'F'); // farad
        case 'F':
          return value; // farad
        case 'mF':
          return value; // milifarad
        case 'mf':
          return value = value.replace('mf', 'mF'); // milifarad
        case 'uF':
          return value = value.replace('uF', 'μF'); // microfarad
        case 'uf':
          return value = value.replace('uf', 'μF'); // microfarad
        case 'UF':
          return value = value.replace('UF', 'μF'); // microfarad
        case 'μf':
          return value = value.replace('μf', 'μF'); // microfarad
        case 'μF':
          return value; // microfarad
        case 'nF':
          return value; // nanofarad
        case 'nf':
          return value = value.replace('nf', 'nF'); // nanofarad
        case 'pF':
          return value; // picofarad
        case 'pf':
          return value = value.replace('pf', 'pF'); // picofarad
      }
    }
  }
  // no terminator provided, if it's a number append the symbol
  if (!isNaN(value))
    return value + 'μF'; // microfarad
  // not a number, return original value
  return value;
};

/**
 * Format a value as a inductor value
 * @param {string} value value to format
 * @returns formatted inductor value
 */
export const formatInductorValue = (value) => {
  const terminators = inductorTerminators;
  for (let t = 0; t < terminators.length; t++) {
    const terminator = terminators[t];
    if (value.endsWith(terminator) && !isNaN(value.substr(0, value.length - terminator.length))) {
      switch (terminator) {
        case 'h':
          return value.replace('h', 'H'); // henry
        case 'H':
          return value; // henry
        case 'mh':
          return value.replace('mh', 'mH'); // millahenry
        case 'mH':
          return value; // millahenry
        case 'uH':
          return value = value.replace('uH', 'μH'); // microhenry
        case 'uh':
          return value = value.replace('uh', 'μH'); // microhenry
        case 'UH':
          return value = value.replace('UH', 'μH'); // microhenry
        case 'μh':
          return value = value.replace('μh', 'μH'); // microhenry
        case 'μH':
          return value; // microhenry
        case 'nH':
          return value; // nanohenry
        case 'nh':
          return value = value.replace('nh', 'nH'); // nanohenry
        case 'pH':
          return value; // picohenry
        case 'ph':
          return value = value.replace('ph', 'pH'); // picohenry
      }
    }
  }
  // no terminator provided, if it's a number append the symbol
  if (!isNaN(value))
    return value + 'μH'; // microhenry
  // not a number, return original value
  return value;
};