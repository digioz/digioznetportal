using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class LinkService : ILinkService
    {
        private readonly digiozPortalContext _context;

        public LinkService(digiozPortalContext context)
        {
            _context = context;
        }

        public Link Get(int id)
        {
            return _context.Links.Find(id);
        }

        public List<Link> GetAll()
        {
            return _context.Links.ToList();
        }

        public void Add(Link link)
        {
            _context.Links.Add(link);
            _context.SaveChanges();
        }

        public void Update(Link link)
        {
            _context.Links.Update(link);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var link = _context.Links.Find(id);
            if (link != null)
            {
                _context.Links.Remove(link);
                _context.SaveChanges();
            }
        }
    }
}
