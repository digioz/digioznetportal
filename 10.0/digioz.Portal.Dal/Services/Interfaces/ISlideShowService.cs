using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ISlideShowService
    {
        SlideShow Get(string id);
        List<SlideShow> GetAll();
        void Add(SlideShow slideShow);
        void Update(SlideShow slideShow);
        void Delete(string id);
    }
}
