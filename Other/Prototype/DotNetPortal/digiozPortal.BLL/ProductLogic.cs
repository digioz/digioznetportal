using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ProductLogic
    {
        public Product Get(string id) {
            var ProductRepo = new ProductRepo();
            Product Product = ProductRepo.Get(id);

            return Product;
        }

        public List<Product> GetAll() {
            var ProductRepo = new ProductRepo();
            var Products = ProductRepo.GetAll();

            return Products;
        }

        public void Add(Product Product) {
            var ProductRepo = new ProductRepo();
            ProductRepo.Add(Product);
        }

        public void Edit(Product Product) {
            var ProductRepo = new ProductRepo();

            ProductRepo.Edit(Product);
        }

        public void Delete(string id) {
            var ProductRepo = new ProductRepo();
            var Product = ProductRepo.Get(id); // Db.Products.Find(id);

            if (Product != null) {
                ProductRepo.Delete(Product);
            }
        }
    }

}
