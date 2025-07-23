using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IOrderService
    {
        Order Get(string id);
        List<Order> GetAll();
        void Add(Order order);
        void Update(Order order);
        void Delete(string id);
    }
}
