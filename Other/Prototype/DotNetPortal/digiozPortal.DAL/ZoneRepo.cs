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
    public class ZoneRepo
    {
        private string _connectionString;

        public ZoneRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Zone Get(int id) {
            var model = new Zone();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Zone>(id);
            }

            return model;
        }

        public List<Zone> GetAll() {
            var models = new List<Zone>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Zone>().ToList();
            }

            return models;
        }

        public void Add(Zone model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Zone model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Zone model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
