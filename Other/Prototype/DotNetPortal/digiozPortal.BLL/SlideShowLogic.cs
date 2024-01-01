using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class SlideShowLogic
    {
        public SlideShow Get(int id) {
            var SlideShowRepo = new SlideShowRepo();
            SlideShow SlideShow = SlideShowRepo.Get(id);

            return SlideShow;
        }

        public List<SlideShow> GetAll() {
            var SlideShowRepo = new SlideShowRepo();
            var SlideShows = SlideShowRepo.GetAll();

            return SlideShows;
        }

        public void Add(SlideShow SlideShow) {
            var SlideShowRepo = new SlideShowRepo();
            SlideShowRepo.Add(SlideShow);
        }

        public void Edit(SlideShow SlideShow) {
            var SlideShowRepo = new SlideShowRepo();

            SlideShowRepo.Edit(SlideShow);
        }

        public void Delete(int id) {
            var SlideShowRepo = new SlideShowRepo();
            var SlideShow = SlideShowRepo.Get(id); // Db.SlideShows.Find(id);

            if (SlideShow != null) {
                SlideShowRepo.Delete(SlideShow);
            }
        }
    }

}
