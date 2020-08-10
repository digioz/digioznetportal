using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class VisitorInfoLogic
    {
        public VisitorInfo Get(int id) {
            var VisitorInfoRepo = new VisitorInfoRepo();
            VisitorInfo VisitorInfo = VisitorInfoRepo.Get(id);

            return VisitorInfo;
        }

        public List<VisitorInfo> GetAll() {
            var VisitorInfoRepo = new VisitorInfoRepo();
            var VisitorInfos = VisitorInfoRepo.GetAll();

            return VisitorInfos;
        }

        public void Add(VisitorInfo VisitorInfo) {
            var VisitorInfoRepo = new VisitorInfoRepo();
            VisitorInfoRepo.Add(VisitorInfo);
        }

        public void Edit(VisitorInfo VisitorInfo) {
            var VisitorInfoRepo = new VisitorInfoRepo();

            VisitorInfoRepo.Edit(VisitorInfo);
        }

        public void Delete(int id) {
            var VisitorInfoRepo = new VisitorInfoRepo();
            var VisitorInfo = VisitorInfoRepo.Get(id); // Db.VisitorInfos.Find(id);

            if (VisitorInfo != null) {
                VisitorInfoRepo.Delete(VisitorInfo);
            }
        }
    }

}
