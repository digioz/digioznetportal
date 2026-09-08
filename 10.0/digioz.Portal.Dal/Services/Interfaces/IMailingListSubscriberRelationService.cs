using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListSubscriberRelationService
    {
        MailingListSubscriberRelation Get(string id);
        List<MailingListSubscriberRelation> GetAll();
        List<MailingListSubscriberRelation> GetByMailingListId(string mailingListId);
        List<MailingListSubscriberRelation> GetBySubscriberId(string subscriberId);
        MailingListSubscriberRelation GetByMailingListAndSubscriber(string mailingListId, string subscriberId);
        void Add(MailingListSubscriberRelation relation);
        void Update(MailingListSubscriberRelation relation);
        void Delete(string id);
        void DeleteByMailingListAndSubscriber(string mailingListId, string subscriberId);
    }
}
