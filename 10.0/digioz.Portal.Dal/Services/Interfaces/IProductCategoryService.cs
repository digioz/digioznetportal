using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IProductCategoryService
    {
        ProductCategory Get(string id);
        List<ProductCategory> GetAll();
        void Add(ProductCategory category);
        void Update(ProductCategory category);
        void Delete(string id);
    }
}
