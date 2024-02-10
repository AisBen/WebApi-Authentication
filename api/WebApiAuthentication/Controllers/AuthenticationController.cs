using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuthentication.DataAccess.Authentication;
using WebApiAuthentication.DataAccess.Constants;
using WebApiAuthentication.DataAccess.Context;
using WebApiAuthentication.DataAccess.Models.DTOs.Responses;
using WebApiAuthentication.DataAccess.Models.Entities;
using WebApiAuthentication.Services;

namespace WebApiAuthentication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthenticationController> _logger;
		private readonly JwtAuthenticationService _jwtAuthenticationService;
		private readonly TheDbContext _db;

		public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthenticationController> logger, JwtAuthenticationService jwtAuthenticationService, TheDbContext db)
		{
			_userManager = userManager;
			_configuration = configuration;
			_logger = logger;
			_jwtAuthenticationService = jwtAuthenticationService;
			_db = db;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegistrationModel model)
		{
			_logger.LogInformation("Register called");

			var existingUser = await _userManager.FindByNameAsync(model.Username);

			if (existingUser != null)
				return Conflict("User already exists.");

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
				_logger.LogInformation("Register succeeded");

				return Ok("User successfully created");
			}
			else
				return StatusCode(StatusCodes.Status500InternalServerError,
					   $"Failed to create user: {string.Join(" ", result.Errors.Select(e => e.Description))}");
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] LoginDto model)
		{
			_logger.LogInformation("Login called");

			var user = await _userManager.FindByNameAsync(model.Username);

			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
				return Unauthorized();

			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

			JwtSecurityToken token = _generateJwt(model.Username, roleClaims);

			var cookieRefreshToken = _jwtAuthenticationService.GenerateRefreshToken();

			var refreshToken = _hashToken(cookieRefreshToken);

			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				IsEssential = true,
				Expires = _jwtAuthenticationService.SetRefreshTokenExpiry(), // Adjust according to your refresh token's expiry
				SameSite = SameSiteMode.Strict, // Helps mitigate CSRF attacks
				Secure = true // Ensure this cookie is only sent over HTTPS
			};

			Response.Cookies.Append(JwtTokenValues.RefreshTokenCookieName, cookieRefreshToken, cookieOptions);

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiry = _jwtAuthenticationService.SetRefreshTokenExpiry();

			await _userManager.UpdateAsync(user);

			_logger.LogInformation("Login succeeded");

			return Ok(new LoginDtoResponse
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				AccessTokenExpiration = token.ValidTo,
			});
		}

		[HttpPost("Refresh")]
		public async Task<IActionResult> Refresh([FromBody] RefreshModel model)  // accepts a access token from client and refresh from cookie and returns a new JWT
		{
			_logger.LogInformation("Refresh called");

			var principal = _getPrincipalFromExpiredToken(model.AccessToken);
			if (principal?.Identity?.Name is null)
				return Forbid();
			var user = await _userManager.FindByNameAsync(principal.Identity.Name);

			var cookieRefreshToken = Request.Cookies[JwtTokenValues.RefreshTokenCookieName];

			if (string.IsNullOrWhiteSpace(cookieRefreshToken))
				return Forbid();

			var dbUser = await _db.Users.FirstOrDefaultAsync(x => x.RefreshToken == _hashToken(cookieRefreshToken));

			if (user is null || dbUser == null || dbUser.RefreshTokenExpiry < DateTime.UtcNow)
				return Forbid();

			// Fetch roles again for the user
			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

			var newAccessToken = _generateJwt(principal.Identity.Name, roleClaims);

			_logger.LogInformation("Refresh succeeded");

			return Ok(new LoginDtoResponse
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
				AccessTokenExpiration = newAccessToken.ValidTo,
			});
		}



		[Authorize]
		[HttpDelete("Revoke")] // after this is called, you cant use refresh call to get a new JWT
		public async Task<IActionResult> Revoke()
		{
			_logger.LogInformation("Revoke called");

			var username = HttpContext.User.Identity?.Name;

			if (username is null)
				return Unauthorized();

			var user = await _userManager.FindByNameAsync(username);

			if (user is null)
				return Unauthorized();

			user.RefreshToken = null; // no need to also set expiry of refresh to null

			await _userManager.UpdateAsync(user);

			_logger.LogInformation("Revoke succeeded");

			return Ok();
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
				expires: DateTime.UtcNow.AddMinutes(1), // Adjust token expiry as needed
				claims: authClaims,
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

			return token;
		}

		private static string _hashToken(string token)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
				var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
				return hash;
			}
		}
	}
}