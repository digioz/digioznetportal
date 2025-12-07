using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var existingEntity = _context.Links.Local.FirstOrDefault(e => e.Id == link.Id);
            
            if (existingEntity != null)
            {
                // Entity is already tracked, update its properties
                _context.Entry(existingEntity).CurrentValues.SetValues(link);
            }
            else
            {
                // Entity is not tracked, attach and mark as modified
                _context.Links.Attach(link);
                _context.Entry(link).State = EntityState.Modified;
            }
            
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

        public List<Link> Search(string term, int skip, int take, out int totalCount)
        {
            term = term ?? string.Empty;
            var q = _context.Links.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(l => l.Visible && (
                    (l.Name != null && l.Name.ToLower().Contains(t)) ||
                    (l.Url != null && l.Url.ToLower().Contains(t)) ||
                    (l.Description != null && l.Description.ToLower().Contains(t))
                ));
            }
            else
            {
                q = q.Where(l => l.Visible);
            }

            totalCount = q.Count();
            return q
                .OrderByDescending(l => l.Timestamp ?? System.DateTime.MinValue)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
