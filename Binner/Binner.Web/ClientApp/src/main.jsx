import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
//import ReactDOM from "react-dom/client";
import { createRoot } from 'react-dom/client';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import App from './App';
import AppRoutes from './AppRoutes';
import registerServiceWorker from './registerServiceWorker';
import { ToastContainer } from "react-toastify";
// layouts
import { RootLayout } from "./layouts/RootLayout";
import { BasicLayout } from "./layouts/BasicLayout";

// import i18n
import './i18n';

//const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
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

/**
 * Apply a layout based on the route definition
 */
const routeWithLayout = (route) => {
    if (route.requireAuth)
        return <AuthorizeRoute {...route} />;
    // apply a layout if specified in the route
    if (route.layout)
        return React.createElement(route.layout.type, null, route.element); 
    return <BasicLayout>{route.element}</BasicLayout>;
};

const root = createRoot(rootElement);
const routes = AppRoutes.map((route, index) => ({
  path: route.path || "/",
  requireAuth: route.requireAuth || false,
  element: routeWithLayout(route)
}));
const router = createBrowserRouter([
  {
    // apply a root layout
    element: <RootLayout />,
    // it's children are the routes
    children: routes
  }
]);

root.render(
<>
  <ToastContainer newestOnTop={true} autoClose={5000} hideProgressBar={true} theme="colored" position="top-center" />
  <RouterProvider router={router}>
    <App />
  </RouterProvider>
</>);

registerServiceWorker();

