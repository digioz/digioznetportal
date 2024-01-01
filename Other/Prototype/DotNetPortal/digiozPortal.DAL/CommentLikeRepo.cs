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
    public class CommentLikeRepo
    {
        private string _connectionString;

        public CommentLikeRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public CommentLike Get(string id) {
            var model = new CommentLike();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<CommentLike>(id);
            }

            return model;
        }

        public List<CommentLike> GetAll() {
            var models = new List<CommentLike>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<CommentLike>().ToList();
            }

            return models;
        }

        public void Add(CommentLike model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(CommentLike model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(CommentLike model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
