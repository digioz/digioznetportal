using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class OrderService : IOrderService
    {
        private readonly digiozPortalContext _context;

        public OrderService(digiozPortalContext context)
        {
            _context = context;
        }

        public Order Get(string id)
        {
            return _context.Orders.Find(id);
        }

        public List<Order> GetAll()
        {
            return _context.Orders.ToList();
        }

        public List<Order> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Order>();

            return _context.Orders.Where(o => !string.IsNullOrEmpty(o.UserId) && o.UserId == userId).ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Orders.Count(o => !string.IsNullOrEmpty(o.UserId) && o.UserId == userId);
        }

        public void Add(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public void Update(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
        }
    }
}
