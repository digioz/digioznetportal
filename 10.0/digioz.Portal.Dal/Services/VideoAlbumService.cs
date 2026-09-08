using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class VideoAlbumService : IVideoAlbumService
    {
        private readonly digiozPortalContext _context;

        public VideoAlbumService(digiozPortalContext context)
        {
            _context = context;
        }

        public VideoAlbum Get(int id)
        {
            return _context.VideoAlbums.Find(id);
        }

        public List<VideoAlbum> GetAll()
        {
            return _context.VideoAlbums.ToList();
        }

        public void Add(VideoAlbum album)
        {
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();
        }

        public void Update(VideoAlbum album)
        {
            _context.VideoAlbums.Update(album);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var album = _context.VideoAlbums.Find(id);
            if (album != null)
            {
                _context.VideoAlbums.Remove(album);
                _context.SaveChanges();
            }
        }
    }
}
