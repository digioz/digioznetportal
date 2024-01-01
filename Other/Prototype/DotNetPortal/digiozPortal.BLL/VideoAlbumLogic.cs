using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class VideoAlbumLogic
    {
        public VideoAlbum Get(int id) {
            var VideoAlbumRepo = new VideoAlbumRepo();
            VideoAlbum VideoAlbum = VideoAlbumRepo.Get(id);

            return VideoAlbum;
        }

        public List<VideoAlbum> GetAll() {
            var VideoAlbumRepo = new VideoAlbumRepo();
            var VideoAlbums = VideoAlbumRepo.GetAll();

            return VideoAlbums;
        }

        public void Add(VideoAlbum VideoAlbum) {
            var VideoAlbumRepo = new VideoAlbumRepo();
            VideoAlbumRepo.Add(VideoAlbum);
        }

        public void Edit(VideoAlbum VideoAlbum) {
            var VideoAlbumRepo = new VideoAlbumRepo();

            VideoAlbumRepo.Edit(VideoAlbum);
        }

        public void Delete(int id) {
            var VideoAlbumRepo = new VideoAlbumRepo();
            var VideoAlbum = VideoAlbumRepo.Get(id); // Db.VideoAlbums.Find(id);

            if (VideoAlbum != null) {
                VideoAlbumRepo.Delete(VideoAlbum);
            }
        }
    }

}
