export class NotificationService { 
  _callbacks = [];
  _nextSubscriptionId = 0;

  subscribe(callback, who = 'unknown') {
    this._callbacks.push({ callback, subscription: this._nextSubscriptionId++ });
    return this._nextSubscriptionId - 1;
  }

  unsubscribe(subscriptionId) {
    const subscriptionIndex = this._callbacks
      .map((element, index) => element.subscription === subscriptionId ? { found: true, index } : { found: false })
      .filter(element => element.found === true);
    if (subscriptionIndex.length !== 1) {
      //throw new Error(`Found an invalid number of subscriptions ${subscriptionIndex.length}`);
    } else {
      this._callbacks.splice(subscriptionIndex[0].index, 1);
    }
  }

  notifySubscribers() {
    for (let i = 0; i < this._callbacks.length; i++) {
      const callback = this._callbacks[i].callback;
      callback();
    }
  }
};

const notificationService = new NotificationService();
export default notificationService;