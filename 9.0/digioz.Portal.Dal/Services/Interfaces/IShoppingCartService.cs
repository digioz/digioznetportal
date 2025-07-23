using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IShoppingCartService
    {
        ShoppingCart Get(string id);
        List<ShoppingCart> GetAll();
        void Add(ShoppingCart cart);
        void Update(ShoppingCart cart);
        void Delete(string id);
    }
}
