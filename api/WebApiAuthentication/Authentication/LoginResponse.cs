namespace WebApiAuthentication.Authentication
{
	public class LoginResponse
	{
		public required string JwtToken { get; set; }
		public DateTime JwtExpiration { get; set; }
		public required string RefreshToken { get; set; }
	}
}
