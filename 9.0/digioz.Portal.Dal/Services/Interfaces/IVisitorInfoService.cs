using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVisitorInfoService
    {
        VisitorInfo Get(int id);
        List<VisitorInfo> GetAll();
        void Add(VisitorInfo info);
        void AddRange(IEnumerable<VisitorInfo> infos);
        void Update(VisitorInfo info);
        void Delete(int id);
    }
}
