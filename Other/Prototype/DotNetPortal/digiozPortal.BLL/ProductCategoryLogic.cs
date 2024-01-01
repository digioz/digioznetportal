using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ProductCategoryLogic
    {
        public ProductCategory Get(string id) {
            var productCategoryRepo = new ProductCategoryRepo();
            ProductCategory productCategory = productCategoryRepo.Get(id);

            return productCategory;
        }

        public List<ProductCategory> GetAll() {
            var productCategoryRepo = new ProductCategoryRepo();
            var productCategorys = productCategoryRepo.GetAll();

            return productCategorys;
        }

        public void Add(ProductCategory ProductCategory) {
            var productCategoryRepo = new ProductCategoryRepo();
            productCategoryRepo.Add(ProductCategory);
        }

        public void Edit(ProductCategory ProductCategory) {
            var productCategoryRepo = new ProductCategoryRepo();

            productCategoryRepo.Edit(ProductCategory);
        }

        public void Delete(string id) {
            var productCategoryRepo = new ProductCategoryRepo();
            var productCategory = productCategoryRepo.Get(id); // Db.ProductCategorys.Find(id);

            if (productCategory != null) {
                productCategoryRepo.Delete(productCategory);
            }
        }
    }

}
