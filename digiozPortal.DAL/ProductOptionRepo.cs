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
    public class ProductOptionRepo
    {
        private string _connectionString;

        public ProductOptionRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public ProductOption Get(string id) {
            var model = new ProductOption();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<ProductOption>(id);
            }

            return model;
        }

        public List<ProductOption> GetAll() {
            var models = new List<ProductOption>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<ProductOption>().ToList();
            }

            return models;
        }

        public void Add(ProductOption model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(ProductOption model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(ProductOption model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
