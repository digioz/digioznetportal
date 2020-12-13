using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PageLogic
    {
        public Page Get(int id) {
            var PageRepo = new PageRepo();
            Page Page = PageRepo.Get(id);

            return Page;
        }

        public List<Page> GetAll() {
            var PageRepo = new PageRepo();
            var Pages = PageRepo.GetAll();

            return Pages;
        }

        public void Add(Page Page) {
            var PageRepo = new PageRepo();
            PageRepo.Add(Page);
        }

        public void Edit(Page Page) {
            var PageRepo = new PageRepo();

            PageRepo.Edit(Page);
        }

        public void Delete(int id) {
            var PageRepo = new PageRepo();
            var Page = PageRepo.Get(id); // Db.Pages.Find(id);

            if (Page != null) {
                PageRepo.Delete(Page);
            }
        }
    }

}
