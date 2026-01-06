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

        public List<Poll> GetLatest(int count)
        {
            return _context.Polls
                .Where(p => p.Visible == true && p.Approved == true)
                .OrderByDescending(p => p.DateCreated)
                .Take(count)
                .ToList();
        }

        public List<Poll> GetLatestFeatured(int count)
        {
            return _context.Polls
                .Where(p => p.Featured && p.Visible == true && p.Approved == true)
                .OrderByDescending(p => p.DateCreated)
                .Take(count)
                .ToList();
        }

        public List<Poll> GetByIds(IEnumerable<string> ids)
        {
            var set = ids?.ToHashSet() ?? new HashSet<string>();
            if (set.Count == 0) return new List<Poll>();
            return _context.Polls.Where(p => set.Contains(p.Id)).ToList();
        }

        public List<Poll> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Poll>();

            return _context.Polls.Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId).ToList();
        }

        public List<Poll> GetPaged(int pageNumber, int pageSize, out int totalCount)
        {
            var query = _context.Polls.OrderByDescending(p => p.DateCreated);
            totalCount = query.Count();
            var skip = (pageNumber - 1) * pageSize;
            return query.Skip(skip).Take(pageSize).ToList();
        }

        public List<Poll> GetPagedFiltered(int pageNumber, int pageSize, string userId, out int totalCount)
        {
            var query = _context.Polls.AsQueryable();
            
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => 
                    (p.UserId == userId) || 
                    (p.Visible == true && p.Approved == true)
                );
            }
            else
            {
                query = query.Where(p => p.Visible == true && p.Approved == true);
            }
            
            query = query.OrderByDescending(p => p.DateCreated);
            totalCount = query.Count();
            
            var skip = (pageNumber - 1) * pageSize;
            return query.Skip(skip).Take(pageSize).ToList();
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

        public int DeleteByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var polls = _context.Polls
                .Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId)
                .ToList();

            if (polls.Any())
            {
                _context.Polls.RemoveRange(polls);
                _context.SaveChanges();
            }

            return polls.Count;
        }

        public int ReassignByUserId(string fromUserId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
                return 0;

            var polls = _context.Polls
                .Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == fromUserId)
                .ToList();

            if (polls.Any())
            {
                foreach (var poll in polls)
                {
                    poll.UserId = toUserId;
                }
                _context.SaveChanges();
            }

            return polls.Count;
        }
    }
}
