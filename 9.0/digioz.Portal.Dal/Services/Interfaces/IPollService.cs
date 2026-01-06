using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollService
    {
        Poll Get(string id);
        List<Poll> GetAll();
        List<Poll> GetByUserId(string userId);
        List<Poll> GetLatest(int count);
        List<Poll> GetLatestFeatured(int count);
        List<Poll> GetByIds(IEnumerable<string> ids);
        List<Poll> GetPaged(int pageNumber, int pageSize, out int totalCount);
        List<Poll> GetPagedFiltered(int pageNumber, int pageSize, string userId, out int totalCount);
        int CountByUserId(string userId);
        void Add(Poll poll);
        void Update(Poll poll);
        void Delete(string id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
    }
}
