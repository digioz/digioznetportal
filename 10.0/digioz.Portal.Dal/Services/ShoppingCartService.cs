using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly digiozPortalContext _context;

        public ShoppingCartService(digiozPortalContext context)
        {
            _context = context;
        }

        public ShoppingCart Get(string id)
        {
            return _context.ShoppingCarts.Find(id);
        }

        public List<ShoppingCart> GetAll()
        {
            return _context.ShoppingCarts.ToList();
        }

        public void Add(ShoppingCart cart)
        {
            _context.ShoppingCarts.Add(cart);
            _context.SaveChanges();
        }

        public void Update(ShoppingCart cart)
        {
            _context.ShoppingCarts.Update(cart);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var cart = _context.ShoppingCarts.Find(id);
            if (cart != null)
            {
                _context.ShoppingCarts.Remove(cart);
                _context.SaveChanges();
            }
        }
    }
}
