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
    public class LinkCategoryRepo
    {
        private string _connectionString;

        public LinkCategoryRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public LinkCategory Get(int id) {
            var model = new LinkCategory();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<LinkCategory>(id);
            }

            return model;
        }

        public List<LinkCategory> GetAll() {
            var models = new List<LinkCategory>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<LinkCategory>().ToList();
            }

            return models;
        }

        public void Add(LinkCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(LinkCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(LinkCategory model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
