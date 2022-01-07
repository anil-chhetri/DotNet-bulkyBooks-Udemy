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
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext context;

        public CoverTypeRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public void Update(CoverType obj)
        {
            context.Entry<CoverType>(obj).State = EntityState.Modified;
        }
    }
}
