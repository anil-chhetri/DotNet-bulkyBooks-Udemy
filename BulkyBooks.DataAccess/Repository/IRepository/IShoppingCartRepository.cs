using BulkyBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        int IncreaseOrder(ShoppingCart shoppingCart, int count);

        int DecreseOrder(ShoppingCart shoppingCart, int count);
    }
}
