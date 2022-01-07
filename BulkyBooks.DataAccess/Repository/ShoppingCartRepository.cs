using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext context;

        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public int DecreseOrder(ShoppingCart shoppingCart, int count)
        {
            var fromDb = context.ShoppingCart.FirstOrDefault(c => c.ApplicationUserId == shoppingCart.ApplicationUserId && c.ProductId == shoppingCart.ProductId);
            fromDb.Count -= count;
            if (fromDb.Count < 1)
            {
                context.ShoppingCart.Remove(fromDb);
            }
            else
            {
                context.Entry<ShoppingCart>(fromDb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            return fromDb.Count;
        }

        public int IncreaseOrder(ShoppingCart shoppingCart, int count)
        {
            var fromDb = context.ShoppingCart.FirstOrDefault(c => c.ApplicationUserId == shoppingCart.ApplicationUserId && c.ProductId == shoppingCart.ProductId);
            fromDb.Count += count;
            context.Entry<ShoppingCart>(fromDb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            return fromDb.Count;
        }
    }
}
