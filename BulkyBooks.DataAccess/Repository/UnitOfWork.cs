using BulkyBooks.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            this.Category = new CategoryRepository(context);
            this.CoverType = new CoverTypeRepository(context);
            this.Product = new ProductRepository(context);
            this.Company = new CompanyRepository(context);
            this.ApplicationUser = new ApplicationUserRepository(context);
            this.ShoppingCart = new ShoppingCartRepository(context);
            this.OrderHeader = new OrderHeaderRepository(context);
            this.OrderDetail = new OrderDetailRepository(context);

        }

        public ICategoryRepository Category { get; private set; }

        public ICoverTypeRepository CoverType { get; private set; }

        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
