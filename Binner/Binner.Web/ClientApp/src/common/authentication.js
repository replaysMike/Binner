import { fetchApi } from "./fetchApi";

export const EmptyUserAccount = {
  userId: 0,
  isAuthenticated: false,
  name: "",
  accessToken: "",
	imagesToken: "",
	isAdmin: false,
};

/**
 * Decode a Jwt string
 * @param {string} token the Jwt encoded token
 * @returns decoded Jwt object
 */
export const decodeJwt = (token) => {
	var base64Url = token.split('.')[1];
	var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
	var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
			return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
	}).join(''));

	return JSON.parse(jsonPayload);
};

/**
 * Get the authenticated user account
 * @returns {any} the user account object
 */
export const getUserAccount = () => {
  const user = localStorage.getItem("user");
  return (user && JSON.parse(user)) || EmptyUserAccount;
};

/**
 * Get the bearer jwt token for authenticated user
 * @returns {string} the jwt bearer token
 */
 export const getAuthToken = () => {
  const user = getUserAccount();
  return user.accessToken || "";
};

/**
 * Get the image access token for authenticated user
 * @returns {string} the image access token
 */
export const getImagesToken = () => {
  const user = getUserAccount();
  return user.imagesToken || "";
};

/**
 * Set the authenticated user account
 * @param {any} the user account object
 */
export const setUserAccount = (user) => {
  localStorage.setItem("user", JSON.stringify(user));
};

/**
 * Tell the UI there is no authenticated user.
 * This is used when we receive a 401 from the server.
 * @returns {any} the request promise
 */
export const deAuthenticateUserAccount = () => {
  localStorage.removeItem("user");
};

/**
 * Ask for a new token using the refresh_token (http-only cookie).
 * If successful, replay the original XHR request.
 * If unsuccessful, deAuthenticate user and require a login.
 * @returns {any} the request promise
 */
 export const refreshTokenAuthorizationAsync = async (response, requestContext) => {
	// ask for a new token
	const fetchResponse = await fetch("/api/authentication/refresh-token", {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		  },
	}).then(async (newResponse) => {
		// we consider either a 401, or a message with isAuthenticated=false as a failure to refresh the token.
		if (newResponse.status === 200) {
			const newResponseData = await newResponse.clone().json();
			// we have a new token
			if (newResponseData.isAuthenticated) {
				// use the new token
				let userAccount = getUserAccount();
				userAccount.accessToken = newResponseData.jwtToken;
				userAccount.imagesToken = newResponseData.imagesToken;
				setUserAccount(userAccount);

				// re-issue the original request and return it
        		console.debug('reissuing original request', requestContext);
				// todo: this part is a little wonky and could be refactored. because fetchApi returns json() body and not the response,
				// we need to get it from our predefined response structure.
				const finalResult = await fetchApi(requestContext.url, requestContext.data, true);
				return finalResult.responseObject;
			} else {
        		console.error('failed to refresh token', newResponseData.message);
			}
		}else if (newResponse.status === 401 || newResponse.status === 403) {
			// failed to refresh token
      		console.error('failed to refresh token 401', newResponse);
		}

		return newResponse;
	});
	return fetchResponse;
  };

/**
 * Logout the current user
 * @returns {any} the request promise
 */
export const logoutUserAccountAsync = async () => {
  return await fetchApi("/api/authentication/logout", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((_) => {
    deAuthenticateUserAccount();
  });
};

/**
 * Check if the user is authenticated
 * @returns {boolean} true if the user is authenticated
 */
export const isAuthenticated = () => {
  const user = getUserAccount();
  return user && user.isAuthenticated;
};

/**
 * Check if the user is an admin
 * @returns {boolean} true if the user is an admin user
 */
 export const isAdmin = () => {
  const user = getUserAccount();
  return user && user.isAdmin;
};

/**
 * Get the subscription level of the user
 * @returns {number} subscription level
 */
export const getSubscriptionLevel = () => {
  const user = getUserAccount();
  return user && user.subscriptionLevel;
};

/**
 * Get the subscription tag name of the user's subscription
 * @returns {string} free, maker, pro
 */
export const getSubscriptionTag = (includeFree = false) => {
  const user = getUserAccount();
  if (user) {
    switch(user.subscriptionLevel) {
      case 0:
        if (includeFree) return 'free';
        return '';
      case 1:
        return 'maker';
      case 2:
        return 'pro';
    }
  }
  return '';
};

/**
 * Handle navigating a link requiring a given subscription level
 * @param {number} requiredSubscriptionLevel The subscription level required to visit link
 * @param {function} navigateFunc navigate function call
 */
export const handleSubscriptionLink = (e, requiredSubscriptionLevel, navigateFunc) => {
  const subscriptionLevel = getSubscriptionLevel();
  if (subscriptionLevel >= requiredSubscriptionLevel) {
    navigateFunc();
  }
};