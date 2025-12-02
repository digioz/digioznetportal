using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public List<VisitorInfo> GetAllGreaterThan(DateTime timestamp)
        {
            return _context.VisitorInfos
                .Where(v => v.Timestamp.HasValue && v.Timestamp.Value >= timestamp)
                .AsNoTracking()
                .ToList();
        }

        public List<VisitorInfo> GetLastN(int n, string sortOrder)
        {
            var query = _context.VisitorInfos.AsQueryable();
            if (sortOrder.ToLower() == "asc")
            {
                query = query.OrderBy(v => v.Timestamp ?? System.DateTime.MinValue).ThenBy(v => v.Id);
            }
            else
            {
                query = query.OrderByDescending(v => v.Timestamp ?? System.DateTime.MinValue).ThenByDescending(v => v.Id);
            }
            return query.Take(n).AsNoTracking().ToList();
        }

        public int CountAll() => _context.VisitorInfos.Count();

        public int CountSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return CountAll();
            var like = $"%{term.Trim()}%";
            return _context.VisitorInfos
                .Where(v => EF.Functions.Like(v.UserAgent, like)
                       || EF.Functions.Like(v.Href, like)
              || EF.Functions.Like(v.Referrer, like)
             || EF.Functions.Like(v.IpAddress, like))
                  .Count();
        }

        public List<VisitorInfo> GetPaged(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            return _context.VisitorInfos
                          .OrderByDescending(v => v.Timestamp ?? System.DateTime.MinValue)
                    .ThenByDescending(v => v.Id)
                .Skip(skip)
           .Take(pageSize)
                .AsNoTracking()
                 .ToList();
        }

        public List<VisitorInfo> SearchPaged(string term, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;
            if (string.IsNullOrWhiteSpace(term)) return GetPaged(page, pageSize);
            var like = $"%{term.Trim()}%";
            return _context.VisitorInfos
              .Where(v => EF.Functions.Like(v.UserAgent, like)
                    || EF.Functions.Like(v.Href, like)
                   || EF.Functions.Like(v.Referrer, like)
                    || EF.Functions.Like(v.IpAddress, like))
                .OrderByDescending(v => v.Timestamp ?? System.DateTime.MinValue)
                     .ThenByDescending(v => v.Id)
                     .Skip(skip)
                 .Take(pageSize)
                        .AsNoTracking()
              .ToList();
        }

        public Dictionary<DateTime, int> GetUniqueVisitorCountsByDate(DateTime startDate, DateTime endDate)
        {
            // Ensure we're comparing dates only (no time component)
            var start = startDate.Date;
            var end = endDate.Date;

            // Query database to get unique session counts per date
            var result = _context.VisitorInfos
                .Where(v => v.Timestamp.HasValue &&
               v.Timestamp.Value.Date >= start &&
              v.Timestamp.Value.Date <= end &&
               !string.IsNullOrEmpty(v.SessionId))
              .GroupBy(v => v.Timestamp.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                UniqueVisitors = g.Select(v => v.SessionId).Distinct().Count()
            })
                .AsNoTracking()
                      .ToDictionary(x => x.Date, x => x.UniqueVisitors);

            return result;
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
