using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuthentication.Controllers;
using WebApiAuthentication.DataAccess.Authentication;
using WebApiAuthentication.DataAccess.Models.Entities;

namespace WebApiAuthentication.Services
{
	public class JwtAuthenticationService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthenticationController> _logger;
		//create constant for token expiry
		private const int AccessTokenExpiryMinutes = 1;
		private const int RefreshTokenExpiryHours = 3;
		public JwtAuthenticationService(ILogger<AuthenticationController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_configuration = configuration;
			_userManager = userManager;
		}

		public async Task<LoginResponseModel> Login(LoginModel model)
		{
			_logger.LogInformation("Login called");

			var user = await _userManager.FindByNameAsync(model.Username);

			if (user == null)
				return new LoginResponseModel { Message = "User does not exist", IsSuccess = false };

			if (!await _userManager.CheckPasswordAsync(user, model.Password))
				return new LoginResponseModel { Message = "Passwords do not match", IsSuccess = false };

			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

			JwtSecurityToken token = _generateJwt(model.Username, roleClaims);

			var refreshToken = _generateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(RefreshTokenExpiryHours); // refresh token expiry must be larger than access token expiry

			await _userManager.UpdateAsync(user);

			_logger.LogInformation("Login succeeded");

			return new LoginResponseModel
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				AccessTokenExpiration = token.ValidTo,
				RefreshToken = refreshToken,
				IsSuccess = true,
				Message = "Login succeeded"
			};
		}
		public async Task<RegistrationResponseModel> Register(RegistrationModel model)
		{
			_logger.LogInformation("Register called");

			var existingUser = await _userManager.FindByNameAsync(model.Username);

			if (existingUser != null)
				return new RegistrationResponseModel { Message = "User already exists", IsSuccess = false };

			var newUser = new ApplicationUser
			{
				Reviews = new List<BookReview>(),
				UserName = model.Username,
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString()
			};

			var result = await _userManager.CreateAsync(newUser, model.Password);

			if (result.Succeeded)
			{
				return new RegistrationResponseModel { Message = "User already exists", IsSuccess = true };

			}
			else
				return new RegistrationResponseModel
				{
					Message = "Failed to create user",
					IsSuccess = false,
					Errors = result.Errors.Select(e => e.Description)
				};

		}

		public async Task<LoginResponseModel> Refresh(RefreshModel model)
		{
			_logger.LogInformation("Refresh called");

			var principal = _getPrincipalFromExpiredToken(model.AccessToken);

			if (principal?.Identity?.Name is null)
				return new LoginResponseModel { Message = "Accesstoken not valid", IsSuccess = false };

			var user = await _userManager.FindByNameAsync(principal.Identity.Name);
			if (user is null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
				return new LoginResponseModel { Message = "User does not exist", IsSuccess = false };


			// Fetch roles again for the user
			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

			var newAccessToken = _generateJwt(principal.Identity.Name, roleClaims);


			return new LoginResponseModel
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
				AccessTokenExpiration = newAccessToken.ValidTo,
				RefreshToken = model.RefreshToken, // Decide if you want to issue a new refresh token here
				IsSuccess = true,
				Message = "Refresh succeeded"
			};
		}
		private ClaimsPrincipal? _getPrincipalFromExpiredToken(string token)
		{
			var secret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured");

			var validation = new TokenValidationParameters
			{
				ValidIssuer = _configuration["JWT:ValidIssuer"],
				ValidAudience = _configuration["JWT:ValidAudience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
				ValidateLifetime = false
			};

			return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
		}

		private JwtSecurityToken _generateJwt(string username, List<Claim> roleClaims)
		{
			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};
			authClaims.AddRange(roleClaims);


			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				_configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured")));
			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				expires: DateTime.UtcNow.AddSeconds(AccessTokenExpiryMinutes), // Adjust token expiry as needed
				claims: authClaims,
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

			return token;
		}

		private static string _generateRefreshToken()
		{
			var randomNumber = new byte[64];

			using var generator = RandomNumberGenerator.Create();

			generator.GetBytes(randomNumber);

			return Convert.ToBase64String(randomNumber);
		}


	}
	public class AisResponseModel
	{
		public string Message { get; set; }
		public bool IsSuccess { get; set; }
		public IEnumerable<string> Errors { get; set; }
	}
	public class LoginResponseModel : AisResponseModel
	{
		public string? AccessToken { get; set; }
		public DateTime AccessTokenExpiration { get; set; }
		public string? RefreshToken { get; set; }
	}
	public class RegistrationResponseModel : AisResponseModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		[EmailAddress]
		public string Email { get; set; }
	}
}

