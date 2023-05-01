import { Navigate } from "react-router";
import { isAdmin } from "../common/authentication";
import PageWrapper from "./PageWrapper";

const AuthWrapper = ({ children }) => {
  if (isAdmin()) {
    return <PageWrapper>{children}</PageWrapper>;
  }
  return <Navigate to={"/accessdenied"} />;
};

export default AuthWrapper;