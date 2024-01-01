using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class VisitorSessionLogic
    {
        public VisitorSession Get(int id) {
            var VisitorSessionRepo = new VisitorSessionRepo();
            VisitorSession VisitorSession = VisitorSessionRepo.Get(id);

            return VisitorSession;
        }

        public List<VisitorSession> GetAll() {
            var VisitorSessionRepo = new VisitorSessionRepo();
            var VisitorSessions = VisitorSessionRepo.GetAll();

            return VisitorSessions;
        }

        public void Add(VisitorSession VisitorSession) {
            var VisitorSessionRepo = new VisitorSessionRepo();
            VisitorSessionRepo.Add(VisitorSession);
        }

        public void Edit(VisitorSession VisitorSession) {
            var VisitorSessionRepo = new VisitorSessionRepo();

            VisitorSessionRepo.Edit(VisitorSession);
        }

        public void Delete(int id) {
            var VisitorSessionRepo = new VisitorSessionRepo();
            var VisitorSession = VisitorSessionRepo.Get(id); // Db.VisitorSessions.Find(id);

            if (VisitorSession != null) {
                VisitorSessionRepo.Delete(VisitorSession);
            }
        }
    }

}
