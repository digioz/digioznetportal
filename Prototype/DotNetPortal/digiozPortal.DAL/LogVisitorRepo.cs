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
    public class LogVisitorRepo
    {
        private string _connectionString;

        public LogVisitorRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public LogVisitor Get(int id) {
            var model = new LogVisitor();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<LogVisitor>(id);
            }

            return model;
        }

        public List<LogVisitor> GetAll() {
            var models = new List<LogVisitor>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<LogVisitor>().ToList();
            }

            return models;
        }

        public void Add(LogVisitor model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(LogVisitor model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(LogVisitor model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
