using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IOrderService
    {
        Order Get(string id);
        List<Order> GetAll();
        List<Order> GetByUserId(string userId);
        int CountByUserId(string userId);
        void Add(Order order);
        void Update(Order order);
        void Delete(string id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
    }
}
