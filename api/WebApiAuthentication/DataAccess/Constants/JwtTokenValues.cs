namespace WebApiAuthentication.DataAccess.Constants
{
	public class JwtTokenValues
	{
		public const int AccessTokenExpiryMinutes = 1;
		public const int RefreshTokenExpiryHours = 3;
		public const string RefreshTokenCookieName = "refreshToken";
	}
}
