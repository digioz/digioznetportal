using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IZoneService
    {
        Zone Get(int id);
        List<Zone> GetAll();
        void Add(Zone zone);
        void Update(Zone zone);
        void Delete(int id);
    }
}
