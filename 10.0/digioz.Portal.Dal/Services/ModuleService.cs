using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ModuleService : IModuleService
    {
        private readonly digiozPortalContext _context;

        public ModuleService(digiozPortalContext context)
        {
            _context = context;
        }

        public Module Get(int id)
        {
            return _context.Modules.Find(id);
        }

        public List<Module> GetAll()
        {
            return _context.Modules.ToList();
        }

        public void Add(Module module)
        {
            _context.Modules.Add(module);
            _context.SaveChanges();
        }

        public void Update(Module module)
        {
            _context.Modules.Update(module);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var module = _context.Modules.Find(id);
            if (module != null)
            {
                _context.Modules.Remove(module);
                _context.SaveChanges();
            }
        }
    }
}
