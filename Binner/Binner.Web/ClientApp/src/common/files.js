/**
 * Format bytes as human-readable text.
 * 
 * @param bytes Number of bytes.
 * @param useSiUnits True to use metric (SI) units, aka powers of 1000. False to use 
 *           binary (IEC), aka powers of 1024.
 * @param decimalPlaces Number of decimal places to display.
 * 
 * @return Formatted string.
 */
export const humanFileSize = (bytes, useSiUnits = false, decimalPlaces = 1) => {
  const thresh = useSiUnits ? 1000 : 1024;

  if (Math.abs(bytes) < thresh) {
    return bytes + ' B';
  }

  const units = useSiUnits 
    ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'] 
    : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
  let u = -1;
  const r = 10**decimalPlaces;

  do {
    bytes /= thresh;
    ++u;
  } while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1);


  return bytes.toFixed(decimalPlaces) + ' ' + units[u];
};