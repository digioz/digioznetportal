using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using digioz.Portal.Bo.ViewModels;
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

        public List<T> GetQuery(Query query)
		{
            return _repo.GetQuery(query);
		}

		public List<T> GetQueryString(string query)
		{
            return _repo.GetQueryString(query);
        }

		List<T> ILogic<T>.GetGeneric(Expression<Func<T, bool>> where)
		{
            return _repo.GetGeneric(where);
        }
	}
}
