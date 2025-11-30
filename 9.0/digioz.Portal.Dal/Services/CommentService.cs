using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Services
{
    public class CommentService : ICommentService
    {
        private readonly digiozPortalContext _context;

        public CommentService(digiozPortalContext context)
        {
            _context = context;
        }

        public Comment Get(string id)
        {
            return _context.Comments.Find(id);
        }

        public List<Comment> GetAll()
        {
            return _context.Comments.AsNoTracking().ToList();
        }

        public List<Comment> GetByReferenceType(string referenceType)
        {
            return _context.Comments
                .AsNoTracking()
                .Where(c => c.ReferenceType == referenceType)
                .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate)
                .ToList();
        }

        public List<Comment> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Comment>();

            return _context.Comments
                .AsNoTracking()
                .Where(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == userId)
                .ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Comments.Count(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == userId);
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
        }

        public int DeleteByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var comments = _context.Comments
                .Where(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == userId)
                .ToList();

            if (comments.Any())
            {
                _context.Comments.RemoveRange(comments);
                _context.SaveChanges();
            }

            return comments.Count;
        }

        public int ReassignByUserId(string fromUserId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
                return 0;

            var comments = _context.Comments
                .Where(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == fromUserId)
                .ToList();

            if (comments.Any())
            {
                foreach (var comment in comments)
                {
                    comment.UserId = toUserId;
                }
                _context.SaveChanges();
            }

            return comments.Count;
        }

        public List<Comment> Search(string term, int skip, int take, out int totalCount)
        {
            term = term ?? string.Empty;
            var q = _context.Comments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(c => c.Body != null && c.Body.ToLower().Contains(t));
            }

            totalCount = q.Count();
            return q
                .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate ?? System.DateTime.MinValue)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
