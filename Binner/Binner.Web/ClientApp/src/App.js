import React, { useState, useEffect } from "react";
import { Route, Routes } from "react-router";
import { useSearchParams } from "react-router-dom";
import { toast } from "react-toastify";

// styles
import "./custom.css";
import "./bootstrap.css"; /* needed for the carousel control */

import AppRoutes from './AppRoutes';

function withSearchParams(Component) {
  return (props) => <Component {...props} searchParams={useSearchParams()} />;
}

export const App = (props) => {
  

  return (
    <div>
      <Routes>
        {AppRoutes.map((route, index) => {
          const { element, requireAuth, ...rest } = route;
          return <Route key={index} {...rest} element={requireAuth ? <AuthorizeRoute {...rest} element={element} /> : element} />;
        })}
      </Routes>
    </div>
  );
}

export default withSearchParams(App);
