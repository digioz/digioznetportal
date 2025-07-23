using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class LogService : ILogService
    {
        private readonly digiozPortalContext _context;

        public LogService(digiozPortalContext context)
        {
            _context = context;
        }

        public Log Get(int id)
        {
            return _context.Logs.Find(id);
        }

        public List<Log> GetAll()
        {
            return _context.Logs.ToList();
        }

        public void Add(Log log)
        {
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void Update(Log log)
        {
            _context.Logs.Update(log);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var log = _context.Logs.Find(id);
            if (log != null)
            {
                _context.Logs.Remove(log);
                _context.SaveChanges();
            }
        }
    }
}
