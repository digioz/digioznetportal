using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PageService : IPageService
    {
        private readonly digiozPortalContext _context;

        public PageService(digiozPortalContext context)
        {
            _context = context;
        }

        public Page Get(int id)
        {
            return _context.Pages.Find(id);
        }

        public List<Page> GetAll()
        {
            return _context.Pages.ToList();
        }

        public void Add(Page page)
        {
            _context.Pages.Add(page);
            _context.SaveChanges();
        }

        public void Update(Page page)
        {
            _context.Pages.Update(page);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var page = _context.Pages.Find(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
                _context.SaveChanges();
            }
        }

        public Page GetByTitle(string title)
        {
            return _context.Pages.FirstOrDefault(p => p.Title == title);
        }
    }
}
