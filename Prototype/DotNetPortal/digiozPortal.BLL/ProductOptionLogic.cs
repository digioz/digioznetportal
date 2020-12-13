using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ProductOptionLogic
    {
        public ProductOption Get(string id) {
            var ProductOptionRepo = new ProductOptionRepo();
            ProductOption ProductOption = ProductOptionRepo.Get(id);

            return ProductOption;
        }

        public List<ProductOption> GetAll() {
            var ProductOptionRepo = new ProductOptionRepo();
            var ProductOptions = ProductOptionRepo.GetAll();

            return ProductOptions;
        }

        public void Add(ProductOption ProductOption) {
            var ProductOptionRepo = new ProductOptionRepo();
            ProductOptionRepo.Add(ProductOption);
        }

        public void Edit(ProductOption ProductOption) {
            var ProductOptionRepo = new ProductOptionRepo();

            ProductOptionRepo.Edit(ProductOption);
        }

        public void Delete(string id) {
            var ProductOptionRepo = new ProductOptionRepo();
            var ProductOption = ProductOptionRepo.Get(id); // Db.ProductOptions.Find(id);

            if (ProductOption != null) {
                ProductOptionRepo.Delete(ProductOption);
            }
        }
    }

}
