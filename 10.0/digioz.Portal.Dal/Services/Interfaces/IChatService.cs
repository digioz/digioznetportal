using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IChatService
    {
        Chat Get(int id);
        List<Chat> GetAll();
        List<Chat> GetByUserId(string userId);
        int CountByUserId(string userId);
        void Add(Chat chat);
        void Update(Chat chat);
        void Delete(int id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
    }
}
