// import { createApi, fetchBaseQuery, BaseQueryFn, FetchArgs, FetchBaseQueryError } from "@reduxjs/toolkit/query/react";
// import { RootState } from "../../app/store"; // Assuming you have a store type
// import { logOut } from "../../features/auth/authSlice";
// import { authApiSlice } from "../../features/auth/authApiSlice";

// // Define the TypeScript interface for the tokens
// interface RefreshTokenBody {
//   accessToken: string;
//   refreshToken: string;
// }

// const baseQuery = fetchBaseQuery({
//   baseUrl: "http://localhost:5000/api",
//   credentials: "include", // send HTTPonly cookies for every request
//   prepareHeaders: (headers, { getState }) => {
//     const token = (getState() as RootState).auth.accessToken;
//     if (token) {
//       headers.set("Authorization", `Bearer ${token}`);
//     }
//     return headers;
//   },
// });

// // Typing the baseQuery with re-authentication
// const baseQueryWithReauth: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError, {}, RefreshTokenBody> = async (args, api, extraOptions) => {
//   let result = await baseQuery(args, api, extraOptions);
//   if (result.error && result.error.status === 401) {
//     // Attempt to refresh the token here
//     const accessToken = (api.getState() as RootState).auth.accessToken;
//     const refreshToken = (api.getState() as RootState).auth.refreshToken;

//     const response = await api.dispatch(authApiSlice.endpoints.refresh.initiate({ accessToken, refreshToken }));
//     const newAccessToken = response.data?.accessToken;
//     if (newAccessToken) {
//       // Retry the original query with new access token
//       result = await baseQuery(args, api, extraOptions);
//     } else {
//       // If the refresh token failed, log the user out
//       api.dispatch(logOut());
//     }
//   }
//   return result;
// };

// // Ensure `apiSlice` uses `baseQueryWithReauth` for its baseQuery to integrate the re-authentication logic
// export const apiSlice = createApi({
//   reducerPath: "api", // Ensure this matches your existing configuration
//   baseQuery: baseQueryWithReauth,
//   endpoints: (builder) => ({}),
// });
