using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace digioz.Portal.Dal.Services
{
    public class MenuService : IMenuService
    {
        private readonly digiozPortalContext _context;

        public MenuService(digiozPortalContext context)
        {
            _context = context;
        }

        public Menu Get(int id)
        {
            return _context.Menus.Find(id);
        }

        public Menu GetNoTracking(int id)
        {
            return _context.Menus.AsNoTracking().FirstOrDefault(m => m.Id == id);
        }

        public List<Menu> GetAll()
        {
            return _context.Menus.ToList();
        }

        public void Add(Menu menu)
        {
            _context.Menus.Add(menu);
            _context.SaveChanges();
        }

        public void Update(Menu menu)
        {
            _context.Menus.Update(menu);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu != null)
            {
                _context.Menus.Remove(menu);
                _context.SaveChanges();
            }
        }
    }
}
