using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PictureService : IPictureService
    {
        private readonly digiozPortalContext _context;

        public PictureService(digiozPortalContext context)
        {
            _context = context;
        }

        public Picture Get(int id)
        {
            return _context.Pictures.Find(id);
        }

        public List<Picture> GetByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Picture>();
            }
            return _context.Pictures.Where(p => ids.Contains(p.Id)).ToList();
        }

        public List<Picture> GetAll()
        {
            return _context.Pictures.ToList();
        }

        public List<Picture> GetFiltered(string userId = null, int? albumId = null, bool? visible = null, bool? approved = null, bool isAdmin = false)
        {
            var query = _context.Pictures.AsQueryable();

            // Apply album filter if specified
            if (albumId.HasValue)
            {
                query = query.Where(p => p.AlbumId == albumId.Value);
            }

            // Apply visibility and approval filters based on user role
            if (!isAdmin)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    // User can see their own pictures (any visibility/approval) or public approved pictures
                    query = query.Where(p => p.UserId == userId || (p.Visible && p.Approved));
                }
                else
                {
                    // Anonymous users only see visible and approved pictures
                    query = query.Where(p => p.Visible && p.Approved);
                }
            }

            // Apply explicit visibility filter if specified
            if (visible.HasValue)
            {
                query = query.Where(p => p.Visible == visible.Value);
            }

            // Apply explicit approval filter if specified
            if (approved.HasValue)
            {
                query = query.Where(p => p.Approved == approved.Value);
            }

            return query.OrderByDescending(p => p.Timestamp).ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Pictures.Count(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId);
        }

        public void Add(Picture picture)
        {
            _context.Pictures.Add(picture);
            _context.SaveChanges();
        }

        public void Update(Picture picture)
        {
            _context.Pictures.Update(picture);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var picture = _context.Pictures.Find(id);
            if (picture != null)
            {
                _context.Pictures.Remove(picture);
                _context.SaveChanges();
            }
        }

        public int DeleteByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var pictures = _context.Pictures
                .Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == userId)
                .ToList();

            if (pictures.Any())
            {
                _context.Pictures.RemoveRange(pictures);
                _context.SaveChanges();
            }

            return pictures.Count;
        }

        public int ReassignByUserId(string fromUserId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
                return 0;

            var pictures = _context.Pictures
                .Where(p => !string.IsNullOrEmpty(p.UserId) && p.UserId == fromUserId)
                .ToList();

            if (pictures.Any())
            {
                foreach (var picture in pictures)
                {
                    picture.UserId = toUserId;
                }
                _context.SaveChanges();
            }

            return pictures.Count;
        }
    }
}
