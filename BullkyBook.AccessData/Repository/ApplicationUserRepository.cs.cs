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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db): base(db)
        {
            _db = db;   
        }
        

      
    }
}
