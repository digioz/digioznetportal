using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListCampaignRelationService
    {
        MailingListCampaignRelation Get(string id);
        List<MailingListCampaignRelation> GetAll();
        void Add(MailingListCampaignRelation relation);
        void Update(MailingListCampaignRelation relation);
        void Delete(string id);
    }
}
