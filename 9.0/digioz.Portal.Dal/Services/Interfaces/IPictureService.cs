using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPictureService
    {
        Picture Get(int id);
        List<Picture> GetAll();
        List<Picture> GetFiltered(string userId = null, int? albumId = null, bool? visible = null, bool? approved = null, bool isAdmin = false);
        int CountByUserId(string userId);
        void Add(Picture picture);
        void Update(Picture picture);
        void Delete(int id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
    }
}
