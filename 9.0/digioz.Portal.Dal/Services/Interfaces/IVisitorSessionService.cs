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
    }
}
