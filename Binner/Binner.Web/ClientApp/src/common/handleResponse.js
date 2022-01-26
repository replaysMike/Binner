let hasError = false;
const noData = {};

/**
 * Get the json response from a fetch operation
 * @param {any} response the response from fetch
 * @returns {any} the response json object
 */
export const HandleJsonResponse = async (response) => {
	hasError = !response.ok;
	if(hasError) await InvokeErrorHandler(response);
	return response.ok && response.json ? response.json() : noData;
}

/**
 * Get the response from a fetch operation
 * @param {any} response the response from fetch
 * @returns {any} the response binary object
 */
export const HandleBinaryResponse = async (response) => {
	hasError = !response.ok;
	if(hasError) await InvokeErrorHandler(response);
	return response.ok && response.arrayBuffer ? response.arrayBuffer() : noData;
}


const InvokeErrorHandler = async (response) => {
	const json = await response.json();
	const responseWithBody = {
		url: response.url,
		status: response.status,
		...json
	};
	window.showErrorWindow(responseWithBody);
};