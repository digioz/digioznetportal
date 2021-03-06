using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;

namespace digioz.Portal.Bll.Interfaces
{
    public interface IChatLogic : ILogic<Chat>
    {
        List<ChatViewModel> GetLatestChats();
    }
}
