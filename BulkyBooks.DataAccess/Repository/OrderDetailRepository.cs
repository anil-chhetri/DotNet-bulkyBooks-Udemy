using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext context;

        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(OrderDetail obj)
        {
            context.Entry<OrderDetail>(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
    }
}
