using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class VideoService : IVideoService
    {
        private readonly digiozPortalContext _context;

        public VideoService(digiozPortalContext context)
        {
            _context = context;
        }

        public Video Get(int id)
        {
            return _context.Videos.Find(id);
        }

        public List<Video> GetByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Video>();
            }
            return _context.Videos.Where(v => ids.Contains(v.Id)).ToList();
        }

        public List<Video> GetAll()
        {
            return _context.Videos.ToList();
        }

        public List<Video> GetFiltered(string userId = null, int? albumId = null, bool? visible = null, bool? approved = null, bool isAdmin = false)
        {
            var query = _context.Videos.AsQueryable();

            // Apply album filter if specified
            if (albumId.HasValue)
            {
                query = query.Where(v => v.AlbumId == albumId.Value);
            }

            // Apply visibility and approval filters based on user role
            if (!isAdmin)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    // User can see their own videos (any visibility/approval) or public approved videos
                    query = query.Where(v => v.UserId == userId || (v.Visible && v.Approved));
                }
                else
                {
                    // Anonymous users only see visible and approved videos
                    query = query.Where(v => v.Visible && v.Approved);
                }
            }

            // Apply explicit visibility filter if specified
            if (visible.HasValue)
            {
                query = query.Where(v => v.Visible == visible.Value);
            }

            // Apply explicit approval filter if specified
            if (approved.HasValue)
            {
                query = query.Where(v => v.Approved == approved.Value);
            }

            return query.OrderByDescending(v => v.Timestamp).ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Videos.Count(v => !string.IsNullOrEmpty(v.UserId) && v.UserId == userId);
        }

        public void Add(Video video)
        {
            _context.Videos.Add(video);
            _context.SaveChanges();
        }

        public void Update(Video video)
        {
            _context.Videos.Update(video);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var video = _context.Videos.Find(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();
            }
        }

        public void IncrementViews(int id)
        {
            var video = _context.Videos.Find(id);
            if (video != null)
            {
                video.Views++;
                _context.SaveChanges();
            }
        }

        public int DeleteByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var videos = _context.Videos
                .Where(v => !string.IsNullOrEmpty(v.UserId) && v.UserId == userId)
                .ToList();

            if (videos.Any())
            {
                _context.Videos.RemoveRange(videos);
                _context.SaveChanges();
            }

            return videos.Count;
        }

        public int ReassignByUserId(string fromUserId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
                return 0;

            var videos = _context.Videos
                .Where(v => !string.IsNullOrEmpty(v.UserId) && v.UserId == fromUserId)
                .ToList();

            if (videos.Any())
            {
                foreach (var video in videos)
                {
                    video.UserId = toUserId;
                }
                _context.SaveChanges();
            }

            return videos.Count;
        }

        public List<Video> Search(string term, int skip, int take, out int totalCount)
        {
            term = term ?? string.Empty;
            var q = _context.Videos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(v => v.Visible && v.Approved && (
                    (v.Filename != null && v.Filename.ToLower().Contains(t)) ||
                    (v.Description != null && v.Description.ToLower().Contains(t))
                ));
            }
            else
            {
                q = q.Where(v => v.Visible && v.Approved);
            }

            totalCount = q.Count();
            return q
                .OrderByDescending(v => v.Timestamp ?? System.DateTime.MinValue)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
