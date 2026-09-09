using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPictureAlbumService
    {
        PictureAlbum Get(int id);
        List<PictureAlbum> GetAll();
        void Add(PictureAlbum album);
        void Update(PictureAlbum album);
        void Delete(int id);
    }
}
