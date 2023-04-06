import { createContext } from "react";

export const ErrorContext = createContext({
  modalTitle: "",
  url: "",
  header: "",
  errorMessage: "",
  stackTrace: ""
});
