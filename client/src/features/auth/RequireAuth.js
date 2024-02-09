import { useLocation, Navigate, Outlet } from "react-router-dom";
import { useSelector } from "react-redux";
import { selectAccessToken } from "./authSlice";

const RequireAuth = () => {
  const token = useSelector(selectAccessToken);
  const location = useLocation();

  return token ? (
    <Outlet />
  ) : (
    <Navigate
      to="/login"
      state={{ from: location }}
      replace
    />
  );
};
export default RequireAuth;
