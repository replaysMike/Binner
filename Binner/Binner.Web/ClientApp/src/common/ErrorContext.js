import React from "react";

export const ErrorContext = React.createContext({
  modalTitle: "",
  url: "",
  header: "",
  errorMessage: "",
  stackTrace: ""
});
