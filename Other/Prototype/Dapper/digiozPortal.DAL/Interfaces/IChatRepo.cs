using System.Collections.Generic;
using digiozPortal.BO;

namespace digiozPortal.DAL.Interfaces
{
    public interface IChatRepo : IRepo<Chat>
    {
        List<Chat> GetLatestChats(int top, string orderBy, string order, string fields="*");
    }
}
