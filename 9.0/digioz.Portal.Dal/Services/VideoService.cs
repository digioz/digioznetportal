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

        public Video Get(string id)
        {
            return _context.Videos.Find(id);
        }

        public List<Video> GetAll()
        {
            return _context.Videos.ToList();
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

        public void Delete(string id)
        {
            var video = _context.Videos.Find(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();
            }
        }
    }
}
