using System.Collections.Generic;
using digiozPortal.BO;

namespace digiozPortal.BLL.Interfaces
{
    public interface IChatLogic : ILogic<Chat>
    {
        List<Chat> GetLatestChats();
    }
}
