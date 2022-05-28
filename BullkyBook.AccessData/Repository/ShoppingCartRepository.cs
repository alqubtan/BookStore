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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db): base(db)
        {
            _db = db;   
        }

        public int DecrementCount(ShoppingCart cart, int Count)
        {
            cart.Count -= Count;
            return cart.Count;
        }

        public int IncrementCount(ShoppingCart cart, int Count)
        {
            cart.Count += Count;
            return cart.Count;
        }
    }
}
