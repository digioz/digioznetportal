using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVideoService
    {
        Video Get(string id);
        List<Video> GetAll();
        void Add(Video video);
        void Update(Video video);
        void Delete(string id);
    }
}
