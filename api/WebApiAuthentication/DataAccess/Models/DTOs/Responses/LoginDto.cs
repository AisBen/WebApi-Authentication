namespace WebApiAuthentication.DataAccess.Models.DTOs.Responses
{
	public class LoginDto
	{
		public required string AccessToken { get; set; }
		public DateTime AccessTokenExpiration { get; set; }
		public required string RefreshToken { get; set; }
	}
}
