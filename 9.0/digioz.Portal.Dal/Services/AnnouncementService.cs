using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly digiozPortalContext _context;

        public AnnouncementService(digiozPortalContext context)
        {
            _context = context;
        }

        public Announcement Get(int id)
        {
            return _context.Announcements.Find(id);
        }

        public List<Announcement> GetAll()
        {
            return _context.Announcements.ToList();
        }

        public void Add(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
            _context.SaveChanges();
        }

        public void Update(Announcement announcement)
        {
            _context.Announcements.Update(announcement);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var announcement = _context.Announcements.Find(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                _context.SaveChanges();
            }
        }
    }
}
