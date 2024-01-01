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
    public class PollVoteRepo
    {
        private string _connectionString;

        public PollVoteRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public PollVote Get(string id) {
            var model = new PollVote();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<PollVote>(id);
            }

            return model;
        }

        public List<PollVote> GetAll() {
            var models = new List<PollVote>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<PollVote>().ToList();
            }

            return models;
        }

        public void Add(PollVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(PollVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(PollVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
