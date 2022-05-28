using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.AccessData.Repository.IRepository
{
    public interface IRepository<T> where T : class 
    {

        // let's say that our repo is about category
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includedProps = null, bool tracked = true);
        IEnumerable<T> GetAll(Expression<Func<T, bool>> ?predicate = null, string? includedProps = null);

        // add category
        void Add(T entity);
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

       

    }
}
