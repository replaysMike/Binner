const DefaultRootContainerName = 'values';
const DefaultContainerName = 'root';

	/**
	 * Structure of local storage data
	 * 
	 * with location
	 {
		 settingsName = {
			 location: {
				settingName: {}
			 }
	   }
	 }
	 *
	 * without location
	 settingsName = {
     containerName: {
			settingName: {}
	   }
	 }
	 */

/**
 * Get a value from local storage
 * @param {string} settingName the name of the setting
 * @param {object} options namespace options
 * @returns value
 */
export const getLocalData = (settingName, options = { settingsName: DefaultRootContainerName, containerName: DefaultContainerName, location: null }) => {
	const values = JSON.parse(localStorage.getItem(options.settingsName));
	if (values) {
		const prefLocation = options.location?.pathname?.toLowerCase().replaceAll('/', '') || options.containerName || DefaultContainerName;
		const prefLocationSettings = values[prefLocation];
		if (prefLocationSettings){
			const val = prefLocationSettings[settingName];
			return val;
		}
	}
	return null;
};

/**
 * Set a value in local storage
 * @param {string} settingName name of the setting
 * @param {object} value the value to set
 * @param {object} options namespace options
 */
export const setLocalData = (settingName, value, options = { settingsName: DefaultRootContainerName, containerName: DefaultContainerName, location: null }) => {
	const currentViewPreferences = JSON.parse(localStorage.getItem(options.settingsName))
	const prefLocation = options.location?.pathname?.toLowerCase().replaceAll('/', '') || options.containerName || DefaultContainerName;
	let values = {...currentViewPreferences };
	values[prefLocation] = {...values[prefLocation], [settingName]: value};
	localStorage.setItem(options.settingsName, JSON.stringify(values));
};

/**
 * Remove a value from local storage
 * @param {string} settingName name of the setting
 * @param {object} value the value to set
 * @param {object} options namespace options
 */
export const removeLocalData = (settingName, options = { settingsName: DefaultRootContainerName, containerName: DefaultContainerName, location: null }) => {
	const currentViewPreferences = JSON.parse(localStorage.getItem(options.settingsName))
	const prefLocation = options.location?.pathname?.toLowerCase().replaceAll('/', '') || options.containerName || DefaultContainerName;
	let values = {...currentViewPreferences };
	const prefValues = values[prefLocation];
	delete prefValues[settingName];
	const keys = Object.keys(prefValues);
	// if there are no children left, remove the entire key
	if (keys.length === 0)
		localStorage.removeItem(options.settingsName);
	else
		localStorage.setItem(options.settingsName, JSON.stringify(values));
};