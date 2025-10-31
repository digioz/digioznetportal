using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVisitorInfoService
    {
        VisitorInfo Get(int id);
        List<VisitorInfo> GetAll();
        List<VisitorInfo> GetLastN(int n, string sortOrder);
        void Add(VisitorInfo info);
        void AddRange(IEnumerable<VisitorInfo> infos);
        void Update(VisitorInfo info);
        void Delete(int id);
        // New: pagination and search helpers for Admin/Visitor
        List<VisitorInfo> GetPaged(int page, int pageSize);
        List<VisitorInfo> SearchPaged(string term, int page, int pageSize);
        int CountAll();
        int CountSearch(string term);
    }
}
