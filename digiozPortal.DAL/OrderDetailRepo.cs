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
    public class OrderDetailRepo
    {
        private string _connectionString;

        public OrderDetailRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public OrderDetail Get(string id) {
            var model = new OrderDetail();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<OrderDetail>(id);
            }

            return model;
        }

        public List<OrderDetail> GetAll() {
            var models = new List<OrderDetail>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<OrderDetail>().ToList();
            }

            return models;
        }

        public void Add(OrderDetail model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(OrderDetail model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(OrderDetail model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
