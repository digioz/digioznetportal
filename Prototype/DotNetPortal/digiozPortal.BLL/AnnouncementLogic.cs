using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class AnnouncementLogic
    {
        public Announcement Get(int id) {
            var AnnouncementRepo = new AnnouncementRepo();
            Announcement Announcement = AnnouncementRepo.Get(id);

            return Announcement;
        }

        public List<Announcement> GetAll() {
            var AnnouncementRepo = new AnnouncementRepo();
            var Announcements = AnnouncementRepo.GetAll();

            return Announcements;
        }

        public void Add(Announcement Announcement) {
            var AnnouncementRepo = new AnnouncementRepo();
            AnnouncementRepo.Add(Announcement);
        }

        public void Edit(Announcement Announcement) {
            var AnnouncementRepo = new AnnouncementRepo();

            AnnouncementRepo.Edit(Announcement);
        }

        public void Delete(int id) {
            var AnnouncementRepo = new AnnouncementRepo();
            var Announcement = AnnouncementRepo.Get(id); // Db.Announcements.Find(id);

            if (Announcement != null) {
                AnnouncementRepo.Delete(Announcement);
            }
        }
    }
}
