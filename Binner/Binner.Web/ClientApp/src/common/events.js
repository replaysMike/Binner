
export const AppEvents = {

/**
 * Send an application event
 * @param {string} eventHandler Name of event to send
 */
	sendEvent: (eventHandler) => {
		document.body.dispatchEvent(new CustomEvent(eventHandler));
	}

};

export const Events = {
	/**
	 * Disable barcode input scanning.
	 * Barcode scanning takes over all keyboard input, and this can be undesirable when focused on input elements.
	 */
	DisableBarcodeInput: 'disableBarcodeInput',
	/**
	 * Restore barcode input scanning
	 */
	RestoreBarcodeInput: 'restoreBarcodeInput',
};
