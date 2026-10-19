using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ConfigService : IConfigService
    {
        private readonly digiozPortalContext _context;

        public ConfigService(digiozPortalContext context)
        {
            _context = context;
        }

        public Config Get(string id)
        {
            return _context.Configs.Find(id);
        }

        public Config GetByKey(string configKey)
        {
            return _context.Configs.FirstOrDefault(c => c.ConfigKey == configKey);
        }

        public List<Config> GetAll()
        {
            return _context.Configs.ToList();
        }

        public void Add(Config config)
        {
            _context.Configs.Add(config);
            _context.SaveChanges();
        }

        public void Update(Config config)
        {
            _context.Configs.Update(config);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var config = _context.Configs.Find(id);
            if (config != null)
            {
                _context.Configs.Remove(config);
                _context.SaveChanges();
            }
        }
    }
}
