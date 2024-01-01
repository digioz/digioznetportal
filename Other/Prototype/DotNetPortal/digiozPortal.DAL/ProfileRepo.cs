using System;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using Dapper.Contrib.Extensions;
using digiozPortal.Utilities;
using digiozPortal.BO;
using Dapper;

namespace digiozPortal.DAL
{
    public class ProfileRepo
    {
        private string _connectionString;

        public ProfileRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Profile Get(int id) {
            var model = new Profile();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Profile>(id);
            }

            return model;
        }

        public Profile GetProfileByEmail(string email) {
            var model = new Profile();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Query<Profile>("SELECT * FROM Profile WHERE Email=@Email", new { Email = email }).FirstOrDefault();
            }

            return model;
        }

        public Profile GetProfileByUserId(string id) {
            var model = new Profile();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Query<Profile>("SELECT * FROM Profile WHERE UserID=@UserID", new { UserID = id }).FirstOrDefault();
            }

            return model;
        }

        public List<Profile> GetAll() {
            var models = new List<Profile>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Profile>().ToList();
            }

            return models;
        }

        public void Add(Profile model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Profile model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Profile model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
