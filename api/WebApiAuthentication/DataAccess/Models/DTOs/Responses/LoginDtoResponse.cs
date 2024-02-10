namespace WebApiAuthentication.DataAccess.Models.DTOs.Responses
{
	public class LoginDtoResponse
	{
		public required string AccessToken { get; set; }
		//public DateTime AccessTokenExpiration { get; set; }
		//public string? RefreshToken { get; set; }
	}
}
