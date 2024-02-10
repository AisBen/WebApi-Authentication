interface CredentialsPayload {
  user: any; // Use a more specific type based on your user object structure
  accessToken: string;
  refreshToken: string;
}
