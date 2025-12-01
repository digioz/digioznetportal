using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PollVoteService : IPollVoteService
    {
        private readonly digiozPortalContext _context;

        public PollVoteService(digiozPortalContext context)
        {
            _context = context;
        }

        public PollVote Get(string id)
        {
            return _context.PollVotes.Find(id);
        }

        public List<PollVote> GetAll()
        {
            return _context.PollVotes.ToList();
        }

        public int CountByAnswerId(string answerId)
        {
            return _context.PollVotes.Count(v => v.PollAnswerId == answerId);
        }

        public void DeleteByAnswerId(string answerId)
        {
            var votes = _context.PollVotes.Where(v => v.PollAnswerId == answerId).ToList();
            if (votes.Count > 0)
            {
                _context.PollVotes.RemoveRange(votes);
                _context.SaveChanges();
            }
        }

        public List<PollVote> GetByPollAnswerIds(IEnumerable<string> answerIds)
        {
            var set = answerIds?.ToHashSet() ?? new HashSet<string>();
            if (set.Count == 0) return new List<PollVote>();
            return _context.PollVotes.Where(v => set.Contains(v.PollAnswerId)).ToList();
        }

        public void Add(PollVote vote)
        {
            _context.PollVotes.Add(vote);
            _context.SaveChanges();
        }

        public void Update(PollVote vote)
        {
            _context.PollVotes.Update(vote);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var vote = _context.PollVotes.Find(id);
            if (vote != null)
            {
                _context.PollVotes.Remove(vote);
                _context.SaveChanges();
            }
        }
    }
}
