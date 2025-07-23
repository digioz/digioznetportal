using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IChatService
    {
        Chat Get(int id);
        List<Chat> GetAll();
        void Add(Chat chat);
        void Update(Chat chat);
        void Delete(int id);
    }
}
