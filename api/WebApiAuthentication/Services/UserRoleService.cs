using Microsoft.AspNetCore.Identity;
using WebApiAuthentication.DataAccess.Models.Entities;
namespace WebApiAuthentication.Services
{
    public class UserRoleService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserRoleService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IList<string>> GetRolesForUser(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return new List<string>();
			}

			return await _userManager.GetRolesAsync(user);
		}

		public async Task<IdentityResult> AssignRoleToUser(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "User not found." });
			}

			var result = await _userManager.AddToRoleAsync(user, roleName);
			return result;
		}

		public async Task<IdentityResult> RemoveRoleFromUser(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "User not found." });
			}

			if (!await _userManager.IsInRoleAsync(user, roleName))
			{
				return IdentityResult.Failed(new IdentityError { Description = $"User is not in the role: {roleName}." });
			}

			var result = await _userManager.RemoveFromRoleAsync(user, roleName);
			return result;
		}

		public async Task<bool> IsUserInRole(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return false;
			}

			return await _userManager.IsInRoleAsync(user, roleName);
		}

	}

}