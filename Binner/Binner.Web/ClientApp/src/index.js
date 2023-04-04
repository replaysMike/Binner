import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from "react-dom/client";
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import registerServiceWorker from './registerServiceWorker';

// import i18n
import './i18n';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

// Fixes wheel events so they don't bubble up
// https://github.com/facebook/react/issues/14856
const EVENTS_TO_MODIFY = ['wheel'];
const originalAddEventListener = document.addEventListener.bind();
document.addEventListener = (type, listener, options, wantsUntrusted) => {
    let modOptions = options;
    if (EVENTS_TO_MODIFY.includes(type)) {
        if (typeof options === 'boolean') {
            modOptions = {
                capture: options,
                passive: false,
            };
        } else if (typeof options === 'object') {
            modOptions = {
                passive: false,
                ...options,
            };
        }
    }

    return originalAddEventListener(type, listener, modOptions, wantsUntrusted);
};

const originalRemoveEventListener = document.removeEventListener.bind();
document.removeEventListener = (type, listener, options) => {
    let modOptions = options;
    if (EVENTS_TO_MODIFY.includes(type)) {
        if (typeof options === 'boolean') {
            modOptions = {
                capture: options,
                passive: false,
            };
        } else if (typeof options === 'object') {
            modOptions = {
                passive: false,
                ...options,
            };
        }
    }
    return originalRemoveEventListener(type, listener, modOptions);
};
// end fix

const root = ReactDOM.createRoot(rootElement);
root.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
);

registerServiceWorker();

