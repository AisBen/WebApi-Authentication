using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiAuthentication.Services;
namespace WebApiAuthentication.Controllers
{


	[Route("api/[controller]")]
	[ApiController]
	public class RolesController : ControllerBase
	{
		private readonly RoleService _roleService;

		public RolesController(RoleService roleService)
		{
			_roleService = roleService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<IdentityRole>>> GetAllRoles()
		{
			var roles = await _roleService.GetAllRolesAsync();
			return Ok(roles);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetRoleById(string id)
		{
			var role = await _roleService.GetRoleByIdAsync(id);
			if (role == null)
			{
				return NotFound($"Role with ID {id} not found.");
			}
			return Ok(role);
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole([FromBody] string roleName)
		{
			if (string.IsNullOrWhiteSpace(roleName))
			{
				return BadRequest("Role name must be provided.");
			}

			var result = await _roleService.CreateRoleAsync(roleName);
			if (result.Succeeded)
			{
				return Ok($"Role '{roleName}' created successfully.");
			}

			return BadRequest(result.Errors);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateRole(string id, [FromBody] string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				return BadRequest("New role name must be provided.");
			}

			var result = await _roleService.UpdateRoleAsync(id, newName);
			if (result.Succeeded)
			{
				return Ok($"Role with ID {id} updated to '{newName}' successfully.");
			}

			return BadRequest(result.Errors);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteRole(string id)
		{
			var result = await _roleService.DeleteRoleAsync(id);
			if (result.Succeeded)
			{
				return Ok($"Role with ID {id} deleted successfully.");
			}

			return BadRequest(result.Errors);
		}
	}

}