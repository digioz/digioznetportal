using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ProductOptionService : IProductOptionService
    {
        private readonly digiozPortalContext _context;

        public ProductOptionService(digiozPortalContext context)
        {
            _context = context;
        }

        public ProductOption Get(string id)
        {
            return _context.ProductOptions.Find(id);
        }

        public List<ProductOption> GetAll()
        {
            return _context.ProductOptions.ToList();
        }

        public void Add(ProductOption option)
        {
            _context.ProductOptions.Add(option);
            _context.SaveChanges();
        }

        public void Update(ProductOption option)
        {
            _context.ProductOptions.Update(option);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var option = _context.ProductOptions.Find(id);
            if (option != null)
            {
                _context.ProductOptions.Remove(option);
                _context.SaveChanges();
            }
        }
    }
}
