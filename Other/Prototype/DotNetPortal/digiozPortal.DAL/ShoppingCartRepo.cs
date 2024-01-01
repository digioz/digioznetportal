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
    public class ShoppingCartRepo
    {
        private string _connectionString;

        public ShoppingCartRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public ShoppingCart Get(string id) {
            var model = new ShoppingCart();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<ShoppingCart>(id);
            }

            return model;
        }

        public List<ShoppingCart> GetAll() {
            var models = new List<ShoppingCart>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<ShoppingCart>().ToList();
            }

            return models;
        }

        public void Add(ShoppingCart model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(ShoppingCart model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(ShoppingCart model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
