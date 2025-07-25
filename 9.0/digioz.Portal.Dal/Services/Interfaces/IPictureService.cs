using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPictureService
    {
        Picture Get(string id);
        List<Picture> GetAll();
        void Add(Picture picture);
        void Update(Picture picture);
        void Delete(string id);
    }
}
