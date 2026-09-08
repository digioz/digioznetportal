using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly digiozPortalContext _context;

        public ProductCategoryService(digiozPortalContext context)
        {
            _context = context;
        }

        public ProductCategory Get(string id)
        {
            return _context.ProductCategories.Find(id);
        }

        public List<ProductCategory> GetAll()
        {
            return _context.ProductCategories.ToList();
        }

        public void Add(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
            _context.SaveChanges();
        }

        public void Update(ProductCategory category)
        {
            _context.ProductCategories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var category = _context.ProductCategories.Find(id);
            if (category != null)
            {
                _context.ProductCategories.Remove(category);
                _context.SaveChanges();
            }
        }
    }
}
