using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IProductService
    {
        Product Get(string id);
        List<Product> GetAll();
        void Add(Product product);
        void Update(Product product);
        void Delete(string id);
        void IncrementViews(string id);
        void ClearProductCategory(string categoryId);
    }
}
