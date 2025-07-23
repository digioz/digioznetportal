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

        public Picture Get(string id)
        {
            return _context.Pictures.Find(id);
        }

        public List<Picture> GetAll()
        {
            return _context.Pictures.ToList();
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

        public void Delete(string id)
        {
            var picture = _context.Pictures.Find(id);
            if (picture != null)
            {
                _context.Pictures.Remove(picture);
                _context.SaveChanges();
            }
        }
    }
}
