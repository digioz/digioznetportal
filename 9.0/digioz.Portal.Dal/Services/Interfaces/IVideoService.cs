using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVideoService
    {
        Video Get(int id);
        List<Video> GetAll();
        List<Video> GetFiltered(string userId = null, int? albumId = null, bool? visible = null, bool? approved = null, bool isAdmin = false);
        int CountByUserId(string userId);
        void Add(Video video);
        void Update(Video video);
        void Delete(int id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
    }
}
