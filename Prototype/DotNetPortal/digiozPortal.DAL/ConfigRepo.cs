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
    public class ConfigRepo
    {
        private string _connectionString;

        public ConfigRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Config Get(string id) {
            var model = new Config();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Config>(id);
            }

            return model;
        }

        public List<Config> GetAll() {
            var models = new List<Config>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Config>().ToList();
            }

            return models;
        }

        public void Add(Config model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Config model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Config model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
