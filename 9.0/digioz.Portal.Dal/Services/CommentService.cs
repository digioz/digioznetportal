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
    }
}
