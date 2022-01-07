using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T: class
    {
        // properties are comma separated values of foreign key.
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string IncludeProperties = null);

        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string IncludeProperties = null);

        void Add(T Entity);

        void AddRange(IEnumerable<T> Entity);

        void Remove(T Entity);

        void RemoveRange(IEnumerable<T> Entity);
    }
}
