using System.Collections.Generic;
using digioz.Portal.Dal.Interfaces;


namespace digioz.Portal.Bll.Interfaces
{
    public abstract class AbstractLogic<T> : ILogic<T>
    {
        private readonly IRepo<T> _repo;
        public AbstractLogic(IRepo<T> repo)
        {
            _repo = repo;
        }
        public T Get(object id)
        {
            return _repo.Get(id);
        }

        public List<T> GetAll()
        {
            return _repo.GetAll();
        }

        public void Add(T entity)
        {
            _repo.Add(entity);
        }

        public void Edit(T entity)
        {
            _repo.Edit(entity);
        }

        public void Delete(T entity)
        {
            if (entity != null)
            {
                _repo.Delete(entity);
            }
        }
    }
}
