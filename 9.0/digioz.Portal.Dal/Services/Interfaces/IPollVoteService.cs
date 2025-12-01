using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollVoteService
    {
        PollVote Get(string id);
        List<PollVote> GetAll();
        int CountByAnswerId(string answerId);
        void DeleteByAnswerId(string answerId);
        List<PollVote> GetByPollAnswerIds(IEnumerable<string> answerIds);
        void Add(PollVote vote);
        void Update(PollVote vote);
        void Delete(string id);
    }
}
