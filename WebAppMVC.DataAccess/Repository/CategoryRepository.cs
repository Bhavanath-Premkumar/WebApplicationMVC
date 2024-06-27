using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplicationMVC.DataAccess.Data;
using WebApplicationMVC.DataAccess.Repository.IRepository;
using WebApplicationMVC.Models.Models;

namespace WebApplicationMVC.DataAccess.Repository
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
		private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

		public void Update(Category category)
		{
			_db.Update(category);		
		}
	}
}
