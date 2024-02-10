using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuthentication.Authentication;
using WebApiAuthentication.DataAccess.Entities;

namespace WebApiAuthentication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<LibraryUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthenticationController> _logger;

		public AuthenticationController(UserManager<LibraryUser> userManager, IConfiguration configuration, ILogger<AuthenticationController> logger)
		{
			_userManager = userManager;
			_configuration = configuration;
			_logger = logger;
		}

		[HttpPost("Register")]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Register([FromBody] RegistrationModel model)
		{
			_logger.LogInformation("Register called");

			var existingUser = await _userManager.FindByNameAsync(model.Username);

			if (existingUser != null)
				return Conflict("User already exists.");

			var newUser = new LibraryUser
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
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			_logger.LogInformation("Login called");

			var user = await _userManager.FindByNameAsync(model.Username);

			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
				return Unauthorized();

			JwtSecurityToken token = _generateJwt(model.Username);

			var refreshToken = _generateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiry = DateTime.UtcNow.AddSeconds(20); // refresh token expiry must be larger than access token expiry
																	  //user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(3);

			await _userManager.UpdateAsync(user);

			_logger.LogInformation("Login succeeded");

			return Ok(new LoginResponse
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				AccessTokenExpiration = token.ValidTo,
				RefreshToken = refreshToken
			});
		}

		[HttpPost("Refresh")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Refresh([FromBody] RefreshModel model) // accepts a refresh token and returns a new JWT
		{
			_logger.LogInformation("Refresh called");

			var principal = _getPrincipalFromExpiredToken(model.AccessToken);

			if (principal?.Identity?.Name is null)
				return Forbid();

			var user = await _userManager.FindByNameAsync(principal.Identity.Name);

			if (user is null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
				return Forbid();

			var token = _generateJwt(principal.Identity.Name);

			_logger.LogInformation("Refresh succeeded");

			return Ok(new LoginResponse
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				AccessTokenExpiration = token.ValidTo,
				RefreshToken = model.RefreshToken // could also refresh the refresh token here
			});
		}

		[Authorize]
		[HttpDelete("Revoke")] // after this is called, you cant use refresh call to get a new JWT
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

		private JwtSecurityToken _generateJwt(string username)
		{
			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				_configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured")));

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				expires: DateTime.UtcNow.AddSeconds(5),
				claims: authClaims,
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
				);

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
}
