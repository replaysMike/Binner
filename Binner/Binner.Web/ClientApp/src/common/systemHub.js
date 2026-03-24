import { createContext, useState, useEffect, useRef, useContext, createRef } from 'react';
import PropTypes from 'prop-types';
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useHub, useClientMethod } from "../hooks/signalR";
import notificationService from "./notificationService.js";
import { GetTypeName } from "./Types.js";
import { getUserAccount, isAuthenticated, getAuthToken } from "./authentication.js";
import _ from 'underscore';

export const SystemHubContext = createContext([0, () => { }]);
export const useSystemHub = () => useContext(SystemHubContext);

export const SystemHubEvents = {
  SubscriptionLevellChange: 0,
}

export const systemHub = {
  /** Expose signalR connection instance */
  connection: null,
  /** Expose subscribe method */
  subscribe: null,
  /** Expose unsubscribe method */
  unsubscribe: null,
  /** Expose unsubscribeHandler method */
  unsubscribeHandler: null,
};

/**
 * SignalR implementation provider that handles connections and events.
 * Usage: <SystemHubProvider onConnect={} onReceiveEvent={}><div>...children</div></SystemHubProvider>
 */
const SystemHubProvider = ({ onReceiveEvent, onConnect, onConnected, onDisconnected, children }) => {
  const [subscription, setSubscription] = useState(null);
  const [eventHandlerId, setEventHandlerId] = useState(0);
  const [registeredEventHandlers, setRegisteredEventHandlers] = useState([]);

  const instance = createRef();
  const connection = useRef(null);
  const { hubConnectionState, error } = useHub(connection.current);

  // create an instance object which will hold state passed to subscribed events
  instance.current = systemHub;

  useEffect(() => {
    const initialize = async () => {
      // see: https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-8.0&tabs=visual-studio
      // build the websockets connection once
      const signalRConnection = new HubConnectionBuilder()
        .withUrl("/hubs/systemHub", { accessTokenFactory: async () => await getAuthToken() })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      connection.current = signalRConnection;

      const startConnection = async () => {
        try {
          if (onConnect) onConnect(instance.current);
          await signalRConnection.start();
          signalRConnection.enableReconnect = true;
          if (onConnected) onConnected(instance.current);
          console.log('SignalR connected!');
          return signalRConnection;
          // await handleSubscribeToPendingApprovalRequestCount();
        } catch (err) {
          console.error('startConnection error, will retry...', err);
          setTimeout(startConnection, 5000);
        }
      };

      signalRConnection.onclose(async (err) => {
        if (onDisconnected) onDisconnected(instance.current);
        if (signalRConnection.enableReconnect) {
          console.log('SignalR connection closed. Reconnecting...', err);
          await startConnection();
        }
      });

      startConnection();

    };

    // subscribe to auth service state
    setSubscription(notificationService.subscribe(() => populateState(initialize)));
    populateState(initialize);

    return () => {
      notificationService.unsubscribe(subscription);
      connection.current.enableReconnect = false;
      connection.current?.stop();
    };
  }, []);

  const populateState = async (initializer) => {
    // wait for a user to be authenticated
    const [isUserAuthenticated, user] = await Promise.all([isAuthenticated(), getUserAccount()])
    // if a user was authenticated, connect SignalR
    if (isUserAuthenticated && initializer)
      await initializer();
  };

  const performSubscribeTo = async (invoker, args) => {
    try {
      if (connection.current === null) {
        console.error("Cant subscribe, not connected!");
        return;
      }
      if (invoker) await invoker(args);
    } catch (err) {
      console.error('subscribe failed', err);
    }
  };

  const performUnsubscribeFrom = async (invoker, args) => {
    try {
      if (connection.current === null) {
        console.error("Cant unsubscribe, not connected!");
        return;
      }
      if (invoker) await invoker(args);
    } catch (err) {
      console.error('unsubscribe failed', err);
    }
  };

  // receive events
  useClientMethod(connection.current, "SubscriptionLevelChange", (subscriptionLevel) => {
    const args = { subscriptionLevel };
    console.log('rx event (SubscriptionLevelChange)', args);
    if (onReceiveEvent)
      onReceiveEvent(instance.current, SystemHubEvents.SubscriptionLevellChange, args);

    invokeRegisteredHandlers(SystemHubEvents.SubscriptionLevellChange, args);
  });

  /**
 * Invokes any registered handlers for the specified event type
 * @param {SystemHubEvents} eventType the type of event to invoke
 * @param {object} args The arguments to pass to the event
 * @param {object} comparitor a comparitor expression to match the args
 */
  const invokeRegisteredHandlers = (eventType, args, comparitor = null) => {
    const registeredHandlers = _.filter(registeredEventHandlers, comparitor != null ? comparitor : x => x.event === eventType);
    console.log('invokeRegisteredHandlers()', eventType, args, registeredEventHandlers, registeredHandlers);
    if (registeredHandlers && registeredEventHandlers.length > 0) {
      for (let i = 0; i < registeredHandlers.length; i++) {
        console.log('invoking registered handler', eventType, args, registeredHandlers[i]);
        registeredHandlers[i].invoker({ ...instance.current, handlerId: registeredHandlers[i].id }, eventType, args, registeredHandlers[i].additionalData);
      }
    }
  };

  const nextEventHandlerId = () => {
    const nextId = eventHandlerId + 1;
    setEventHandlerId(nextId);
    return nextId;
  };

  // subscribe/unsubscribe
  const subscribe = async (event, args = null, eventHandler = null, additionalData = null) => {
    console.log('subscribing to', GetTypeName(SystemHubEvents, event), args);
    let id = null;
    if (eventHandler) {
      console.log('subscribing to a custom event handler', event, args);
      id = nextEventHandlerId();
      const newRegisteredEventHandlers = [...registeredEventHandlers, { id, event, args, invoker: eventHandler, additionalData }];
      setRegisteredEventHandlers(newRegisteredEventHandlers);
      console.log('registered custom event handler', id, newRegisteredEventHandlers);
    }
    await subscribeToEvent(event, args);
    return id;
  };

  const subscribeToEvent = async (event, args) => {
    switch (event) {
      case SystemHubEvents.SubscriptionLevellChange:
        await subscribeToSubscriptionLevelChange(args);
        break;
      default:
        throw new Error(`Unknown event type '${event}'`);
    }
  }

  const unsubscribe = async (event, args = null) => {
    console.log('unsubscribing from', GetTypeName(SystemHubEvents, event), args);
    // unsubscribe from regular event
    await unsubscribeFromEvent(event, args);
  };

  const unsubscribeHandler = async (ids, args = null) => {
    console.log('unsubscribing from handler', ids, args);
    if (Array.isArray(ids)) {
      // unsubscribe from a list of registered events
      const eventHandlerIds = ids;
      for (let i = 0; i < eventHandlerIds.length; i++) {
        const registeredEventHandler = _.find(registeredEventHandlers, i => i.id === eventHandlerIds[i]);
        if (registeredEventHandler) {
          await unsubscribeFromEvent(registeredEventHandler.event, registeredEventHandler.args);
          console.log('unregistered custom event handler by ids', registeredEventHandler.id);
        }
      }
      // remove handlers from tracking
      const newEventHandlerIds = _.filter(registeredEventHandlers, i => eventHandlerIds.includes(i.id));
      setRegisteredEventHandlers([...newEventHandlerIds]);
      console.log('unregister newEventHandlerIds', newEventHandlerIds);
      return;
    }
    if (ids instanceof Number) {
      console.log('unsub by id', ids);
      const registeredEventHandler = _.find(registeredEventHandlers, i => i.id === ids);
      if (registeredEventHandler) {
        await unsubscribeFromEvent(registeredEventHandler.event, registeredEventHandler.args);
        console.log('unregistered custom event handler by id', registeredEventHandler.id);
      }
    }
  };

  const unsubscribeFromEvent = async (event, args) => {
    switch (event) {
      case SystemHubEvents.SubscriptionLevellChange:
        await unsubscribeFromSubscriptionLevelChange();
        break;
      default:
        throw new Error(`Unknown event type '${event}'`);
    }
  }

  // handlers to subscribe to events
  const subscribeToSubscriptionLevelChange = async () => {
    await performSubscribeTo(async () => await connection.current.invoke("SubscribeSubscriptions"));
  };

  const unsubscribeFromSubscriptionLevelChange = async () => {
    await performUnsubscribeFrom(async () => await connection.current.invoke("UnsubscribeSubscriptionLevelChange"));
  };

  // expose subscribe & unsubscribe public methods
  instance.current.subscribe = subscribe;
  instance.current.unsubscribe = unsubscribe;
  instance.current.unsubscribeHandler = unsubscribeHandler;

  return (<SystemHubContext.Provider value={[connection, subscribe, unsubscribe, unsubscribeHandler]}>
    {children}
  </SystemHubContext.Provider>);

};

SystemHubProvider.propTypes = {
  onReceiveEvent: PropTypes.func,
  onConnect: PropTypes.func,
  onConnected: PropTypes.func,
  onDisconnected: PropTypes.func,
};

export default SystemHubProvider;