
export const AppEvents = {

/**
 * Send an application event
 * @param {string} eventHandler Name of event to send
 * @param {object} args Optional arguments, stored in the detail portion of the event
 * @param {string} source The source that initiated the event
 * @param {string} destination The destination the event should be received by
 */
	sendEvent: (eventHandler, args = null, source = null, destination = null) => {
		const parameters = {
			...args,
			source: source, 
			destination: destination
		}
		document.body.dispatchEvent(new CustomEvent(eventHandler, {detail: parameters }));
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
	 * A barcode read operation was cancelled
	 */
	BarcodeReadingCancelled: 'barcodeReadingCancelled',
	/** 
	 * A barcode was received
	 */
	BarcodeReceived: 'barcodeReceived'
};
