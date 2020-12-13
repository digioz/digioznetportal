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
    public class ChatRepo
    {
        private string _connectionString;

        public ChatRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Chat Get(int id) {
            var model = new Chat();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Chat>(id);
            }

            return model;
        }

        public List<Chat> GetAll() {
            var models = new List<Chat>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Chat>().ToList();
            }

            return models;
        }

        public void Add(Chat model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Chat model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Chat model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
