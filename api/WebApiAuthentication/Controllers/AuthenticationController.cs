using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiAuthentication.DataAccess.Authentication;
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

		public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthenticationController> logger, JwtAuthenticationService jwtAuthenticationService)
		{
			_userManager = userManager;
			_configuration = configuration;
			_logger = logger;
			_jwtAuthenticationService = jwtAuthenticationService;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegistrationModel model)
		{
			var result = await _jwtAuthenticationService.Register(model);
			if (!result.IsSuccess)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			var result = await _jwtAuthenticationService.Login(model);
			if (!result.IsSuccess)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpPost("Refresh")]
		public async Task<IActionResult> Refresh([FromBody] RefreshModel model)  // accepts a refresh token and returns a new JWT
		{
			var result = await _jwtAuthenticationService.Refresh(model);
			if (!result.IsSuccess)
			{
				return BadRequest(result);
			}
			return Ok(result);
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

	}
}
