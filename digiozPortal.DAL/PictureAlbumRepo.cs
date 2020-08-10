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
    public class PictureAlbumRepo
    {
        private string _connectionString;

        public PictureAlbumRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public PictureAlbum Get(int id) {
            var model = new PictureAlbum();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<PictureAlbum>(id);
            }

            return model;
        }

        public List<PictureAlbum> GetAll() {
            var models = new List<PictureAlbum>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<PictureAlbum>().ToList();
            }

            return models;
        }

        public void Add(PictureAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(PictureAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(PictureAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
