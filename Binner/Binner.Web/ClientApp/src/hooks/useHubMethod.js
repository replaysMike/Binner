import { useCallback, useEffect, useRef, useState } from "react";

const initialState = {
  loading: false
}

/**
 * Provide an "invoke" function to invokes a hub method on the server and provide the async state (loading & error)
 * @param hubConnection The hub connection to use
 * @param methodName The name of the server method to invoke.
 */
export function useHubMethod(hubConnection, methodName) {
  const [state, setState] = useState(initialState);
  const isMounted = useRef(true);

  const setStateIfMounted = useCallback((value) => {
    if (isMounted.current) {
      setState(value);
    }
  }, []);

  const invoke = useCallback(async (...args) => {
    setStateIfMounted(s => ({ ...s, loading: true }));

    try {
      if (hubConnection) {
        const data = await hubConnection.invoke(methodName, ...args);
        setStateIfMounted(s => ({ ...s, data: data, loading: false, error: undefined }));
        return data;
      }
      else {
        throw new Error('hubConnection is not defined');
      }
    }
    catch (e) {
      setStateIfMounted(s => ({ ...s, error: e, loading: false }));
    }
  }, [hubConnection, methodName]);

  useEffect(() => () => { isMounted.current = false }, []);

  return { invoke, ...state };
}