using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListCampaignRelationService
    {
        MailingListCampaignRelation Get(string id);
        List<MailingListCampaignRelation> GetAll();
        List<MailingListCampaignRelation> GetByMailingListId(string mailingListId);
        List<MailingListCampaignRelation> GetByCampaignId(string campaignId);
        MailingListCampaignRelation GetByMailingListAndCampaign(string mailingListId, string campaignId);
        void Add(MailingListCampaignRelation relation);
        void Update(MailingListCampaignRelation relation);
        void Delete(string id);
        void DeleteByMailingListAndCampaign(string mailingListId, string campaignId);
    }
}
