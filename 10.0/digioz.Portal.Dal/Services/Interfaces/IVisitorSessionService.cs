using System;
using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVisitorSessionService
    {
        VisitorSession Get(int id);
        List<VisitorSession> GetAll();
        List<VisitorSession> GetAllGreaterThan(DateTime dateTime);
        void Add(VisitorSession session);
        void Update(VisitorSession session);
        void Delete(int id);
        List<VisitorSession> GetPaged(int page, int pageSize);
        List<VisitorSession> SearchPaged(string term, int page, int pageSize);
        int CountAll();
        int CountSearch(string term);
        List<VisitorSession> GetByDateRange(DateTime? start, DateTime? end);
        int DeleteRange(IEnumerable<int> ids);
    }
}
