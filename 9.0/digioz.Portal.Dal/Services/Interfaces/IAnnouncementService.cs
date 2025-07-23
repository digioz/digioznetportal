using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Announcement Get(int id);
        List<Announcement> GetAll();
        void Add(Announcement announcement);
        void Update(Announcement announcement);
        void Delete(int id);
    }
}
