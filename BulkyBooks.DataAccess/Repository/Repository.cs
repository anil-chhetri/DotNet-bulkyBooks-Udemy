using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository
{
    public class Repository<T> : IRepository.IRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }
        public void Add(T Entity)
        {
            dbSet.Add(Entity);
        }

        public void AddRange(IEnumerable<T> Entity)
        {
            dbSet.AddRange(Entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter, string IncludeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if(filter != null)
            {
                query = query.Where(filter);
            }

            Debug.WriteLine(query.ToQueryString());

            if(IncludeProperties != null)
            {
                foreach (string properties in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include<T>(properties.Trim());
                }
            }

            return query.ToList();
        }

       

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string IncludeProperties = null)
        {
            IQueryable<T> query = dbSet;

            query = query.Where(filter);
            if (IncludeProperties != null)
            {
                foreach (var properties in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include<T>(properties);
                }
            }


            return query.FirstOrDefault();

        }

       
        public void Remove(T Entity)
        {
            dbSet.Remove(Entity);
        }

        public void RemoveRange(IEnumerable<T> Entity)
        {
            dbSet.RemoveRange(Entity);
        }
    }
}
