using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ZoneService : IZoneService
    {
        private readonly digiozPortalContext _context;

        public ZoneService(digiozPortalContext context)
        {
            _context = context;
        }

        public Zone Get(int id)
        {
            return _context.Zones.Find(id);
        }

        public List<Zone> GetAll()
        {
            return _context.Zones.ToList();
        }

        public void Add(Zone zone)
        {
            _context.Zones.Add(zone);
            _context.SaveChanges();
        }

        public void Update(Zone zone)
        {
            _context.Zones.Update(zone);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var zone = _context.Zones.Find(id);
            if (zone != null)
            {
                _context.Zones.Remove(zone);
                _context.SaveChanges();
            }
        }
    }
}
