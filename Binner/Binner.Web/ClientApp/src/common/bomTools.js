import _ from "underscore";

/**
 * Get the color for the project
 * @param {object} ProjectColors The project colors definition
 * @param {*} project The project
 * @returns 
 */
export const getProjectColor = (ProjectColors, project) => {
	return _.find(ProjectColors, (c) => c.value === project.color).name !== "" && { color: _.find(ProjectColors, (c) => c.value === project.color).name };
}

/**
 * Get a count of parts that are in stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getInStockPartsCount = (parts) => {
	return getInStockParts(parts).length;
};

/**
 * Get a list of parts that are in stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getInStockParts = (parts) => {
	if (parts === undefined) return 0;
	return _.filter(parts, (s) => (s.part?.quantity || s.quantityAvailable || 0) >= s.quantity);
};

/**
 * Get a count of parts that are out of stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getOutOfStockPartsCount = (parts) => {
	return getOutOfStockParts(parts).length;
};

/**
 * Get a list of parts that are out of stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getOutOfStockParts = (parts) => {
	if (parts === undefined) return 0;
	return _.filter(parts, (s) => (s.part?.quantity || s.quantityAvailable || 0) < s.quantity);
};

/**
 * Compute the total number of PCBs that can be produced based on BOM list inventory
 * @param {array} parts Parts array from BOM list
 * @param {object} pcb The pcb to produce (optionally)
 * @returns The count of how many PCBs can be produced
 */
export const getProduciblePcbCount = (parts, pcb) => {
	if (parts === undefined) return { count: 0, limitingPcb: -1 };
	const inStock = getInStockPartsCount(parts);
	const outOfStock = getOutOfStockPartsCount(parts);
	if (outOfStock > 0) {
		return { count: 0, limitingPcb: -1 };
	}
	if (inStock > 0) {
		// deep clone the parts array
		const partsConsumed = parts.map(x => ({ quantity: x.quantity, part: { quantity: x.part?.quantity || x.quantityAvailable || 0, pcbId: x.pcbId }}));
		let pcbsProduced = 0;
		let pcbsExceeded = false;
		let limitingPcb = -1;
		for(let pcb = 0; pcb < 10000; pcb++){
			for(let i = 0; i < partsConsumed.length; i++) {
				const pcbId = partsConsumed[i].part.pcbId;
				// if a pcb is provided, take into consideration the pcb quantity as well.
				// if the pcb has a quantity > 1, it acts as a multiplier for BOMs that produce multiples of a single PCB
				if (pcb && pcb.pcbId > 0) {
					let pcbQuantity = pcb.quantity;
					// a value of 0 is invalid
					if (pcbQuantity <= 0) pcbQuantity = 1;
					partsConsumed[i].part.quantity -= (partsConsumed[i].quantity * pcbQuantity);
				} else {
					partsConsumed[i].part.quantity -= partsConsumed[i].quantity;
				}
				if (partsConsumed[i].part.quantity < 0) {
					pcbsExceeded = true;
					limitingPcb = pcbId;
					break;
				}
			}
			if (pcbsExceeded)
				break;
			pcbsProduced++;
		}
		return { count: pcbsProduced, limitingPcb };
	}
	return { count: 0, limitingPcb: -1 };
};