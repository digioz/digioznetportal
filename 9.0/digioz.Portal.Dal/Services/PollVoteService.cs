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
