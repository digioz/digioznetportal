using System.Collections.Generic;

namespace digioz.Portal.Bll.Interfaces
{
    public interface ILogic <T>
    {
        T Get(object id);
        List<T> GetAll();
        void Add(T entity);
        void Edit(T entity);
        void Delete(T entity);
    }
}
