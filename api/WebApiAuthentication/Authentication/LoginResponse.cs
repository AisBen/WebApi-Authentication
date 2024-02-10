namespace WebApiAuthentication.Authentication
{
	public class LoginResponse
	{
		public required string AccessToken { get; set; }
		public DateTime AccessTokenExpiration { get; set; }
		public required string RefreshToken { get; set; }
		public List<string> Roles { get; set; } = new List<string>();
	}
}
