export const BarcodeLabelTypes = {
  /** Label encodes data fields delimited by a character */
  Delimited: 0,
  /** Label encodes data fields using tokens at the start of the string to indicate the type of data */
  Tokenized: 1,
  /** Label encodes data fields using a fixed width size for each field */
  FixedWidth: 2,
  /** Multiple label types are handled */
  Multiple: 3
};