using System;
using System.Collections.Generic;
using System.Text;

namespace digiozPortal.DAL.Interfaces
{
    public interface IRepo<T>  
    {
        T Get(int id);
        List<T> GetAll();
        void Add(T entity);
        void Edit(T entity);
        void Delete(T entity);
    }
}
