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
    public class MailingListCampaignRepo
    {
        private string _connectionString;

        public MailingListCampaignRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public MailingListCampaign Get(string id) {
            var model = new MailingListCampaign();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<MailingListCampaign>(id);
            }

            return model;
        }

        public List<MailingListCampaign> GetAll() {
            var models = new List<MailingListCampaign>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<MailingListCampaign>().ToList();
            }

            return models;
        }

        public void Add(MailingListCampaign model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(MailingListCampaign model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(MailingListCampaign model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
