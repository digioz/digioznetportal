using System.Collections.Generic;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL.Interfaces
{
    public abstract class AbstractLogic<T> : ILogic<T>
    {
        IRepo<T> _repo;
        public AbstractLogic(IRepo<T> repo) {
            _repo = repo;
        }
        public T Get(object id) {
            return _repo.Get(id);
        }

        public List<T> GetAll() {
            return _repo.GetAll();
        }

        public void Add(T entity) {
            _repo.Add(entity);
        }

        public void Edit(T entity) {
            _repo.Edit(entity);
        }

        public void Delete(int id) {
            var entity = _repo.Get(id);
            if (entity != null) {
                _repo.Delete(entity);
            }
        }
    }
}
