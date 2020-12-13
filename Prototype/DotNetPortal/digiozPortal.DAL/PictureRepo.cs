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
    public class PictureRepo
    {
        private string _connectionString;

        public PictureRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public Picture Get(int id) {
            var model = new Picture();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<Picture>(id);
            }

            return model;
        }

        public List<Picture> GetAll() {
            var models = new List<Picture>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<Picture>().ToList();
            }

            return models;
        }

        public void Add(Picture model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(Picture model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(Picture model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
