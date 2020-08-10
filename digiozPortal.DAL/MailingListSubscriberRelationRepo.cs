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
    public class MailingListSubscriberRelationRepo
    {
        private string _connectionString;

        public MailingListSubscriberRelationRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public MailingListSubscriberRelation Get(string id) {
            var model = new MailingListSubscriberRelation();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<MailingListSubscriberRelation>(id);
            }

            return model;
        }

        public List<MailingListSubscriberRelation> GetAll() {
            var models = new List<MailingListSubscriberRelation>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<MailingListSubscriberRelation>().ToList();
            }

            return models;
        }

        public void Add(MailingListSubscriberRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(MailingListSubscriberRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(MailingListSubscriberRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
