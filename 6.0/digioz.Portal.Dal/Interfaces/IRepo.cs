using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;

namespace digioz.Portal.Dal.Interfaces
{
    public interface IRepo<T>  
    {
        T Get(object id);
        List<T> GetAll();
        void Add(T entity);
        void Edit(T entity);
        void Delete(T entity);
        List<T> GetQuery(Query query);
        List<T> GetQueryString(string query);
        List<T> GetGeneric(Expression<Func<T, bool>> where);
    }
}
