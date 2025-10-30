using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Services
{
    public class LogService : ILogService
    {
        private readonly digiozPortalContext _context;

        public LogService(digiozPortalContext context)
        {
            _context = context;
        }

        public Log Get(int id) => _context.Logs.Find(id);

        public List<Log> GetAll() => _context.Logs.ToList();

        public List<Log> GetLastN(int n, string order)
        {
            var query = _context.Logs.AsQueryable();
            if (order.ToLower() == "asc")
            {
                query = query.OrderBy(l => l.Id);
            }
            else
            {
                query = query.OrderByDescending(l => l.Id);
            }
            return query.Take(n).ToList();
        }

        public int CountAll() => _context.Logs.Count();

        public int CountSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return CountAll();
            var like = $"%{term.Trim()}%";
            return _context.Logs
                .Where(l => EF.Functions.Like(l.Message, like)
                         || EF.Functions.Like(l.Level, like)
                         || EF.Functions.Like(l.LogEvent, like))
                .Count();
        }

        public List<Log> GetLatest(int count) => _context.Logs
            .OrderByDescending(l => l.Timestamp ?? System.DateTime.MinValue)
            .ThenByDescending(l => l.Id)
            .Take(count)
            .AsNoTracking()
            .ToList();

        public List<Log> Search(string term, int maxCount)
        {
            if (string.IsNullOrWhiteSpace(term)) return GetLatest(maxCount);
            var like = $"%{term.Trim()}%";
            return _context.Logs
                .Where(l => EF.Functions.Like(l.Message, like)
                         || EF.Functions.Like(l.Level, like)
                         || EF.Functions.Like(l.LogEvent, like))
                .OrderByDescending(l => l.Timestamp ?? System.DateTime.MinValue)
                .ThenByDescending(l => l.Id)
                .Take(maxCount)
                .AsNoTracking()
                .ToList();
        }

        public List<Log> GetPaged(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            return _context.Logs
                .OrderByDescending(l => l.Timestamp ?? System.DateTime.MinValue)
                .ThenByDescending(l => l.Id)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();
        }

        public List<Log> SearchPaged(string term, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            if (string.IsNullOrWhiteSpace(term)) return GetPaged(page, pageSize);
            var like = $"%{term.Trim()}%";
            return _context.Logs
                .Where(l => EF.Functions.Like(l.Message, like)
                         || EF.Functions.Like(l.Level, like)
                         || EF.Functions.Like(l.LogEvent, like))
                .OrderByDescending(l => l.Timestamp ?? System.DateTime.MinValue)
                .ThenByDescending(l => l.Id)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();
        }

        public void Add(Log log)
        {
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<Log> logs)
        {
            var list = logs is IList<Log> l ? l : logs.ToList();
            if (list.Count == 0) return;
            _context.Logs.AddRange(list);
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
