export class CustomEvents {
	_callbacks = [];
	_nextSubscriptionId = 0;
	
  /**
   * Subscribe to an event
   * @param {string} name Name of event
   * @param {func} callback Callback to call when event is fired
   * @returns 
   */
	subscribe(name, callback) {
    this._callbacks.push({ name, callback, subscription: this._nextSubscriptionId++ });
    return this._nextSubscriptionId - 1;
  }

  /**
   * Unsubscribe from an event
   * @param {int} subscriptionId The subscriptionId that was returned when the event was subscribed
   */
	unsubscribe(subscriptionId) {
    const subscriptionIndex = this._callbacks
      .map((element, index) => element.subscription === subscriptionId ? { found: true, index } : { found: false })
      .filter(element => element.found === true);
    if (subscriptionIndex.length !== 1) {
      throw new Error(`Found an invalid number of subscriptions ${subscriptionIndex.length}`);
    }

    this._callbacks.splice(subscriptionIndex[0].index, 1);
  }

  /**
   * Notify all subscribers of a specific event
   * @param {string} name Name of event
   * @param {object} data Optional data included with the event
   */
  notifySubscribers(name, data = {}) {
    for (let i = 0; i < this._callbacks.length; i++) {
      var _callback = this._callbacks[i];
      if (_callback.name === name) {
        const callback = _callback.callback;
        if (callback) callback(data, _callback.subscription);
      }
    }
  }
};

const customEvents = new CustomEvents();

export default customEvents;