using System.ComponentModel.DataAnnotations;

namespace WebApiAuthentication.DataAccess.Models.DTOs.Responses
{

	public class RegistrationDto
	{
		public required string Username { get; set; }
		public required string Password { get; set; }
		[EmailAddress]
		public required string Email { get; set; }
	}
}
