using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IRssService
    {
        Rss Get(int id);
        List<Rss> GetAll();
        List<Rss> GetPage(int pageNumber, int pageSize, out int totalCount);
        void Add(Rss rss);
        void Update(Rss rss);
        void Delete(int id);
    }
}
