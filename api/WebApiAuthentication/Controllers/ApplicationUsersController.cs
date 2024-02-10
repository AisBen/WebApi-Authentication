using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAuthentication.DataAccess.Context;
using WebApiAuthentication.DataAccess.Models.Entities;


namespace WebApiAuthentication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ApplicationUsersController : ControllerBase
	{
		private readonly TheDbContext _db;
		public ApplicationUsersController(TheDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetAllUsers()
		{
			var users = await _db.Users.ToListAsync();

			return Ok(users);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				return NotFound($"User with ID {id} not found.");
			}
			_db.Users.Remove(user);
			await _db.SaveChangesAsync();
			return Ok($"User with ID {id} deleted successfully.");
		}
	}
}
