using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class LogVisitorLogic
    {
        public LogVisitor Get(int id) {
            var LogVisitorRepo = new LogVisitorRepo();
            LogVisitor LogVisitor = LogVisitorRepo.Get(id);

            return LogVisitor;
        }

        public List<LogVisitor> GetAll() {
            var LogVisitorRepo = new LogVisitorRepo();
            var LogVisitors = LogVisitorRepo.GetAll();

            return LogVisitors;
        }

        public void Add(LogVisitor LogVisitor) {
            var LogVisitorRepo = new LogVisitorRepo();
            LogVisitorRepo.Add(LogVisitor);
        }

        public void Edit(LogVisitor LogVisitor) {
            var LogVisitorRepo = new LogVisitorRepo();

            LogVisitorRepo.Edit(LogVisitor);
        }

        public void Delete(int id) {
            var LogVisitorRepo = new LogVisitorRepo();
            var LogVisitor = LogVisitorRepo.Get(id); // Db.LogVisitors.Find(id);

            if (LogVisitor != null) {
                LogVisitorRepo.Delete(LogVisitor);
            }
        }
    }

}
