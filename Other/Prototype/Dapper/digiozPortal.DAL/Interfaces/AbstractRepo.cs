using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using digiozPortal.BO;
using digiozPortal.Utilities;


namespace digiozPortal.DAL.Interfaces
{
    public class AbstractRepo<T> : IRepo<T> where T : class
    {
        protected string _connectionString;

        public AbstractRepo(IConfigHelper config) {
            _connectionString = config.GetConnectionString();
        }

        public T Get(object id) {
            using var connection = new SqlConnection(_connectionString);
            return connection.Get<T>(id);
        }

        public List<T> GetAll() {
            using var connection = new SqlConnection(_connectionString);
            return connection.GetAll<T>().ToList();
        }

        public void Add(T model) {
            using var connection = new SqlConnection(_connectionString);
            connection.Insert(model);
        }

        public void Edit(T model) {
            using var connection = new SqlConnection(_connectionString);
            connection.Update(model);
        }

        public void Delete(T model) {
            using var connection = new SqlConnection(_connectionString);
            connection.Delete(model);
        }

        public IEnumerable<T> Get(Query query) 
        {
            var queryString = GetQuery(query);
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<T>(queryString);
        }

        public IEnumerable<T> Get(string query) {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<T>(query);
        }
        private string GetQuery(Query query) 
        {
            var queryString = $"SELECT {(query.Top > 0 ? $"TOP {query.Top}" : "")} " +
                (string.IsNullOrWhiteSpace(query.Select) ? " * " : query.Select)  + " FROM " + typeof(T).Name +
                (!string.IsNullOrWhiteSpace(query.Where) ? " WHERE " + query.Where : "") + 
                (!string.IsNullOrWhiteSpace(query.OrderBy) ? " ORDER BY " + query.OrderBy : "");

            return queryString;
        }
    }
}
