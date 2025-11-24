using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Services
{
    public class ThemeService : IThemeService
    {
        private readonly digiozPortalContext _context;

        public ThemeService(digiozPortalContext context)
        {
            _context = context;
        }

        public Theme Get(int id)
        {
            return _context.Themes.Find(id);
        }

        public Theme GetDefault()
        {
            return _context.Themes.FirstOrDefault(t => t.IsDefault);
        }

        public List<Theme> GetAll()
        {
            return _context.Themes.ToList();
        }

        public void Add(Theme theme)
        {
            _context.Themes.Add(theme);
            _context.SaveChanges();
        }

        public void Update(Theme theme)
        {
            _context.Themes.Update(theme);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var theme = _context.Themes.Find(id);
            if (theme != null)
            {
                _context.Themes.Remove(theme);
                _context.SaveChanges();
            }
        }
    }
}
