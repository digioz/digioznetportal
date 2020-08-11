using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;
using digiozPortal.Utilities;


namespace digiozPortal.DAL.Interfaces
{
    public class AbstractRepo<T> : IRepo<T> where T : class
    {
        private string _connectionString;

        public AbstractRepo(IConfigHelper config) {
            _connectionString = config.GetConnectionString();
        }

        public T Get(int id) {
            using (var connection = new SqlConnection(_connectionString)) {
                return connection.Get<T>(id);
            }
        }

        public List<T> GetAll() {
            using (var connection = new SqlConnection(_connectionString)) {
                return connection.GetAll<T>().ToList();
            }
        }

        public void Add(T model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(T model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(T model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
