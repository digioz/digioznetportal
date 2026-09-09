using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly digiozPortalContext _context;

        public OrderDetailService(digiozPortalContext context)
        {
            _context = context;
        }

        public OrderDetail Get(string id)
        {
            return _context.OrderDetails.Find(id);
        }

        public List<OrderDetail> GetAll()
        {
            return _context.OrderDetails.ToList();
        }

        public void Add(OrderDetail detail)
        {
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();
        }

        public void Update(OrderDetail detail)
        {
            _context.OrderDetails.Update(detail);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var detail = _context.OrderDetails.Find(id);
            if (detail != null)
            {
                _context.OrderDetails.Remove(detail);
                _context.SaveChanges();
            }
        }
    }
}
