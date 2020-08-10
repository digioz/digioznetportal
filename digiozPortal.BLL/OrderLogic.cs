using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class OrderLogic
    {
        public Order Get(string id) {
            var OrderRepo = new OrderRepo();
            Order Order = OrderRepo.Get(id);

            return Order;
        }

        public List<Order> GetAll() {
            var OrderRepo = new OrderRepo();
            var Orders = OrderRepo.GetAll();

            return Orders;
        }

        public void Add(Order Order) {
            var OrderRepo = new OrderRepo();
            OrderRepo.Add(Order);
        }

        public void Edit(Order Order) {
            var OrderRepo = new OrderRepo();

            OrderRepo.Edit(Order);
        }

        public void Delete(string id) {
            var OrderRepo = new OrderRepo();
            var Order = OrderRepo.Get(id); // Db.Orders.Find(id);

            if (Order != null) {
                OrderRepo.Delete(Order);
            }
        }
    }

}
