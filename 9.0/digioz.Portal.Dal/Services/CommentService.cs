using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

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
            return _context.Comments.ToList();
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
