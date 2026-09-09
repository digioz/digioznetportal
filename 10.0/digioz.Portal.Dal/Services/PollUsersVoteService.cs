using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PollUsersVoteService : IPollUsersVoteService
    {
        private readonly digiozPortalContext _context;

        public PollUsersVoteService(digiozPortalContext context)
        {
            _context = context;
        }

        public PollUsersVote Get(string pollId, string userId)
        {
            return _context.PollUsersVotes
                .FirstOrDefault(x => x.PollId == pollId && x.UserId == userId);
        }

        public List<PollUsersVote> GetAll()
        {
            return _context.PollUsersVotes.ToList();
        }

        public List<PollUsersVote> GetByUserId(string userId)
        {
            return _context.PollUsersVotes.Where(x => x.UserId == userId).ToList();
        }

        public bool Exists(string pollId, string userId)
        {
            return _context.PollUsersVotes.Any(x => x.PollId == pollId && x.UserId == userId);
        }

        public void Add(PollUsersVote usersVote)
        {
            _context.PollUsersVotes.Add(usersVote);
            _context.SaveChanges();
        }

        public void Update(PollUsersVote usersVote)
        {
            _context.PollUsersVotes.Update(usersVote);
            _context.SaveChanges();
        }

        public void Delete(string pollId, string userId)
        {
            var usersVote = _context.PollUsersVotes
                .FirstOrDefault(x => x.PollId == pollId && x.UserId == userId);
            if (usersVote != null)
            {
                _context.PollUsersVotes.Remove(usersVote);
                _context.SaveChanges();
            }
        }

        public void DeleteByPollId(string pollId)
        {
            var items = _context.PollUsersVotes.Where(x => x.PollId == pollId).ToList();
            if (items.Count > 0)
            {
                _context.PollUsersVotes.RemoveRange(items);
                _context.SaveChanges();
            }
        }
    }
}
