using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPageService
    {
        Page Get(int id);
        List<Page> GetAll();
        void Add(Page page);
        void Update(Page page);
        void Delete(int id);
        Page GetByTitle(string title);
        Page GetByUrl(string url);
    }
}
