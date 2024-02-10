using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApiAuthentication.DataAccess.Constants;
namespace WebApiAuthentication.Services
{

	public class RoleService
	{
		private readonly RoleManager<IdentityRole> _roleManager;

		public RoleService(RoleManager<IdentityRole> roleManager)
		{
			_roleManager = roleManager;
		}

		public async Task<List<IdentityRole>> GetAllRolesAsync()
		{
			return await _roleManager.Roles.ToListAsync();
		}

		public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
		{
			return await _roleManager.FindByIdAsync(roleId);
		}

		public async Task<IdentityResult> CreateRoleAsync(string roleName)
		{
			var isPredefinedRole = USER_ROLES.GetAllRoles().Contains(roleName.ToUpper());

			if (!isPredefinedRole)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Role creation is not allowed. Only predefined roles can be created." });
			}

			var roleExists = await _roleManager.RoleExistsAsync(roleName);
			if (roleExists)
			{
				return IdentityResult.Failed(new IdentityError { Description = $"Role '{roleName}' already exists." });
			}

			// Create the role with the original case for display purposes
			return await _roleManager.CreateAsync(new IdentityRole(roleName));
		}


		public async Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName)
		{
			var role = await _roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
			}

			role.Name = newRoleName;
			var result = await _roleManager.UpdateAsync(role);
			return result;
		}

		public async Task<IdentityResult> DeleteRoleAsync(string roleId)
		{
			var role = await _roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
			}

			var result = await _roleManager.DeleteAsync(role);
			return result;
		}
	}

}