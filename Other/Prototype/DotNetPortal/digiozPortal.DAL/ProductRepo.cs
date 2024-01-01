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
    public class ProductRepo
    {
        private string _connectionString;

        public ProductRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Product Get(string id) {
            var model = new Product();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Product>(id);
            }

            return model;
        }

        public List<Product> GetAll() {
            var models = new List<Product>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Product>().ToList();
            }

            return models;
        }

        public void Add(Product model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Product model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Product model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
