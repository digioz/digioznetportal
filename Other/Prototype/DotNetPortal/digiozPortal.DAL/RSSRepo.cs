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
    public class RssRepo
    {
        private string _connectionString;

        public RssRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Rss Get(int id) {
            var model = new Rss();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Rss>(id);
            }

            return model;
        }

        public List<Rss> GetAll() {
            var models = new List<Rss>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Rss>().ToList();
            }

            return models;
        }

        public void Add(Rss model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Rss model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Rss model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
