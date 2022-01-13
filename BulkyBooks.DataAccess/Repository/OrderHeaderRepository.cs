using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext context;

        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(OrderHeader obj)
        {
            context.Entry<OrderHeader>(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public void UpdateStatus(int orderId, string OrderStatus, string PaymentStatus = null)
        {
            var fromdb = context.orderHeader.FirstOrDefault(o => o.Id == orderId);
            if(fromdb != null)
            {
                fromdb.OrderStatus = OrderStatus;
                if(PaymentStatus != null)
                {
                    fromdb.PaymentStatus = PaymentStatus;
                }
            }
        }

        public void UpdateStripSessionId(int orderId, string sessionId, string paymentIntentId)
        {
            var fromDb = context.orderHeader.FirstOrDefault(o => o.Id == orderId);

            if(fromDb != null)
            {
                fromDb.SessionId = sessionId;
                fromDb.PaymentIntentId = paymentIntentId;
            }
        }
    }
}
