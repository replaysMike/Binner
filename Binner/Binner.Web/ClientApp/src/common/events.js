
export const AppEvents = {

/**
 * Send an application event
 * @param {string} eventHandler Name of event to send
 */
	sendEvent: (eventHandler, args = null) => {
		document.body.dispatchEvent(new CustomEvent(eventHandler, { detail: args }));
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
	/** 
	 * A barcode input event
	*/
	BarcodeInput: 'barcodeInput',
	/** 
	 * A barcode is being read
	*/
	BarcodeReading: 'barcodeReading',
	/** 
	 * A barcode was received
	 */
	BarcodeReceived: 'barcodeReceived'
};
