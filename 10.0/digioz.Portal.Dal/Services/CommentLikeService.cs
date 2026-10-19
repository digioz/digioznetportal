using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class CommentLikeService : ICommentLikeService
    {
        private readonly digiozPortalContext _context;

        public CommentLikeService(digiozPortalContext context)
        {
            _context = context;
        }

        public CommentLike Get(string id)
        {
            return _context.CommentLikes.Find(id);
        }

        public List<CommentLike> GetAll()
        {
            return _context.CommentLikes.ToList();
        }

        public void Add(CommentLike like)
        {
            _context.CommentLikes.Add(like);
            _context.SaveChanges();
        }

        public void Update(CommentLike like)
        {
            _context.CommentLikes.Update(like);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var like = _context.CommentLikes.Find(id);
            if (like != null)
            {
                _context.CommentLikes.Remove(like);
                _context.SaveChanges();
            }
        }
    }
}
