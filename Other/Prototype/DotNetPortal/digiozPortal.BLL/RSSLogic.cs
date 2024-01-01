using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class RssLogic
    {
        public Rss Get(int id) {
            var RssRepo = new RssRepo();
            Rss Rss = RssRepo.Get(id);

            return Rss;
        }

        public List<Rss> GetAll() {
            var RssRepo = new RssRepo();
            var Rsss = RssRepo.GetAll();

            return Rsss;
        }

        public void Add(Rss Rss) {
            var RssRepo = new RssRepo();
            RssRepo.Add(Rss);
        }

        public void Edit(Rss Rss) {
            var RssRepo = new RssRepo();

            RssRepo.Edit(Rss);
        }

        public void Delete(int id) {
            var RssRepo = new RssRepo();
            var Rss = RssRepo.Get(id); // Db.Rsss.Find(id);

            if (Rss != null) {
                RssRepo.Delete(Rss);
            }
        }
    }

}
