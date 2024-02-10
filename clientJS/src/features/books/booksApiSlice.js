import { apiSlice } from "../../app/api/apiSlice";

export const booksApiSlice = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getBooks: builder.query({
      query: () => "/BookReviews",
    }),
  }),
});

export const { useGetBooksQuery } = booksApiSlice;
