using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuthentication.Controllers;
using WebApiAuthentication.DataAccess.Constants;
using WebApiAuthentication.DataAccess.Models.Entities;

namespace WebApiAuthentication.Services
{
	public class JwtAuthenticationService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthenticationController> _logger;
		public JwtAuthenticationService(ILogger<AuthenticationController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_configuration = configuration;
			_userManager = userManager;
		}

		public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
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

		public JwtSecurityToken GenerateJwt(string username, List<Claim> roleClaims)
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
				expires: DateTime.UtcNow.AddSeconds(JwtTokenValues.AccessTokenExpiryMinutes), // Adjust token expiry as needed
				claims: authClaims,
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

			return token;
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[64];

			using var generator = RandomNumberGenerator.Create();

			generator.GetBytes(randomNumber);

			return Convert.ToBase64String(randomNumber);
		}


	}

}

