using Microsoft.AspNetCore.Identity;

namespace WebApiAuthentication.DataAccess.Entities
{
	public class ApplicationUser : IdentityUser
	{
		public bool RatingsAllowed { get; set; }
		public required ICollection<BookReview> Reviews { get; set; }
		public string? RefreshToken { get; set; }
		public DateTime RefreshTokenExpiry { get; set; }
	}
}
