using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class LinkCategoryService : ILinkCategoryService
    {
        private readonly digiozPortalContext _context;

        public LinkCategoryService(digiozPortalContext context)
        {
            _context = context;
        }

        public LinkCategory Get(int id)
        {
            return _context.LinkCategories.Find(id);
        }

        public List<LinkCategory> GetAll()
        {
            return _context.LinkCategories.ToList();
        }

        public void Add(LinkCategory category)
        {
            _context.LinkCategories.Add(category);
            _context.SaveChanges();
        }

        public void Update(LinkCategory category)
        {
            _context.LinkCategories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var category = _context.LinkCategories.Find(id);
            if (category != null)
            {
                _context.LinkCategories.Remove(category);
                _context.SaveChanges();
            }
        }
    }
}
