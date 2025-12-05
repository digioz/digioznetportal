using System;
using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVisitorInfoService
    {
        VisitorInfo Get(long id);
        List<VisitorInfo> GetAll();
        List<VisitorInfo> GetAllGreaterThan(DateTime timestamp);
        List<VisitorInfo> GetLastN(int n, string sortOrder);
        void Add(VisitorInfo info);
        void AddRange(IEnumerable<VisitorInfo> infos);
        void Update(VisitorInfo info);
        void Delete(long id);
        // New: pagination and search helpers for Admin/Visitor
        List<VisitorInfo> GetPaged(int page, int pageSize);
        List<VisitorInfo> SearchPaged(string term, int page, int pageSize);
        int CountAll();
        int CountSearch(string term);
        Dictionary<DateTime, int> GetUniqueVisitorCountsByDate(DateTime startDate, DateTime endDate);

        // New: filtered retrieval for bulk export/purge operations
        List<VisitorInfo> GetByDateRange(DateTime? start, DateTime? end);
    }
}
