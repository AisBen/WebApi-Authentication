import { createSlice } from "@reduxjs/toolkit";
import { jwtDecode } from "jwt-decode";
const authSlice = createSlice({
  name: "auth",
  initialState: { user: null, accessToken: null, refreshToken: null, roles: null },
  reducers: {
    setCredentials: (state, action) => {
      const { user, accessToken, refreshToken } = action.payload.data;
      const rolesClaimUri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
      state.user = user;
      state.accessToken = accessToken;
      state.refreshToken = refreshToken;
      state.roles = jwtDecode(accessToken)[rolesClaimUri];
    },
    logOut: (state, action) => {
      state.user = null;
      state.accessToken = null;
      state.refreshToken = null;
    },
  },
});

export const { setCredentials, logOut } = authSlice.actions;

export default authSlice.reducer;

export const selectCurrentUser = (state) => state.auth.user;
export const selectAccessToken = (state) => state.auth.accessToken;
export const selectRefreshToken = (state) => state.auth.refreshToken;
