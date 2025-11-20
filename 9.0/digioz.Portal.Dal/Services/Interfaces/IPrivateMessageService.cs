using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPrivateMessageService
    {
        PrivateMessage Get(int id);
        List<PrivateMessage> GetInbox(string userId); // messages sent TO user
        List<PrivateMessage> GetOutbox(string userId); // messages user sent not read yet
        List<PrivateMessage> GetSent(string userId); // messages user sent and have been read
        List<PrivateMessage> GetThread(int rootOrReplyId); // ordered newest first
        void Add(PrivateMessage message);
        void MarkRead(int id);
        void Delete(int id, string userId); // only allow if owner (sender or receiver)
    }
}
