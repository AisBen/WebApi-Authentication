using Microsoft.AspNetCore.Identity;
using WebApiAuthentication.DataAccess.Constants;
using WebApiAuthentication.DataAccess.Context;
using WebApiAuthentication.DataAccess.Models.Entities;
using WebApiAuthentication.DataAccess.Repositories;

namespace WebApiAuthentication.Services
{
	public static class ServiceExtensions
	{
		public static IServiceCollection RegisterServices(this IServiceCollection services)
		{
			// Custom Repositories
			services.AddScoped<IReviewRepository, SqlServerRepository>();

			// Identity Services
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ReviewContext>();

			// Custom Services
			services.AddScoped<UserManager<ApplicationUser>>();
			services.AddScoped<RoleService>();
			services.AddScoped<UserRoleService>();
			services.AddScoped<JwtAuthenticationService>();

			// Register authorization policies
			services.AddAuthorization(options =>
			{
				options.AddPolicy(AUTHORIZE_POLICY.ARTIST_ADMIN, policy =>
					policy.RequireRole(USER_ROLES.ARTIST, USER_ROLES.ADMIN));

				options.AddPolicy(AUTHORIZE_POLICY.MANAGER_ADMIN, policy =>
					policy.RequireRole(USER_ROLES.MANAGER, USER_ROLES.ADMIN));

				options.AddPolicy(AUTHORIZE_POLICY.ONLY_ARTIST, policy =>
					policy.RequireRole(USER_ROLES.ARTIST));

				options.AddPolicy(AUTHORIZE_POLICY.ONLY_MANAGER, policy =>
					policy.RequireRole(USER_ROLES.MANAGER));

				options.AddPolicy(AUTHORIZE_POLICY.ALL_ROLES, policy =>
					policy.RequireRole(USER_ROLES.ARTIST, USER_ROLES.MANAGER, USER_ROLES.ADMIN));
			});

			return services;
		}
	}
}
