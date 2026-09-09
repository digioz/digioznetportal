using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListSubscriberService
    {
        MailingListSubscriber Get(string id);
        List<MailingListSubscriber> GetAll();
        void Add(MailingListSubscriber subscriber);
        void Update(MailingListSubscriber subscriber);
        void Delete(string id);
    }
}
