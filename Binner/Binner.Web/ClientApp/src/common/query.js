
/**
 * Get a variable from the query string
 * @param {any} query the full querystring
 * @param {any} variable the variable to get
 */
export const getQueryVariable = (query, variable) => {
  const queryStr = query.replace('?', '');
  var pairs = queryStr.split("&");
  for (var i = 0; i < pairs.length; i++) {
    var pair = pairs[i].split("=");
    if (pair[0] === variable && pair.length > 1) return pair[1];
  }
  return false;
};
