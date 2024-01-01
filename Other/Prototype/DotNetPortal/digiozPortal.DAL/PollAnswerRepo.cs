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
    public class PollAnswerRepo
    {
        private string _connectionString;

        public PollAnswerRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public PollAnswer Get(string id) {
            var model = new PollAnswer();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<PollAnswer>(id);
            }

            return model;
        }

        public List<PollAnswer> GetAll() {
            var models = new List<PollAnswer>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<PollAnswer>().ToList();
            }

            return models;
        }

        public void Add(PollAnswer model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(PollAnswer model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(PollAnswer model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
