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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdatePaymentSession(int id, string sessionId, string paymentIntentId)
        {
            var OrderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            OrderHeader.SessionId = sessionId;
            OrderHeader.PaymentIntentId = paymentIntentId;

        }


        public void UpdateStatus(int id, string OrderSatus, string? PaymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.PaymentDate = DateTime.Now;
                orderFromDb.OrderStatus = OrderSatus;
                orderFromDb.PaymentStatus = PaymentStatus;
                
            }
        }
    }
}
