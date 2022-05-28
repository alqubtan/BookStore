using BulkyBook.AccessData;
using BulkyBook.Models;
using BulkyBook.AccessData.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.AccessData.Repository
{
    public class CategoryRepository : Repository<Category>,ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db): base(db)
        {
            _db = db;   
        }
        

        public void Update(Category cat)
        {
            _db.Categories.Update(cat);
        }
    }
}
