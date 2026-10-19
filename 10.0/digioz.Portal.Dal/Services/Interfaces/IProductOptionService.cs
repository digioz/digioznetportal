using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IProductOptionService
    {
        ProductOption Get(string id);
        List<ProductOption> GetAll();
        void Add(ProductOption option);
        void Update(ProductOption option);
        void Delete(string id);
    }
}
