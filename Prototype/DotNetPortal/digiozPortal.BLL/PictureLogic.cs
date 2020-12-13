using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PictureLogic
    {
        public Picture Get(int id) {
            var PictureRepo = new PictureRepo();
            Picture Picture = PictureRepo.Get(id);

            return Picture;
        }

        public List<Picture> GetAll() {
            var PictureRepo = new PictureRepo();
            var Pictures = PictureRepo.GetAll();

            return Pictures;
        }

        public void Add(Picture Picture) {
            var PictureRepo = new PictureRepo();
            PictureRepo.Add(Picture);
        }

        public void Edit(Picture Picture) {
            var PictureRepo = new PictureRepo();

            PictureRepo.Edit(Picture);
        }

        public void Delete(int id) {
            var PictureRepo = new PictureRepo();
            var Picture = PictureRepo.Get(id); // Db.Pictures.Find(id);

            if (Picture != null) {
                PictureRepo.Delete(Picture);
            }
        }
    }

}
