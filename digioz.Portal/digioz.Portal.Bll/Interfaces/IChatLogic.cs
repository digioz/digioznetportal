using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Bll.Interfaces
{
    public interface IChatLogic : ILogic<Chat>
    {
        List<Chat> GetLatestChats();
    }
}
