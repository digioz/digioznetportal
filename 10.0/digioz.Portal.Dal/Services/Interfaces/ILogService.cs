using System;
using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ILogService
    {
        Log Get(int id);
        List<Log> GetAll();
        List<Log> GetLastN(int n, string order);
        void Add(Log log);
        void AddRange(IEnumerable<Log> logs);
        void Update(Log log);
        void Delete(int id);
        // Existing helpers
        List<Log> GetLatest(int count);
        List<Log> Search(string term, int maxCount);
        // Pagination helpers
        List<Log> GetPaged(int page, int pageSize);
        List<Log> SearchPaged(string term, int page, int pageSize);
        // Counts for pagination without loading all rows
        int CountAll();
        int CountSearch(string term);

        // New: filtered retrieval for bulk export/purge operations
        List<Log> GetByDateRange(DateTime? start, DateTime? end);
        
        // New: bulk delete for performance
        int DeleteRange(IEnumerable<int> ids);
    }
}
