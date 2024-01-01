using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ShoppingCartLogic
    {
        public ShoppingCart Get(string id) {
            var ShoppingCartRepo = new ShoppingCartRepo();
            ShoppingCart ShoppingCart = ShoppingCartRepo.Get(id);

            return ShoppingCart;
        }

        public List<ShoppingCart> GetAll() {
            var ShoppingCartRepo = new ShoppingCartRepo();
            var ShoppingCarts = ShoppingCartRepo.GetAll();

            return ShoppingCarts;
        }

        public void Add(ShoppingCart ShoppingCart) {
            var ShoppingCartRepo = new ShoppingCartRepo();
            ShoppingCartRepo.Add(ShoppingCart);
        }

        public void Edit(ShoppingCart ShoppingCart) {
            var ShoppingCartRepo = new ShoppingCartRepo();

            ShoppingCartRepo.Edit(ShoppingCart);
        }

        public void Delete(string id) {
            var ShoppingCartRepo = new ShoppingCartRepo();
            var ShoppingCart = ShoppingCartRepo.Get(id); // Db.ShoppingCarts.Find(id);

            if (ShoppingCart != null) {
                ShoppingCartRepo.Delete(ShoppingCart);
            }
        }
    }

}
