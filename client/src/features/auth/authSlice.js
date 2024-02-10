import { createSlice } from "@reduxjs/toolkit";

const authSlice = createSlice({
  name: "auth",
  initialState: { user: null, accessToken: null, refreshToken: null, roles: null },
  reducers: {
    setCredentials: (state, action) => {
      const { user, accessToken, refreshToken, roles } = action.payload;
      state.user = user;
      state.accessToken = accessToken;
      state.refreshToken = refreshToken;
      state.roles = roles;
    },
    logOut: (state, action) => {
      state.user = null;
      state.accessToken = null;
      state.refreshToken = null;
      state.roles = null;
    },
  },
});

export const { setCredentials, logOut } = authSlice.actions;

export default authSlice.reducer;

export const selectCurrentUser = (state) => state.auth.user;
export const selectAccessToken = (state) => state.auth.accessToken;
export const selectRefreshToken = (state) => state.auth.refreshToken;
export const selectRoles = (state) => state.auth.roles;
