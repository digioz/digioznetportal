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
    public class PollUsersVoteRepo
    {
        private string _connectionString;

        public PollUsersVoteRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public PollUsersVote Get(int id) {
            var model = new PollUsersVote();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<PollUsersVote>(id);
            }

            return model;
        }

        public List<PollUsersVote> GetAll() {
            var models = new List<PollUsersVote>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<PollUsersVote>().ToList();
            }

            return models;
        }

        public void Add(PollUsersVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(PollUsersVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(PollUsersVote model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
