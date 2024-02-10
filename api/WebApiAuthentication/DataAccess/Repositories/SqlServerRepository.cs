using WebApiAuthentication.DataAccess.Context;
using WebApiAuthentication.DataAccess.Models.Entities;

namespace WebApiAuthentication.DataAccess.Repositories
{
	public class SqlServerRepository : IReviewRepository
	{
		private readonly TheDbContext _theDbContext;

		public SqlServerRepository(TheDbContext reviewContext)
		{
			_theDbContext = reviewContext;
		}

		public IQueryable<BookReview> AllReviews => _theDbContext.BookReviews;

		public void Create(BookReview review) => _theDbContext.BookReviews.Add(review);

		public void Remove(BookReview review) => _theDbContext.BookReviews.Remove(review);

		public void SaveChanges() => _theDbContext.SaveChanges();
	}
}
