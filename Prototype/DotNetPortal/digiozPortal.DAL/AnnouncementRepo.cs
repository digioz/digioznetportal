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
    public class AnnouncementRepo
    {
        private string _connectionString;

        public AnnouncementRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Announcement Get(int id) {
            var model = new Announcement();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Announcement>(id);
            }

            return model;
        }

        public List<Announcement> GetAll() {
            var models = new List<Announcement>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Announcement>().ToList();
            }

            return models;
        }

        public void Add(Announcement model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Announcement model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Announcement model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
