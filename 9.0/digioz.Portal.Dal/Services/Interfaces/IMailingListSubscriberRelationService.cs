using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListSubscriberRelationService
    {
        MailingListSubscriberRelation Get(string id);
        List<MailingListSubscriberRelation> GetAll();
        void Add(MailingListSubscriberRelation relation);
        void Update(MailingListSubscriberRelation relation);
        void Delete(string id);
    }
}
