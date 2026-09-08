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

        public Page GetByUrl(string url)
        {
            return _context.Pages.FirstOrDefault(p => p.Url == url);
        }

        public List<Page> Search(string term, int skip, int take, out int totalCount)
        {
            term = term ?? string.Empty;
            var q = _context.Pages.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(p => p.Visible && (
                    (p.Title != null && p.Title.ToLower().Contains(t)) ||
                    (p.Body != null && p.Body.ToLower().Contains(t))
                ));
            }
            else
            {
                q = q.Where(p => p.Visible);
            }

            totalCount = q.Count();
            return q
                .OrderByDescending(p => p.Timestamp ?? System.DateTime.MinValue)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
