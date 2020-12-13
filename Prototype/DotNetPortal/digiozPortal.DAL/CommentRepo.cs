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
    public class CommentRepo
    {
        private string _connectionString;

        public CommentRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Comment Get(string id) {
            var model = new Comment();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Comment>(id);
            }

            return model;
        }

        public List<Comment> GetAll() {
            var models = new List<Comment>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Comment>().ToList();
            }

            return models;
        }

        public void Add(Comment model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Comment model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Comment model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
