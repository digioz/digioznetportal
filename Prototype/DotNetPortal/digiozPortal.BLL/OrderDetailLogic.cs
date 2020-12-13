using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class OrderDetailLogic
    {
        public OrderDetail Get(string id) {
            var OrderDetailRepo = new OrderDetailRepo();
            OrderDetail OrderDetail = OrderDetailRepo.Get(id);

            return OrderDetail;
        }

        public List<OrderDetail> GetAll() {
            var OrderDetailRepo = new OrderDetailRepo();
            var OrderDetails = OrderDetailRepo.GetAll();

            return OrderDetails;
        }

        public void Add(OrderDetail OrderDetail) {
            var OrderDetailRepo = new OrderDetailRepo();
            OrderDetailRepo.Add(OrderDetail);
        }

        public void Edit(OrderDetail OrderDetail) {
            var OrderDetailRepo = new OrderDetailRepo();

            OrderDetailRepo.Edit(OrderDetail);
        }

        public void Delete(string id) {
            var OrderDetailRepo = new OrderDetailRepo();
            var OrderDetail = OrderDetailRepo.Get(id); // Db.OrderDetails.Find(id);

            if (OrderDetail != null) {
                OrderDetailRepo.Delete(OrderDetail);
            }
        }
    }

}
