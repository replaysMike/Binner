import { fetchApi } from "./fetchApi";
import _ from "underscore";

let GLOBAL_APPSETTINGS = {};

/**
 * Get the system settings (cached)
 * @returns the system settings
 */
export const getSystemSettings = async (force) => {
	if (force || _.isEmpty(GLOBAL_APPSETTINGS)) {
		return await fetchApi("api/system/settings", {
			method: "GET",
			headers: {
				"Content-Type": "application/json",
			},
		}).then((response) => {
			const { data } = response;
			GLOBAL_APPSETTINGS = data;
			return GLOBAL_APPSETTINGS;
		});
	} else {
		return GLOBAL_APPSETTINGS;	
	}
};

/**
 * Set/update the system settings
 * @param {object} applicationSettings the system settings
 */
export const setSystemSettings = (applicationSettings) => {
	GLOBAL_APPSETTINGS = applicationSettings;
};