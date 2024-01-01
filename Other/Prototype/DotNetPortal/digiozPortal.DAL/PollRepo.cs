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
    public class PollRepo
    {
        private string _connectionString;

        public PollRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Poll Get(string id) {
            var model = new Poll();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Poll>(id);
            }

            return model;
        }

        public List<Poll> GetAll() {
            var models = new List<Poll>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Poll>().ToList();
            }

            return models;
        }

        public void Add(Poll model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Poll model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Poll model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
