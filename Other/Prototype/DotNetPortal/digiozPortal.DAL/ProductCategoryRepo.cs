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
    public class ProductCategoryRepo
    {
        private string _connectionString;

        public ProductCategoryRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public ProductCategory Get(string id) {
            var model = new ProductCategory();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<ProductCategory>(id);
            }

            return model;
        }

        public List<ProductCategory> GetAll() {
            var models = new List<ProductCategory>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<ProductCategory>().ToList();
            }

            return models;
        }

        public void Add(ProductCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(ProductCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(ProductCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
