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
    public class PageRepo
    {
        private string _connectionString;

        public PageRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Page Get(int id) {
            var model = new Page();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Page>(id);
            }

            return model;
        }

        public List<Page> GetAll() {
            var models = new List<Page>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Page>().ToList();
            }

            return models;
        }

        public void Add(Page model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Page model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Page model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
