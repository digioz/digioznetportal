using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class LogLogic
    {
        public Log Get(int id) {
            var LogRepo = new LogRepo();
            Log Log = LogRepo.Get(id);

            return Log;
        }

        public List<Log> GetAll() {
            var LogRepo = new LogRepo();
            var Logs = LogRepo.GetAll();

            return Logs;
        }

        public void Add(Log Log) {
            var LogRepo = new LogRepo();
            LogRepo.Add(Log);
        }

        public void Edit(Log Log) {
            var LogRepo = new LogRepo();

            LogRepo.Edit(Log);
        }

        public void Delete(int id) {
            var LogRepo = new LogRepo();
            var Log = LogRepo.Get(id); // Db.Logs.Find(id);

            if (Log != null) {
                LogRepo.Delete(Log);
            }
        }
    }

}
