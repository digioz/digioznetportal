using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IOrderDetailService
    {
        OrderDetail Get(string id);
        List<OrderDetail> GetAll();
        void Add(OrderDetail detail);
        void Update(OrderDetail detail);
        void Delete(string id);
    }
}
