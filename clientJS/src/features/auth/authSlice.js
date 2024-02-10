import { createSlice } from "@reduxjs/toolkit";
import { jwtDecode } from "jwt-decode";
const authSlice = createSlice({
  name: "auth",
  initialState: { user: null, accessToken: null, accessTokenExpiration: null, roles: null },
  reducers: {
    setCredentials: (state, action) => {
      const { user, accessToken, accessTokenExpiration } = action.payload;
      const rolesClaimUri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
      state.user = user;
      state.accessToken = accessToken;
      state.accessTokenExpiration = accessTokenExpiration;
      state.roles = jwtDecode(accessToken)[rolesClaimUri];
    },
    logOut: (state, action) => {
      state.user = null;
      state.accessToken = null;
      state.accessTokenExpiration = null;
    },
  },
});

export const { setCredentials, logOut } = authSlice.actions;

export default authSlice.reducer;

export const selectCurrentUser = (state) => state.auth.user;
export const selectAccessToken = (state) => state.auth.accessToken;
export const selectAccessTokenExpiration = (state) => state.auth.accessTokenExpiration;
