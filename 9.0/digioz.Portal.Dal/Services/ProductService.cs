using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ProductService : IProductService
    {
        private readonly digiozPortalContext _context;

        public ProductService(digiozPortalContext context)
        {
            _context = context;
        }

        public Product Get(string id)
        {
            return _context.Products.Find(id);
        }

        public List<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        public void IncrementViews(string id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Views++;
                _context.SaveChanges();
            }
        }
    }
}
