using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class CommentConfigLogic
    {
        public CommentConfig Get(string id) {
            var CommentConfigRepo = new CommentConfigRepo();
            CommentConfig CommentConfig = CommentConfigRepo.Get(id);

            return CommentConfig;
        }

        public List<CommentConfig> GetAll() {
            var CommentConfigRepo = new CommentConfigRepo();
            var CommentConfigs = CommentConfigRepo.GetAll();

            return CommentConfigs;
        }

        public void Add(CommentConfig CommentConfig) {
            var CommentConfigRepo = new CommentConfigRepo();
            CommentConfigRepo.Add(CommentConfig);
        }

        public void Edit(CommentConfig CommentConfig) {
            var CommentConfigRepo = new CommentConfigRepo();

            CommentConfigRepo.Edit(CommentConfig);
        }

        public void Delete(string id) {
            var CommentConfigRepo = new CommentConfigRepo();
            var CommentConfig = CommentConfigRepo.Get(id); // Db.CommentConfigs.Find(id);

            if (CommentConfig != null) {
                CommentConfigRepo.Delete(CommentConfig);
            }
        }
    }

}
