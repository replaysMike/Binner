import _ from "underscore";


/**
 * Get the part type id using its name
 * @param {string} partType Name of part type
 * @param {array} partTypes Array of part type objects
 */
export const getPartTypeId = (partType, partTypes) => {
	const item = _.find(partTypes, i => i.text === partType);
	if (item)
		return item.value;
	return null;
};