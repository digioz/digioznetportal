using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MenuLogic
    {
        public Menu Get(int id) {
            var MenuRepo = new MenuRepo();
            Menu Menu = MenuRepo.Get(id);

            return Menu;
        }

        public List<Menu> GetAll() {
            var MenuRepo = new MenuRepo();
            var Menus = MenuRepo.GetAll();

            return Menus;
        }

        public void Add(Menu Menu) {
            var MenuRepo = new MenuRepo();
            MenuRepo.Add(Menu);
        }

        public void Edit(Menu Menu) {
            var MenuRepo = new MenuRepo();

            MenuRepo.Edit(Menu);
        }

        public void Delete(int id) {
            var MenuRepo = new MenuRepo();
            var Menu = MenuRepo.Get(id); // Db.Menus.Find(id);

            if (Menu != null) {
                MenuRepo.Delete(Menu);
            }
        }
    }

}
