using Microsoft.AspNetCore.Mvc;
using WebApiAuthentication.Services;
namespace WebApiAuthentication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserRoleController : ControllerBase
{
	private readonly UserRoleService _userRoleService;

	public UserRoleController(UserRoleService userRoleService)
	{
		_userRoleService = userRoleService;
	}

	[HttpGet("{userId}/roles")]
	public async Task<IActionResult> GetRolesForUser(string userId)
	{
		var roles = await _userRoleService.GetRolesForUser(userId);
		if (roles == null || roles.Count == 0)
		{
			return NotFound($"No roles found for user with ID {userId}.");
		}
		return Ok(new { UserId = userId, Roles = roles });
	}
	[HttpPost("assign")]
	public async Task<IActionResult> AssignRoleToUser([FromBody] UserRoleAssignmentModel model)
	{
		var result = await _userRoleService.AssignRoleToUser(model.UserId, model.RoleName);
		if (result.Succeeded)
		{
			return Ok($"Role '{model.RoleName}' assigned to user successfully.");
		}

		return BadRequest(result.Errors);
	}

	[HttpPost("remove")]
	public async Task<IActionResult> RemoveRoleFromUser([FromBody] UserRoleAssignmentModel model)
	{
		var result = await _userRoleService.RemoveRoleFromUser(model.UserId, model.RoleName);
		if (result.Succeeded)
		{
			return Ok($"Role '{model.RoleName}' removed from user successfully.");
		}

		return BadRequest(result.Errors);
	}

	[HttpGet("{userId}/roles/{roleName}/isInRole")]
	public async Task<IActionResult> IsUserInRole(string userId, string roleName)
	{
		bool isInRole = await _userRoleService.IsUserInRole(userId, roleName);
		return Ok(new { UserId = userId, RoleName = roleName, IsInRole = isInRole });
	}

}

public class UserRoleAssignmentModel
{
	public required string UserId { get; set; }
	public required string RoleName { get; set; }
}
