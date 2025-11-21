using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PollService : IPollService
    {
        private readonly digiozPortalContext _context;

        public PollService(digiozPortalContext context)
        {
            _context = context;
        }

        public Poll Get(string id)
        {
            return _context.Polls.Find(id);
        }

        public List<Poll> GetAll()
        {
            return _context.Polls.ToList();
        }

        public List<Poll> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Poll>();

            return _context.Polls.Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId).ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Polls.Count(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId);
        }

        public void Add(Poll poll)
        {
            _context.Polls.Add(poll);
            _context.SaveChanges();
        }

        public void Update(Poll poll)
        {
            _context.Polls.Update(poll);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var poll = _context.Polls.Find(id);
            if (poll != null)
            {
                _context.Polls.Remove(poll);
                _context.SaveChanges();
            }
        }
    }
}
