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
    public class VisitorInfoRepo
    {
        private string _connectionString;

        public VisitorInfoRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public VisitorInfo Get(int id) {
            var model = new VisitorInfo();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<VisitorInfo>(id);
            }

            return model;
        }

        public List<VisitorInfo> GetAll() {
            var models = new List<VisitorInfo>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<VisitorInfo>().ToList();
            }

            return models;
        }

        public void Add(VisitorInfo model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(VisitorInfo model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(VisitorInfo model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
