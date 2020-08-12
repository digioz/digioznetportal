using System.Collections.Generic;

namespace digiozPortal.BLL.Interfaces
{
    public interface ILogic <T>
    {
        T Get(object id);
        List<T> GetAll();
        void Add(T entity);
        void Edit(T entity);
        void Delete(int id);
    }
}
