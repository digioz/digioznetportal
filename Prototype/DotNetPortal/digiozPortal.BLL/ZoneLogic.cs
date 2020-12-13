using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ZoneLogic
    {
        public Zone Get(int id) {
            var ZoneRepo = new ZoneRepo();
            Zone Zone = ZoneRepo.Get(id);

            return Zone;
        }

        public List<Zone> GetAll() {
            var ZoneRepo = new ZoneRepo();
            var Zones = ZoneRepo.GetAll();

            return Zones;
        }

        public void Add(Zone Zone) {
            var ZoneRepo = new ZoneRepo();
            ZoneRepo.Add(Zone);
        }

        public void Edit(Zone Zone) {
            var ZoneRepo = new ZoneRepo();

            ZoneRepo.Edit(Zone);
        }

        public void Delete(int id) {
            var ZoneRepo = new ZoneRepo();
            var Zone = ZoneRepo.Get(id); // Db.Zones.Find(id);

            if (Zone != null) {
                ZoneRepo.Delete(Zone);
            }
        }
    }

}
