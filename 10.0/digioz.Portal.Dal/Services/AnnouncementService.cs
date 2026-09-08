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

        public List<Announcement> GetVisible(int count)
        {
            return _context.Announcements
                .Where(a => a.Visible)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<Announcement> GetPagedVisible(int pageNumber, int pageSize, out int totalCount)
        {
            var query = _context.Announcements.Where(a => a.Visible);
            
            totalCount = query.Count();
            
            return query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
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

        public List<Announcement> Search(string term, int skip, int take, out int totalCount)
        {
            term = term ?? string.Empty;
            var q = _context.Announcements.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(a => a.Visible && (
                    (a.Title != null && a.Title.ToLower().Contains(t)) ||
                    (a.Body != null && a.Body.ToLower().Contains(t))
                ));
            }
            else
            {
                q = q.Where(a => a.Visible);
            }

            totalCount = q.Count();
            return q
                .OrderByDescending(a => a.Timestamp ?? System.DateTime.MinValue)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
