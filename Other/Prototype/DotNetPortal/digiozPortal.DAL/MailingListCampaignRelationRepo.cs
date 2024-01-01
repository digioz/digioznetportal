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
    public class MailingListCampaignRelationRepo
    {
        private string _connectionString;

        public MailingListCampaignRelationRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public MailingListCampaignRelation Get(string id) {
            var model = new MailingListCampaignRelation();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<MailingListCampaignRelation>(id);
            }

            return model;
        }

        public List<MailingListCampaignRelation> GetAll() {
            var models = new List<MailingListCampaignRelation>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<MailingListCampaignRelation>().ToList();
            }

            return models;
        }

        public void Add(MailingListCampaignRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(MailingListCampaignRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(MailingListCampaignRelation model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
