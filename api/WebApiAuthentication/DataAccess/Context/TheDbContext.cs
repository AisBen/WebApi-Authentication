using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAuthentication.DataAccess.Models.Entities;

namespace WebApiAuthentication.DataAccess.Context
{
    public class TheDbContext : IdentityDbContext<ApplicationUser>
	{
		public TheDbContext(DbContextOptions<TheDbContext> options)
			: base(options)
		{
		}

		public DbSet<BookReview> BookReviews { get; set; }
	}
}
