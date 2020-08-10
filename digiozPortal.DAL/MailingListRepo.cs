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
    public class MailingListRepo
    {
        private string _connectionString;

        public MailingListRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public MailingList Get(string id) {
            var model = new MailingList();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<MailingList>(id);
            }

            return model;
        }

        public List<MailingList> GetAll() {
            var models = new List<MailingList>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<MailingList>().ToList();
            }

            return models;
        }

        public void Add(MailingList model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(MailingList model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(MailingList model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
