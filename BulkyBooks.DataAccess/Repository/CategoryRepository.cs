using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        
        public void Update(Category obj)
        {
            context.Entry<Category>(obj).State = EntityState.Modified;

            //or
            //context.Update(Category);
        }
    }
}
