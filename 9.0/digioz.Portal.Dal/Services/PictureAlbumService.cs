using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PictureAlbumService : IPictureAlbumService
    {
        private readonly digiozPortalContext _context;

        public PictureAlbumService(digiozPortalContext context)
        {
            _context = context;
        }

        public PictureAlbum Get(int id)
        {
            return _context.PictureAlbums.Find(id);
        }

        public List<PictureAlbum> GetAll()
        {
            return _context.PictureAlbums.ToList();
        }

        public void Add(PictureAlbum album)
        {
            _context.PictureAlbums.Add(album);
            _context.SaveChanges();
        }

        public void Update(PictureAlbum album)
        {
            _context.PictureAlbums.Update(album);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var album = _context.PictureAlbums.Find(id);
            if (album != null)
            {
                _context.PictureAlbums.Remove(album);
                _context.SaveChanges();
            }
        }
    }
}
