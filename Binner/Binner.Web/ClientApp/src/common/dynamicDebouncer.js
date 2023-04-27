/**
 * debounce a function call with the ability to update the interval as needed (even memoized)
 * @param {func} func the function to debounce
 * @param {func} getIntervalFunc the function to fetch the debounce interval value
 * @returns 
 */
export const dynamicDebouncer = (func, getIntervalFunc) => {
	let timer;
	return (...args) => {
		clearTimeout(timer);
		timer = setTimeout(() => { func.apply(this, args); }, getIntervalFunc());
	};
};