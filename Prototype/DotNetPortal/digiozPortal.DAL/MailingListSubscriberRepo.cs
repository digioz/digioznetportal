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
    public class MailingListSubscriberRepo
    {
        private string _connectionString;

        public MailingListSubscriberRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public MailingListSubscriber Get(string id) {
            var model = new MailingListSubscriber();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<MailingListSubscriber>(id);
            }

            return model;
        }

        public List<MailingListSubscriber> GetAll() {
            var models = new List<MailingListSubscriber>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<MailingListSubscriber>().ToList();
            }

            return models;
        }

        public void Add(MailingListSubscriber model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(MailingListSubscriber model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(MailingListSubscriber model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
