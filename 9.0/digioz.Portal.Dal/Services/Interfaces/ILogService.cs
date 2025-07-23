using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ILogService
    {
        Log Get(int id);
        List<Log> GetAll();
        void Add(Log log);
        void Update(Log log);
        void Delete(int id);
    }
}
