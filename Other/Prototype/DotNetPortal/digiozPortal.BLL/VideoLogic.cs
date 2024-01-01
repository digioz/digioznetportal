using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class VideoLogic
    {
        public Video Get(int id) {
            var VideoRepo = new VideoRepo();
            Video Video = VideoRepo.Get(id);

            return Video;
        }

        public List<Video> GetAll() {
            var VideoRepo = new VideoRepo();
            var Videos = VideoRepo.GetAll();

            return Videos;
        }

        public void Add(Video Video) {
            var VideoRepo = new VideoRepo();
            VideoRepo.Add(Video);
        }

        public void Edit(Video Video) {
            var VideoRepo = new VideoRepo();

            VideoRepo.Edit(Video);
        }

        public void Delete(int id) {
            var VideoRepo = new VideoRepo();
            var Video = VideoRepo.Get(id); // Db.Videos.Find(id);

            if (Video != null) {
                VideoRepo.Delete(Video);
            }
        }
    }

}
