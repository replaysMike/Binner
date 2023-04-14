import _ from "underscore";
import { ResistorIcon, ICIcon, CapacitorIcon, LEDIcon } from "./icons";

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

/**
 * Get icon of part type
 * @param {string} name Name of part type
 * @param {string} parentName Name of parent type
 * @returns 
 */
export const getIcon = (name, parentName) => {
	console.log('name', name, parentName);
	let icon = getIconForType(name);
	if (!icon && parentName) {
		icon = getIconForType(parentName);
	}

	if (!icon) return ICIcon;
	return icon;
};

const getIconForType = (name) => {
	switch(name){
		case "Resistor":
			return ResistorIcon;
		case "Capacitor":
			return CapacitorIcon;
		case "LED":
			return LEDIcon;
		case "IC":
			return ICIcon;
		default:
			return null;
	}
};