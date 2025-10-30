using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class VisitorInfoService : IVisitorInfoService
    {
        private readonly digiozPortalContext _context;

        public VisitorInfoService(digiozPortalContext context)
        {
            _context = context;
        }

        public VisitorInfo Get(int id)
        {
            return _context.VisitorInfos.Find(id);
        }

        public List<VisitorInfo> GetAll()
        {
            return _context.VisitorInfos.ToList();
        }

        public void Add(VisitorInfo info)
        {
            _context.VisitorInfos.Add(info);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<VisitorInfo> infos)
        {
            var list = infos is IList<VisitorInfo> l ? l : infos.ToList();
            if (list.Count == 0) return;
            _context.VisitorInfos.AddRange(list);
            _context.SaveChanges();
        }

        public void Update(VisitorInfo info)
        {
            _context.VisitorInfos.Update(info);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var info = _context.VisitorInfos.Find(id);
            if (info != null)
            {
                _context.VisitorInfos.Remove(info);
                _context.SaveChanges();
            }
        }
    }
}
