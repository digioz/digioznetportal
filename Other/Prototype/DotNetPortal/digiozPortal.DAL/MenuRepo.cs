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
    public class MenuRepo
    {
        private string _connectionString;

        public MenuRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Menu Get(int id) {
            var model = new Menu();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Menu>(id);
            }

            return model;
        }

        public List<BO.Menu> GetAll() {
            var models = new List<Menu>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<BO.Menu>().ToList();
            }

            return models;
        }

        public void Add(Menu model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Menu model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Menu model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
