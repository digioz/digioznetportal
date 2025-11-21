using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollService
    {
        Poll Get(string id);
        List<Poll> GetAll();
        List<Poll> GetByUserId(string userId);
        int CountByUserId(string userId);
        void Add(Poll poll);
        void Update(Poll poll);
        void Delete(string id);
    }
}
