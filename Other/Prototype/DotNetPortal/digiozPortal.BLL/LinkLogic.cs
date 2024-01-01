using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class LinkLogic
    {
        public Link Get(int id) {
            var LinkRepo = new LinkRepo();
            Link Link = LinkRepo.Get(id);

            return Link;
        }

        public List<Link> GetAll() {
            var LinkRepo = new LinkRepo();
            var Links = LinkRepo.GetAll();

            return Links;
        }

        public void Add(Link Link) {
            var LinkRepo = new LinkRepo();
            LinkRepo.Add(Link);
        }

        public void Edit(Link Link) {
            var LinkRepo = new LinkRepo();

            LinkRepo.Edit(Link);
        }

        public void Delete(int id) {
            var LinkRepo = new LinkRepo();
            var Link = LinkRepo.Get(id); // Db.Links.Find(id);

            if (Link != null) {
                LinkRepo.Delete(Link);
            }
        }
    }

}
