using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class RssService : IRssService
    {
        private readonly digiozPortalContext _context;

        public RssService(digiozPortalContext context)
        {
            _context = context;
        }

        public Rss Get(int id)
        {
            return _context.Rsses.Find(id);
        }

        public List<Rss> GetAll()
        {
            return _context.Rsses.ToList();
        }

        public void Add(Rss rss)
        {
            _context.Rsses.Add(rss);
            _context.SaveChanges();
        }

        public void Update(Rss rss)
        {
            _context.Rsses.Update(rss);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var rss = _context.Rsses.Find(id);
            if (rss != null)
            {
                _context.Rsses.Remove(rss);
                _context.SaveChanges();
            }
        }
    }
}
