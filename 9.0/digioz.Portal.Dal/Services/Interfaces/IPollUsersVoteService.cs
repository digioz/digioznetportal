using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollUsersVoteService
    {
        PollUsersVote Get(string pollId, string userId);
        List<PollUsersVote> GetAll();
        List<PollUsersVote> GetByUserId(string userId);
        bool Exists(string pollId, string userId);
        void Add(PollUsersVote usersVote);
        void Update(PollUsersVote usersVote);
        void Delete(string pollId, string userId);
    }
}
