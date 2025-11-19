using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class VisitorSessionService : IVisitorSessionService
    {
        private readonly digiozPortalContext _context;

        public VisitorSessionService(digiozPortalContext context)
        {
            _context = context;
        }

        public VisitorSession Get(int id)
        {
            return _context.VisitorSessions.Include(x => x.Profile).FirstOrDefault(x => x.Id == id);
        }

        public List<VisitorSession> GetAll()
        {
            return _context.VisitorSessions.Include(x => x.Profile).ToList();
        }

        public List<VisitorSession> GetAllGreaterThan(DateTime dateTime)
        {
            return _context.VisitorSessions
                .Include(x => x.Profile)
                .Where(x => x.DateModified >= dateTime)
                .ToList();
        }

        public void Add(VisitorSession session)
        {
            _context.VisitorSessions.Add(session);
            _context.SaveChanges();
        }

        public void Update(VisitorSession session)
        {
            _context.VisitorSessions.Update(session);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var session = _context.VisitorSessions.Find(id);
            if (session != null)
            {
                _context.VisitorSessions.Remove(session);
                _context.SaveChanges();
            }
        }
    }
}
