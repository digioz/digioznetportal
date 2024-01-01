using digioz.Portal.Bo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace digioz.Portal.Bll.Interfaces
{
    public interface ILogic <T>
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
