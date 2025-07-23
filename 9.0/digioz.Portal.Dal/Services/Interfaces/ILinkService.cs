using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ILinkService
    {
        Link Get(int id);
        List<Link> GetAll();
        void Add(Link link);
        void Update(Link link);
        void Delete(int id);
    }
}
