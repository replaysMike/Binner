import _ from "underscore";
import { cloneDeep } from "lodash";

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
 * Get a list of parts that are in stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @param {object} pcb Pcb to count, null to get unassociated count
 * @returns 
 */
export const getInStockParts = (parts, pcb) => {
	if (parts === undefined) return 0;
	if (pcb)
		return _.filter(parts, (s) => s.pcbId === pcb.pcbId && (s.part?.quantity || s.quantityAvailable || 0) >= s.quantity);
	return _.filter(parts, (s) => s.pcbId === null && (s.part?.quantity || s.quantityAvailable || 0) >= s.quantity);
};

/**
 * Get a list of parts that are in stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getTotalInStockParts = (parts) => {
	if (parts === undefined) return 0;
	return _.filter(parts, (s) => (s.part?.quantity || s.quantityAvailable || 0) >= s.quantity);
};

/**
 * Get a list of parts that are out of stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @param {object} pcb Pcb to count, null to get unassociated count
 * @returns 
 */
export const getOutOfStockParts = (parts, pcb) => {
	if (parts === undefined) return 0;
	if (pcb)
		return _.filter(parts, (s) => s.pcbId === pcb.pcbId && (s.part?.quantity || s.quantityAvailable || 0) < s.quantity);
	return _.filter(parts, (s) => s.pcbId === null && (s.part?.quantity || s.quantityAvailable || 0) < s.quantity);
};

/**
 * Get a list of parts that are out of stock produce a single PCB by quantity needed
 * @param {array} parts Parts array from BOM list
 * @returns 
 */
export const getTotalOutOfStockParts = (parts) => {
	if (parts === undefined) return 0;
	return _.filter(parts, (s) => (s.part?.quantity || s.quantityAvailable || 0) < s.quantity);
};

/**
 * Compute the total number of PCBs that can be produced based on BOM list inventory not assigned to a PCB
 * @param {array} parts Parts array from BOM list
 * @returns The count of how many PCBs can be produced
 */
export const getProducibleUnassociatedCount = (parts) => {
	if (parts === undefined || parts.length === 0) return 0;
	const maxIteration = 10000;
	// deep clone the parts array
	const clonedParts = cloneDeep(parts);
	const partsConsumed = clonedParts.filter(x => x.pcbId === null).map(x => ({ quantity: x.quantity, part: { quantity: x.part?.quantity || x.quantityAvailable || 0 }}));
	let pcbsProduced = 0;
	let pcbsExceeded = false;
	for(let virtualPcb = 0; virtualPcb < maxIteration; virtualPcb++){
		for(let i = 0; i < partsConsumed.length; i++) {
			partsConsumed[i].part.quantity -= partsConsumed[i].quantity;
			if (partsConsumed[i].part.quantity < 0) {
				pcbsExceeded = true;
				break;
			}
		}
		if (pcbsExceeded)
			break;
		pcbsProduced++;
	}
	if (pcbsProduced === maxIteration) pcbsProduced = 0;
	return pcbsProduced;
};

/**
 * Consume the parts in a BOM part list for a given PCB, to determine if it can be produced
 * @param {array} inventory list of parts in inventory (used for referenced quantities) 
 * @param {array} inventoryByPcb list of inventory by each pcb in BOM
 * @param {object} pcb the target pcb from which parts will be consumed 
 * @param {number} quantity the quantity of boards to produce (default: 1)
 * @returns 
 */
export const consumeFromPartList = (inventory, inventoryByPcb, pcb, quantity) => {
	if (!quantity) quantity = 1;
	// get all parts for pcb
	const pcbParts = _.filter(inventoryByPcb, x => x.pcbId === pcb.pcbId);

	// using the original input parts array, modify quantities
	let pcbQuantity = pcb.quantity;
	// a value of 0 is invalid
	if (pcbQuantity <= 0) pcbQuantity = 1;
	for(let i = 0; i < pcbParts.length; i++) {
		const inventoryPart = _.find(inventory, p => p.partId === pcbParts[i].partId);
		const quantityRequired = pcbParts[i].quantity * pcbQuantity * quantity;
		const quantityAvailable = inventoryPart.quantity;
		if (quantityRequired > quantityAvailable) {
			return false; // not enough parts
		} else {
			// reduce inventory
			inventoryPart.quantity -= quantityRequired;
		}
	}

	// the target pcb can be produced
	return true;
};

/**
 * Compute the total number of PCBs that can be produced based on BOM list inventory
 * @param {array} parts Parts array from BOM list
 * @param {object} pcb The pcb to produce (optionally)
 * @returns The count of how many PCBs can be produced
 */
export const getProduciblePcbCount = (parts, pcb) => {
	if (parts === undefined || parts.count === 0) return { count: 0, limitingPcb: -1 };
	const maxIteration = 10000;

	// deep clone the parts array
	const clonedParts = cloneDeep(parts);
	const pcbParts = pcb && pcb.pcbId > 0 ? _.filter(clonedParts, p => p.pcbId === pcb.pcbId) : clonedParts;
	const partsConsumed = pcbParts.map(x => ({ quantity: x.quantity, part: { quantity: x.part?.quantity || x.quantityAvailable || 0, pcbId: x.pcbId }}));
	let pcbsProduced = 0;
	let pcbsExceeded = false;
	let limitingPcb = -1;
	for(let virtualPcb = 0; virtualPcb < maxIteration; virtualPcb++){
		for(let i = 0; i < partsConsumed.length; i++) {
			const pcbId = partsConsumed[i].part.pcbId;
			// if a pcb is provided, take into consideration the pcb quantity as well.
			// if the pcb has a quantity > 1, it acts as a multiplier for BOMs that produce multiples of a single PCB
			if (pcb && pcbId > 0) {
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
	if (pcbsProduced === maxIteration) pcbsProduced = 0;
	return { count: pcbsProduced, limitingPcb };
}

/**
 * Compute the total number of BOMs that can be produced based on BOM list inventory.
 * @param {array} parts Parts array from BOM list
 * @returns The count of how many BOMs can be produced
 */
export const getProducibleBomCount = (parts, pcbs) => {
	if (parts === undefined || parts.count === 0) {
		return { count: 0, limitingPcb: -1 };
	}
	const maxIteration = 10000;

	const clonedParts = cloneDeep(parts);
	const partsConsumed = clonedParts.map(x => ({ quantity: x.quantity, part: { quantity: x.part?.quantity || x.quantityAvailable || 0, pcbId: x.pcbId }}));
	let pcbsProduced = 0;
	let bomProduced = 0;
	let pcbsExceeded = false;
	let limitingPcb = -1;
	// virtualPcb is the max number of pcbs we will calculate that are available
	for(let virtualPcb = 0; virtualPcb < maxIteration; virtualPcb++){
		//console.debug('checking vpcb', virtualPcb, maxIteration, partsConsumed);
		for(let pcbNum = 0; pcbNum < pcbs.length; pcbNum++) {
			// iterate each pcb in the BOM
			let pcb = pcbs[pcbNum];
			
			for(let i = 0; i < partsConsumed.length; i++) {
				const pcbId = partsConsumed[i].part.pcbId;

				// if a pcb is provided, take into consideration the pcb quantity as well.
				// if the pcb has a quantity > 1, it acts as a multiplier for BOMs that produce multiples of a single PCB
				if (pcb && pcbId > 0) {
					let pcbQuantity = pcb.quantity;
					// a value of 0 is invalid
					if (pcbQuantity <= 0) pcbQuantity = 1;
					partsConsumed[i].part.quantity -= (partsConsumed[i].quantity * pcbQuantity);
				} else {
					// no pcb specified
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
		if (pcbsExceeded) 
			break;
		bomProduced++;
	}
	if (bomProduced === maxIteration) bomProduced = 0;

	return { count: bomProduced, pcbsCount: pcbsProduced, limitingPcb, partsConsumed };
};