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
    public class CommentConfigRepo
    {
        private string _connectionString;

        public CommentConfigRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public CommentConfig Get(string id) {
            var model = new CommentConfig();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<CommentConfig>(id);
            }

            return model;
        }

        public List<CommentConfig> GetAll() {
            var models = new List<CommentConfig>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<CommentConfig>().ToList();
            }

            return models;
        }

        public void Add(CommentConfig model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(CommentConfig model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(CommentConfig model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
