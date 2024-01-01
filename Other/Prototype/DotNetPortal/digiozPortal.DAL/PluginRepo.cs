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
    public class PluginRepo
    {
        private string _connectionString;

        public PluginRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Plugin Get(int id) {
            var model = new Plugin();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Plugin>(id);
            }

            return model;
        }

        public List<Plugin> GetAll() {
            var models = new List<Plugin>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Plugin>().ToList();
            }

            return models;
        }

        public void Add(Plugin model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Plugin model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Plugin model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
