using BulkyBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int orderId, string OrderStatus, string? PaymentStatus=null);

        void UpdateStripSessionId(int orderId, string sessionId, string paymentIntentId);
    }
}
