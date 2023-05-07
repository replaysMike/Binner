import _ from "underscore";


export const updateStateItem = (existingItems, newItem, newItemKeyName, newItemKeyValue) => {
	const existingItemsShallowCopy = [...existingItems];
	const newItems = existingItemsShallowCopy.map((i, ikey) => {
		if (i[newItemKeyName] === newItemKeyValue) {
			const keys = Object.keys(i);
			// mutate all properties with the new value
			for(let x = 0; x < keys.length; x++) {
				i[keys[x]] = newItem[keys[x]];
			}
			return i;
		}
		return i;
	});
	return newItems;
};