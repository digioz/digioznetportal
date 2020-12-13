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
    public class VideoRepo
    {
        private string _connectionString;

        public VideoRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Video Get(int id) {
            var model = new Video();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Video>(id);
            }

            return model;
        }

        public List<Video> GetAll() {
            var models = new List<Video>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Video>().ToList();
            }

            return models;
        }

        public void Add(Video model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Video model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Video model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
