using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ILinkCategoryService
    {
        LinkCategory Get(int id);
        List<LinkCategory> GetAll();
        void Add(LinkCategory category);
        void Update(LinkCategory category);
        void Delete(int id);
    }
}
