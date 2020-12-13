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
    public class ModuleRepo
    {
        private string _connectionString;

        public ModuleRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Module Get(int id) {
            var model = new Module();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Module>(id);
            }

            return model;
        }

        public List<Module> GetAll() {
            var models = new List<Module>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Module>().ToList();
            }

            return models;
        }

        public void Add(Module model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Module model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Module model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
