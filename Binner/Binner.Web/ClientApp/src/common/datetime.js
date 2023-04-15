
import { format, parse } from "date-fns";
export const Format24HourTime = 'kk:mm:ss';
export const Format12HourTime = 'h:mm aaa';
export const FormatFullDateTime = 'E, MMM dd h:mm:ss aaa';
export const FormatShortDate = 'E, MMM dd';
export const FormatDateOnly = 'MMM dd, yyyy';
export const FormatWeekNumber = 'w';

/**
 * Get the time represented by a timestamp, with friendly human readable text
 * @param {number} timestamp timestamp in milliseconds
 * @returns 
 */
export const getFriendlyElapsedTime = function (timestamp, includeRelative = false) {
	// Convert to a positive integer
	var time = Math.abs(timestamp);

	// Define humanTime and units
	var humanTime, units;

	// If there are years
	if (time > (1000 * 60 * 60 * 24 * 365)) {
		humanTime = parseInt(time / (1000 * 60 * 60 * 24 * 365), 10);
		units = 'years';
	}

	// If there are months
	else if (time > (1000 * 60 * 60 * 24 * 30)) {
		humanTime = parseInt(time / (1000 * 60 * 60 * 24 * 30), 10);
		units = 'months';
	}

	// If there are weeks
	else if (time > (1000 * 60 * 60 * 24 * 7)) {
		humanTime = parseInt(time / (1000 * 60 * 60 * 24 * 7), 10);
		units = 'weeks';
	}

	// If there are days
	else if (time > (1000 * 60 * 60 * 24)) {
		humanTime = parseInt(time / (1000 * 60 * 60 * 24), 10);
		units = 'days';
	}

	// If there are hours
	else if (time > (1000 * 60 * 60)) {
		humanTime = parseInt(time / (1000 * 60 * 60), 10);
		units = 'hours';
	}

	// If there are minutes
	else if (time > (1000 * 60)) {
		humanTime = parseInt(time / (1000 * 60), 10);
		units = 'minutes';
	}

	// Otherwise, use seconds
	else {
		humanTime = parseInt(time / (1000), 10);
		units = 'seconds';
	}

		// Get the time and units
	var timeUnits = humanTime + ' ' + units;

	if (includeRelative) {
		// If in the future
		if (timestamp > 0) {
			return 'in ' + timeUnits;
		}

		// If in the past
		return timeUnits + ' ago';
	}

	return timeUnits;
};

/**
 * Get a date/time formatted by locale, in local timezone
 * @param {number} time ticks in milliseconds
 * @returns ''
 */
export const getFormattedTime = (time) => {
	if (time === null)
		return '';
	const date = new Date(time);
	const options = { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric', hour12: true };
	return date.toLocaleString('en-US');
};

/**
 * Get the time difference between 2 timestamps
 * @param {number} time1 ticks in milliseconds
 * @param {number} time2 ticks in milliseconds
 * @returns 
 */
export const getTimeDifference = (time1, time2) => {
	return time2 - time1;
};

/**
 * Get elapsed time without milliseconds
 * @param {string} elapsed 
 * @returns 
 */
export const getBasicElapsed = (elapsed) => {
	if (elapsed === null)
		return null;
	const msIndex = elapsed.indexOf('.');
	return elapsed.substring(0, msIndex);
};

/**
 * Get a date object ignoring the time
 * @param {string} dateStr Date string
 * @returns Date object
 */
export const getDateWithoutTime = (dateStr) => {
	var indexOfTime = dateStr.indexOf('T');
	var dateOnly = dateStr.substring(0, indexOfTime);
	var dateParts = dateOnly.split('-');
	var date = new Date(parseInt(dateParts[0]), parseInt(dateParts[1]) - 1, parseInt(dateParts[2]));
	return date;
};

/**
 * Get the elapsed time between two dates
 * @param {Date} date1 first date
 * @param {Date} date2 second date
 */
export const getElapsed = (date1, date2 = new Date()) => {
	if (date1 === null || date2 === null)
		return null;

	let timeDiff = date2.getTime() - date1.getTime();

	// Convert time difference from milliseconds to seconds
	timeDiff = timeDiff / 1000;

	let seconds = Math.floor(timeDiff % 60); //ignoring uncomplete seconds (floor)
	timeDiff = Math.floor(timeDiff / 60);

	let minutes = timeDiff % 60;
	timeDiff = Math.floor(timeDiff / 60);

	let hours = timeDiff % 24;
	timeDiff = Math.floor(timeDiff / 24);

	// The rest of timeDiff is number of days
	let days = timeDiff;

	let totalHours = hours + (days * 24); // add days to hours

	let totalHoursAsString = totalHours < 10 ? "0" + totalHours : totalHours + "";
	let minutesAsString = (minutes < 10 && totalHours > 0) ? "0" + minutes : minutes + "";
	let secondsAsString = seconds < 10 ? "0" + seconds : seconds + "";

	if (totalHoursAsString === "00") {
			return minutesAsString + "m:" + secondsAsString + "s";
	} else {
			return totalHoursAsString + "h:" + minutesAsString + "m:" + secondsAsString + "s";
	}
};

const dateRangeOverlaps = (a_start, a_end, b_start, b_end) => {
	if (a_start < b_start && b_start < a_end) return true; // b starts in a
	if (a_start < b_end   && b_end   < a_end) return true; // b ends in a
	if (b_start <  a_start && a_end   <  b_end) return true; // a in b
	return false;
};

/**
 * Check if multiple date ranges overlap
 * @param {array} timeEntries array of date objects in the format of: { from: Date, to: Date }
 * @returns true if date ranges overlap
 */
export const multipleDateRangeOverlaps = (timeEntries = []) => {
	let i = 0, j = 0;
	let timeIntervals = timeEntries.filter(entry => entry.from != null && entry.to != null);

	if (timeIntervals != null && timeIntervals.length > 1)
	for (i = 0; i < timeIntervals.length - 1; i += 1) {
			for (j = i + 1; j < timeIntervals.length; j += 1) {
							if (
							dateRangeOverlaps(
					timeIntervals[i].from.getTime(), timeIntervals[i].to.getTime(),
					timeIntervals[j].from.getTime(), timeIntervals[j].to.getTime()
									)
							) return true;
					}
			}
 return false;
};

/**
 * Gets the date of the start of the current week, with start of the week as Monday
 * @param {Date} date current date
 * @returns date of the start of the current week
 */
export const getStartOfCurrentWeek = (date) => {
  const inputDate = new Date(date); // ensure no mutating of the input
	inputDate.setHours(0);
	inputDate.setMinutes(0);
	inputDate.setSeconds(0);
  var day = inputDate.getDay(),
      diff = inputDate.getDate() - day + (day === 0 ? -6 : 1);
	const startOfCurrentWeek = new Date(inputDate.setDate(diff));
  return startOfCurrentWeek;
}

/**
 * Gets the date of the start of the next week, with start of the week as Monday
 * @param {Date} date current date
 * @returns date of the start of the next week
 */
export const getStartOfNextWeek = (date) => {
	const inputDate = new Date(date); // ensure no mutating of the input
	inputDate.setHours(0);
	inputDate.setMinutes(0);
	inputDate.setSeconds(0);
  const today = inputDate.getDate();
  let currentDay = inputDate.getDay();
	if (currentDay === 0)
		currentDay = 6;
	else
		currentDay -= 1;
  const newDate = inputDate.setDate(today - currentDay + 7);
	const startOfNextWeek = new Date(newDate);
  return startOfNextWeek;
}

/**
 * Add seconds to a date
 * @param {Date} date 
 * @param {number} seconds seconds to add
 * @returns 
 */
export const addSeconds = (date, seconds = 0) => {
	const newDate = new Date(date);
	newDate.setSeconds(newDate.getSeconds() + seconds);
	return newDate;
};

/**
 * Add minutes to a date
 * @param {Date} date 
 * @param {number} minutes minutes to add
 * @returns 
 */
export const addMinutes = (date, minutes = 0) => {
	const newDate = new Date(date);
	newDate.setMinutes(newDate.getMinutes() + minutes);
	return newDate;
};

/**
 * Add hours to a date
 * @param {Date} date 
 * @param {number} hours hours to add
 * @returns 
 */
export const addHours = (date, hours = 0) => {
	const newDate = new Date(date);
	newDate.setHours(newDate.getHours() + hours);
	return newDate;
};

/**
 * Add days to a date
 * @param {Date} date 
 * @param {number} days days to add
 * @returns 
 */
export const addDays = (date, days = 0) => {
	const newDate = new Date(date);
	newDate.setDate(newDate.getDate() + days);
	return newDate;
};

/**
 * Add weeks to a date
 * @param {Date} date 
 * @param {number} weeks weeks to add 
 * @returns 
 */
export const addWeeks = (date, weeks = 0) => {
	const newDate = new Date(date);
	newDate.setDate(newDate.getDate() + (weeks * 7));
	return newDate;
};

/**
 * Add time component to a date
 * @param {Date} date input date
 * @param {Date} time time to add to input date
 * @returns new Date
 */
export const addTimeToDate = (date, time) => {
	
	var milliseconds = date.getTime();
	var addMilliseconds = (time.getHours() * 60 * 60 * 1000) + (time.getMinutes() * 60 * 1000) + (time.getSeconds() * 1000) + time.getMilliseconds();
	var newDate = new Date(milliseconds + addMilliseconds);
	return newDate;
};

/**
 * Format a string as a timespan
 * @param {string} time TimeSpan string value 00:00 or 00:00:00
 */
export const formatTimeSpan = (time, withSeconds = true, withLeadingZeros = true) => {
	const parts = time.split(':');
	const formattedParts = [];
	const maxParts = withSeconds ? 3 : 2;
	
	for(var i = 0; i < maxParts; i++){
		if (i < parts.length) {
			if (withLeadingZeros || i > 0)
				formattedParts.push(String(parseInt(parts[i])).padStart(2, "0"));
			else
				formattedParts.push(String(parseInt(parts[i])));
		}
		else
			formattedParts.push('00');
	}
	const formattedTime = formattedParts.join(':');
	return formattedTime;
};

/**
 * Format a string as time
 * @param {string} time TimeSpan string value 00:00 or 00:00:00
 */
export const formatTime = (time) => {
	var timeValue = parse(time, "HH:mm:ss", new Date());

	return format(timeValue, 'h:mm aa').replace('AM', '');
};

/**
 * Get the difference in days between two dates
 * @param {Date} date1 date 1
 * @param {Date} date2 date 2
 * @returns number of days difference
 */
export const getDifferenceInDays = (date1, date2) => {
  const MillisecondsPerDay = 1000 * 60 * 60 * 24;
  const utc1 = Date.UTC(date1.getFullYear(), date1.getMonth(), date1.getDate());
  const utc2 = Date.UTC(date2.getFullYear(), date2.getMonth(), date2.getDate());

  return Math.floor((utc2 - utc1) / MillisecondsPerDay);
};

/**
 * Get the difference in minutes between two dates
 * @param {Date} date1 date 1
 * @param {Date} date2 date 2
 * @returns number of minutes difference
 */
export const getDifferenceInMinutes = (date1, date2) => {
  const MillisecondsPerMinute = 1000 * 60;
  const utc1 = Date.UTC(date1.getFullYear(), date1.getMonth(), date1.getDate(), date1.getHours(), date1.getMinutes(), 0);
  const utc2 = Date.UTC(date2.getFullYear(), date2.getMonth(), date2.getDate(), date2.getHours(), date2.getMinutes(), 0);

  return Math.floor((utc2 - utc1) / MillisecondsPerMinute);
};

/**
 * Get the difference in seconds between two dates
 * @param {Date} date1 date 1
 * @param {Date} date2 date 2
 * @returns number of minutes difference
 */
export const getDifferenceInSeconds = (date1, date2) => {
  const MillisecondsPerSecond = 1000;
  const utc1 = Date.UTC(date1.getFullYear(), date1.getMonth(), date1.getDate(), date1.getHours(), date1.getMinutes(), date1.getSeconds());
  const utc2 = Date.UTC(date2.getFullYear(), date2.getMonth(), date2.getDate(), date2.getHours(), date2.getMinutes(), date2.getSeconds());

  return Math.floor((utc2 - utc1) / MillisecondsPerSecond);
};