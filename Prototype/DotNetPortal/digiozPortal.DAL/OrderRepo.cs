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
    public class OrderRepo
    {
        private string _connectionString;

        public OrderRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Order Get(string id) {
            var model = new Order();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Order>(id);
            }

            return model;
        }

        public List<Order> GetAll() {
            var models = new List<Order>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Order>().ToList();
            }

            return models;
        }

        public void Add(Order model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Order model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Order model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
