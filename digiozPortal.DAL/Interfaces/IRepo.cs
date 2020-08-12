using System.Collections.Generic;

namespace digiozPortal.DAL.Interfaces
{
    public interface IRepo<T>  
    {
        T Get(object id);
        List<T> GetAll();
        void Add(T entity);
        void Edit(T entity);
        void Delete(T entity);
    }
}
