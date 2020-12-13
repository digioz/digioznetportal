using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class LinkCategoryLogic
    {
        public LinkCategory Get(int id) {
            var LinkCategoryRepo = new LinkCategoryRepo();
            LinkCategory LinkCategory = LinkCategoryRepo.Get(id);

            return LinkCategory;
        }

        public List<LinkCategory> GetAll() {
            var LinkCategoryRepo = new LinkCategoryRepo();
            var LinkCategorys = LinkCategoryRepo.GetAll();

            return LinkCategorys;
        }

        public void Add(LinkCategory LinkCategory) {
            var LinkCategoryRepo = new LinkCategoryRepo();
            LinkCategoryRepo.Add(LinkCategory);
        }

        public void Edit(LinkCategory LinkCategory) {
            var LinkCategoryRepo = new LinkCategoryRepo();

            LinkCategoryRepo.Edit(LinkCategory);
        }

        public void Delete(int id) {
            var LinkCategoryRepo = new LinkCategoryRepo();
            var LinkCategory = LinkCategoryRepo.Get(id); // Db.LinkCategorys.Find(id);

            if (LinkCategory != null) {
                LinkCategoryRepo.Delete(LinkCategory);
            }
        }
    }

}
