namespace WebApiAuthentication.DataAccess.Authentication
{
    public class LoginResponse
    {
        public required string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
        public required string RefreshToken { get; set; }
    }
}
