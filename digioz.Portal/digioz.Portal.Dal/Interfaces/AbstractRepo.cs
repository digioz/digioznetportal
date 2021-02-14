using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Interfaces
{
    public class AbstractRepo<T> : IRepo<T> where T : class
    {
        protected string _connectionString;

        public AbstractRepo(IConfigHelper config) {
            _connectionString = config.GetConnectionString();
        }

        public T Get(object id) {
            using var db = new digiozPortalContext();
            return db.Set<T>().Find(id);
        }

        public List<T> GetAll() {
            using var db = new digiozPortalContext();
            return db.Set<T>().ToList();
        }

        public void Add(T model) {
            using var db = new digiozPortalContext();
            db.Set<T>().Add(model);
            db.SaveChanges();
        }

        public void Edit(T model) {
            using var db = new digiozPortalContext();

            if (model != null)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Delete(T model) {
            using var db = new digiozPortalContext();
            db.Entry(model).State = EntityState.Deleted;
            db.SaveChanges();
        }

        public List<T> GetQuery(Query query) 
        {
            var queryString = GetQueryParse(query);
            using var db = new digiozPortalContext();
            return db.Set<T>().FromSqlRaw(queryString).ToList();
        }

        public List<T> GetQueryString(string query) {
            using var db = new digiozPortalContext();
            return db.Set<T>().FromSqlRaw(query).ToList();
        }

        private string GetQueryParse(Query query) 
        {
            var queryString = $"SELECT {(query.Top > 0 ? $"TOP {query.Top}" : "")} " +
                (string.IsNullOrWhiteSpace(query.Select) ? " * " : query.Select)  + " FROM " + typeof(T).Name +
                (!string.IsNullOrWhiteSpace(query.Where) ? " WHERE " + query.Where : "") + 
                (!string.IsNullOrWhiteSpace(query.OrderBy) ? " ORDER BY " + query.OrderBy : "");

            return queryString;
        }

		List<T> IRepo<T>.GetGeneric(Expression<Func<T, bool>> where)
		{
            using var db = new digiozPortalContext();
            return db.Set<T>().Where(where).ToList();
        }
	}
}
