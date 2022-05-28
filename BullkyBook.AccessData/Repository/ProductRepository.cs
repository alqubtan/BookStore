using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.AccessData.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Product product)
        {
            var productFromdb = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            if (productFromdb != null)
            {
                productFromdb.Title = product.Title;
                productFromdb.Description = product.Description;
                productFromdb.Price = product.Price;
                productFromdb.CategoryId = product.CategoryId;
                productFromdb.CoverTypeId = product.CoverTypeId;
                productFromdb.Price100 = product.Price100;
                productFromdb.Price50 = product.Price50;
                productFromdb.CoverType = product.CoverType;
                productFromdb.ISPN = product.ISPN;
                productFromdb.ListPrice = product.ListPrice;
                if (product.ImageUrl != null)
                {
                    productFromdb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
