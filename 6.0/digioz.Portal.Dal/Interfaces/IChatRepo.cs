using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Interfaces
{
    public interface IChatRepo : IRepo<Chat>
    {
        List<Chat> GetLatestChats(int top, string orderBy, string order, string fields="*");
    }
}
