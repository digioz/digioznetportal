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
    public class LinkRepo
    {
        private string _connectionString;

        public LinkRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Link Get(int id) {
            var model = new Link();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Link>(id);
            }

            return model;
        }

        public List<Link> GetAll() {
            var models = new List<Link>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Link>().ToList();
            }

            return models;
        }

        public void Add(Link model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Link model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Link model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
