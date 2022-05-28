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
    public class CompanyRepository : Repository<Company>,ICompanyRepository
    {
        private ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db): base(db)
        {
            _db = db;   
        }
        

        public void Update(Company company)
        {
            _db.Companies.Update(company);
        }
    }
}
