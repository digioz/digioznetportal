using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class SlideShowService : ISlideShowService
    {
        private readonly digiozPortalContext _context;

        public SlideShowService(digiozPortalContext context)
        {
            _context = context;
        }

        public SlideShow Get(string id)
        {
            return _context.SlideShows.Find(id);
        }

        public List<SlideShow> GetAll()
        {
            return _context.SlideShows.ToList();
        }

        public void Add(SlideShow slideShow)
        {
            _context.SlideShows.Add(slideShow);
            _context.SaveChanges();
        }

        public void Update(SlideShow slideShow)
        {
            _context.SlideShows.Update(slideShow);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var slideShow = _context.SlideShows.Find(id);
            if (slideShow != null)
            {
                _context.SlideShows.Remove(slideShow);
                _context.SaveChanges();
            }
        }
    }
}
