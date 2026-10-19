using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListCampaignService
    {
        MailingListCampaign Get(string id);
        List<MailingListCampaign> GetAll();
        void Add(MailingListCampaign campaign);
        void Update(MailingListCampaign campaign);
        void Delete(string id);
    }
}
