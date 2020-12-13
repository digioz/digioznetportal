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
    public class VideoAlbumRepo
    {
        private string _connectionString;

        public VideoAlbumRepo() {
            var config = new ConfigHelper();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public VideoAlbum Get(int id) {
            var model = new VideoAlbum();

            using (var connection = new SqlConnection(_connectionString)) {
                model = connection.Get<VideoAlbum>(id);
            }

            return model;
        }

        public List<VideoAlbum> GetAll() {
            var models = new List<VideoAlbum>();

            using (var connection = new SqlConnection(_connectionString)) {
                models = connection.GetAll<VideoAlbum>().ToList();
            }

            return models;
        }

        public void Add(VideoAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Insert(model);
            }
        }

        public void Edit(VideoAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Update(model);
            }
        }

        public void Delete(VideoAlbum model) {
            using (var connection = new SqlConnection(_connectionString)) {
                connection.Delete(model);
            }
        }
    }
}
