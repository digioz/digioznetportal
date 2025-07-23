using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollVoteService
    {
        PollVote Get(string id);
        List<PollVote> GetAll();
        void Add(PollVote vote);
        void Update(PollVote vote);
        void Delete(string id);
    }
}
