import { getUserAccount, deAuthenticateUserAccount, refreshTokenAuthorizationAsync } from "./authentication";
import _ from "underscore";
import customEvents from './customEvents';
const noData = { message: `No message was specified.` };

/**
 * Fetch data from an Api.
 * Automatically includes jwt bearer authentication credentials and handles token refresh requests.
 * @param {string} url the url to fetch
 * @param {any} data the fetch request object
 * @param {bool} isReissuedRequest true if this is a reissued request (via refresh-token fetch)
 * @returns json response from api with an embedded response object
 */
export const fetchApi = async (url, data = { method: "GET", headers: {}, body: null, catchErrors: false }, isReissuedRequest = false) => {
  const userAccount = getUserAccount();

  let headers = data.headers || {};

  if (!headers["Content-Type"]) headers["Content-Type"] = "application/json";

  if (userAccount && userAccount.isAuthenticated && userAccount.accessToken.length > 0) {
    // authenticated users require a bearer token
    headers["Authorization"] = `Bearer ${userAccount.accessToken}`;
  }
  const fetchData = {
    ...data,
    method: data.method || "GET",
    headers: headers
  };
  if (fetchData.method.toUpperCase() !== "GET" && data.body) {
    // is body already stringified?
    if (!_.isString(data.body) && headers["Content-Type"] === "application/json") {
      fetchData["body"] = JSON.stringify(data.body);
    } else {
      fetchData["body"] = data.body;
    }
  }

  // store the request we want to send, in case we need to reissue it when Jwt token expires
  const rootUrl = !url.startsWith('/') ? '/' + url : url;
  const requestContext = {
    url: rootUrl,
    data: fetchData
  };

  const responseBody = await fetch(requestContext.url, requestContext.data)
    .then((response) => {
      return handleJsonResponse(response, requestContext, isReissuedRequest);
    })
    .catch((response) => {
      // opportunity to handle errors internally
      if (data.catchErrors) {
        // swallow errors and let the caller handle them
        return response;
      } else return Promise.reject(response);
    });
  return responseBody;
};

/**
 * Get the json response from a fetch operation
 * @param {any} response the response from fetch
 * @param {bool} requestContext the request context
 * @param {bool} isReissuedRequest true if this is a reissued request (via refresh-token fetch)
 * @returns {any} the response json object
 */
export const handleJsonResponse = async (response, requestContext, isReissuedRequest) => {
  // store the last version header we have seen
  if (response.headers.has("x-version")) {
    const version = response.headers.get("x-version");
    window.version = version;
    customEvents.notifySubscribers("version", { version });
  }
  const wrappedResponse = await handle401UnauthorizedAsync(response, requestContext, isReissuedRequest);
  if (!wrappedResponse.ok) return wrappedResponse.response;

  // valid response
  if (response.bodyUsed) {
    return response;
  } else {
    const data = isJson(wrappedResponse.response) && wrappedResponse.response.json ? await wrappedResponse.response.clone().json() : noData;
    // wrap the result in the final data, but include the response as `responseObject`
    const wrappedData = wrapReturn(data, wrappedResponse.response);
    return wrappedData;
  }
};

/**
 * Get the response from a fetch operation
 * @param {any} response the response from fetch
 * @param {bool} requestContext the request context
 * @param {bool} isReissuedRequest true if this is a reissued request (via refresh-token fetch)
 * @returns {any} the response binary object
 */
export const handleBinaryResponse = async (response, requestContext, isReissuedRequest) => {
  const wrappedResponse = await handle401UnauthorizedAsync(response, requestContext, isReissuedRequest);
  if (!wrappedResponse.ok) return wrappedResponse.response;

  // valid response
  const data = response.arrayBuffer ? await response.clone().arrayBuffer() : noData;
  return wrapReturn(data, response);
};

/**
 * An empty handler for completing promises from error handling
 * @param {any} response the response from fetch
 */
export const doNothing = async (response) => {
  return response;
};

/**
 * Get a list of errors from the response
 * @param {*} response the response object
 * @returns array of errors
 */
export const getErrors = (response) => {
  const errors = [];
  if (response.data && response.data.errors) {
    const keys = Object.keys(response.data.errors);

    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      for (let x = 0; x < response.data.errors[key].length; x++) {
        errors.push({ field: key, value: response.data.errors[key][x] });
      }
    }
  }
  return errors;
};

/**
 * Get a list of errors as a single string
 * @param {*} response the response object
 * @returns a string containing all the errors
 */
export const getErrorsString = (response) => {
  const errors = getErrors(response);
  const errorString = errors
    .map((err) => {
      return err.field + ": " + err.value;
    })
    .join("\n");
  return errorString;
};

/**
 * Get a raw text string from the response
 * @param {*} response the response object
 * @returns a string containing the message
 */
export const getText = async (response) => {
  return await response.text();
};

/**
 * Handle a 401 unauthorized message from a fetch request.
 * This handles the process required to refresh jwt tokens when necessary and reissue the original request
 * @param {any} response the response object from fetch
 * @param {any} requestContext the original request being sent
 * @param {bool} isReissuedRequest true if this is a reissued request (via refresh-token fetch)
 * @returns new reissued response, or original response
 */
const handle401UnauthorizedAsync = async (response, requestContext, isReissuedRequest) => {
  if (response.status === 401 || response.status === 403) {
    // unauthorized
    if (isReissuedRequest) {
      deAuthenticateUserAccount();
      window.location.replace("/login");
      return {
        ok: false,
        response: Promise.reject(response)
      };
    }

    // custom refresh token interceptor blocks requests when a refresh-token is required.
    // alternatives are axios.interceptors.response, or a convoluted react solution but this works well with little overhead.
    if (!window.tokenInterceptor) {
      window.tokenInterceptor = new Promise(async (tokenRenewalRequestSuccess, rejectTokenRenewalRequest) => {
        // unauthorized, try to request a new JWT refresh token
        // if successful, refreshTokenAuthorizationAsync will re-issue the original request
        const reissuedOriginalRequestResponse = await refreshTokenAuthorizationAsync(response, requestContext);
        const reissuedOriginalRequestResponseData = await reissuedOriginalRequestResponse.clone().json();
        if (reissuedOriginalRequestResponse.status === 401 || reissuedOriginalRequestResponseData.status === 403 || reissuedOriginalRequestResponseData.isAuthenticated === false) {
          // cannot get a new token, user must re-login
          deAuthenticateUserAccount();
          window.location.replace("/login");
          rejectTokenRenewalRequest();
        }

        // refresh token renewal succeeded, return the re-issued original request response
        tokenRenewalRequestSuccess(reissuedOriginalRequestResponse);
        return reissuedOriginalRequestResponse;
      });
      response = await window.tokenInterceptor;
      // reset the tokenInterceptor to null so we don't keep queueing requests
      window.tokenInterceptor = null;
    } else {
      // someone has already requested a new refresh token, this request must wait until the refresh token response is received
      // wait for the promise to finish
      const queuedRequestResponse = await window.tokenInterceptor.then((reissuedResponse) => {
        // we are now unblocked and can continue with re-issuing our request
        const originalRequestThatWonRefresh = reissuedResponse; // we don't care about this
        const myRequestThatHasBeenWaiting = requestContext;
        // return the queued request to be retried
        return myRequestThatHasBeenWaiting;
      }, (failed) => {
        // attempt to refresh jwt token failed, all queued requests are to be aborted.
        console.error('Request aborted, re-login required.', failed);
        return null;
      });
      if (queuedRequestResponse) {
        // retry the queued request
        const reissuedQueuedRequestResponse = await fetchApi(queuedRequestResponse.url, queuedRequestResponse.data, true);
        response = reissuedQueuedRequestResponse.responseObject;
      }
    } 
  }

  if (!response.ok) {
    if (response.status === 426) {
      // response has an licensing error, we want to display a licensing error modal box
      return {
        ok: false,
        response: await invokeLicensingErrorHandler(response)
      };
    } else if (response.status >= 404) {
      // response has an error, we want to display a error modal box
      return {
        ok: false,
        response: await invokeErrorHandler(response)
      };
    }
  }

  // return the response
  return {
    ok: true,
    response
  };
};

/**
 * Check if the response is json
 * @param {any} response
 * @returns {boolean} true if the response is json
 */
const isJson = (response) => {
  var validJsonTypes = ["application/json", "application/problem+json"];
  return validJsonTypes.some((h) => (response.headers.get("content-type") !== null ? response.headers.get("content-type").includes(h) : false));
};

const wrapReturn = (returnData, response) => {
  const data = {
    data: returnData,
    // also return the full response object
    responseObject: response
  };
  return data;
};

const invokeErrorHandler = async (response) => {
  let responseObject = {
    message: `Status: ${response.status} - No message was specified.`
  };
  if (isJson(response) && !response.bodyUsed) {
    responseObject = await response.json();
  }

  const errorObject = {
    url: response.url,
    status: response.status,
    ...responseObject
  };
  if (window.showErrorWindow) window.showErrorWindow(errorObject);
  // return the original response along with the responseObject that was read
  return Promise.reject(wrapReturn(responseObject, response));
};

const invokeLicensingErrorHandler = async (response) => {
  let responseObject = {
    message: `Status: ${response.status} - No message was specified.`
  };
  if (isJson(response) && !response.bodyUsed) {
    responseObject = await response.json();
  }

  const errorObject = {
    url: response.url,
    status: response.status,
    ...responseObject
  };
  if (window?.showLicenseErrorWindow) window.showLicenseErrorWindow(errorObject);
  // return the original response along with the responseObject that was read
  return Promise.reject(wrapReturn(responseObject, response));
};
