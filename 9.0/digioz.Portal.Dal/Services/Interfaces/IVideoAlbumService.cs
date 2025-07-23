using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IVideoAlbumService
    {
        VideoAlbum Get(int id);
        List<VideoAlbum> GetAll();
        void Add(VideoAlbum album);
        void Update(VideoAlbum album);
        void Delete(int id);
    }
}
