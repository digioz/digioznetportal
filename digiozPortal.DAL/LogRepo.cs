using System;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using Dapper.Contrib.Extensions;
using digiozPortal.Utilities;
using digiozPortal.BO;

namespace digiozPortal.DAL
{
    public class LogRepo
    {
        private string _connectionString;

        public LogRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Log Get(int id) {
            var model = new Log();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Log>(id);
            }

            return model;
        }

        public List<Log> GetAll() {
            var models = new List<Log>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Log>().ToList();
            }

            return models;
        }

        public void Add(Log model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Log model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Log model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
