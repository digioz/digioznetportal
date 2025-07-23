using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PluginService : IPluginService
    {
        private readonly digiozPortalContext _context;

        public PluginService(digiozPortalContext context)
        {
            _context = context;
        }

        public Plugin Get(int id)
        {
            return _context.Plugins.Find(id);
        }

        public List<Plugin> GetAll()
        {
            return _context.Plugins.ToList();
        }

        public void Add(Plugin plugin)
        {
            _context.Plugins.Add(plugin);
            _context.SaveChanges();
        }

        public void Update(Plugin plugin)
        {
            _context.Plugins.Update(plugin);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var plugin = _context.Plugins.Find(id);
            if (plugin != null)
            {
                _context.Plugins.Remove(plugin);
                _context.SaveChanges();
            }
        }
    }
}
