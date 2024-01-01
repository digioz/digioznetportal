using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PictureAlbumLogic
    {
        public PictureAlbum Get(int id) {
            var PictureAlbumRepo = new PictureAlbumRepo();
            PictureAlbum PictureAlbum = PictureAlbumRepo.Get(id);

            return PictureAlbum;
        }

        public List<PictureAlbum> GetAll() {
            var PictureAlbumRepo = new PictureAlbumRepo();
            var PictureAlbums = PictureAlbumRepo.GetAll();

            return PictureAlbums;
        }

        public void Add(PictureAlbum PictureAlbum) {
            var PictureAlbumRepo = new PictureAlbumRepo();
            PictureAlbumRepo.Add(PictureAlbum);
        }

        public void Edit(PictureAlbum PictureAlbum) {
            var PictureAlbumRepo = new PictureAlbumRepo();

            PictureAlbumRepo.Edit(PictureAlbum);
        }

        public void Delete(int id) {
            var PictureAlbumRepo = new PictureAlbumRepo();
            var PictureAlbum = PictureAlbumRepo.Get(id); // Db.PictureAlbums.Find(id);

            if (PictureAlbum != null) {
                PictureAlbumRepo.Delete(PictureAlbum);
            }
        }
    }

}
