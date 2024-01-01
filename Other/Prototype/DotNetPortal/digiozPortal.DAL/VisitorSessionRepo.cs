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
    public class VisitorSessionRepo
    {
        private string _connectionString;

        public VisitorSessionRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public VisitorSession Get(int id) {
            var model = new VisitorSession();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<VisitorSession>(id);
            }

            return model;
        }

        public List<VisitorSession> GetAll() {
            var models = new List<VisitorSession>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<VisitorSession>().ToList();
            }

            return models;
        }

        public void Add(VisitorSession model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(VisitorSession model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(VisitorSession model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
