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
    public class SlideShowRepo
    {
        private string _connectionString;

        public SlideShowRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public SlideShow Get(int id) {
            var model = new SlideShow();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<SlideShow>(id);
            }

            return model;
        }

        public List<SlideShow> GetAll() {
            var models = new List<SlideShow>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<SlideShow>().ToList();
            }

            return models;
        }

        public void Add(SlideShow model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(SlideShow model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(SlideShow model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
