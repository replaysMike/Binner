import { Navigate } from "react-router";
import { isAdmin } from "../common/authentication";

const AuthWrapper = ({ children }) => {
  if (isAdmin()) {
    return children;
  }
  return <Navigate to={"/accessdenied"} />;
};

export default AuthWrapper;