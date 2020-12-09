using System.Collections.Generic;
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
        IEnumerable<T> Get(Query query);
        IEnumerable<T> Get(string query);
    }
}
