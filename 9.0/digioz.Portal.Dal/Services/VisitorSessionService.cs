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

        public int CountAll() => _context.VisitorSessions.Count();

        public int CountSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return CountAll();
            var like = $"%{term.Trim()}%";
            return _context.VisitorSessions
                .Where(v => EF.Functions.Like(v.SessionId, like)
                         || EF.Functions.Like(v.PageUrl, like)
                         || EF.Functions.Like(v.Username, like)
                         || EF.Functions.Like(v.IpAddress, like))
                .Count();
        }

        public List<VisitorSession> GetPaged(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            return _context.VisitorSessions
                .Include(x => x.Profile)
                .OrderByDescending(v => v.DateModified)
                .ThenByDescending(v => v.Id)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();
        }

        public List<VisitorSession> SearchPaged(string term, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            if (string.IsNullOrWhiteSpace(term)) return GetPaged(page, pageSize);
            var like = $"%{term.Trim()}%";
            return _context.VisitorSessions
                .Include(x => x.Profile)
                .Where(v => EF.Functions.Like(v.SessionId, like)
                         || EF.Functions.Like(v.PageUrl, like)
                         || EF.Functions.Like(v.Username, like)
                         || EF.Functions.Like(v.IpAddress, like))
                .OrderByDescending(v => v.DateModified)
                .ThenByDescending(v => v.Id)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
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

        public List<VisitorSession> GetByDateRange(DateTime? start, DateTime? end)
        {
            var query = _context.VisitorSessions.AsQueryable();

            // Only consider records that have a DateModified
            query = query.Where(v => v.DateModified != default);

            if (start.HasValue)
            {
                var s = start.Value.Date;
                query = query.Where(v => v.DateModified >= s);
            }

            if (end.HasValue)
            {
                var e = end.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(v => v.DateModified <= e);
            }

            return query.OrderBy(v => v.DateModified).ThenBy(v => v.Id).AsNoTracking().ToList();
        }
    }
}
