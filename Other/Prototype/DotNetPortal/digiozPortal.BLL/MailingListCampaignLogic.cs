using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MailingListCampaignLogic
    {
        public MailingListCampaign Get(string id) {
            var MailingListCampaignRepo = new MailingListCampaignRepo();
            MailingListCampaign MailingListCampaign = MailingListCampaignRepo.Get(id);

            return MailingListCampaign;
        }

        public List<MailingListCampaign> GetAll() {
            var MailingListCampaignRepo = new MailingListCampaignRepo();
            var MailingListCampaigns = MailingListCampaignRepo.GetAll();

            return MailingListCampaigns;
        }

        public void Add(MailingListCampaign MailingListCampaign) {
            var MailingListCampaignRepo = new MailingListCampaignRepo();
            MailingListCampaignRepo.Add(MailingListCampaign);
        }

        public void Edit(MailingListCampaign MailingListCampaign) {
            var MailingListCampaignRepo = new MailingListCampaignRepo();

            MailingListCampaignRepo.Edit(MailingListCampaign);
        }

        public void Delete(string id) {
            var MailingListCampaignRepo = new MailingListCampaignRepo();
            var MailingListCampaign = MailingListCampaignRepo.Get(id); // Db.MailingListCampaigns.Find(id);

            if (MailingListCampaign != null) {
                MailingListCampaignRepo.Delete(MailingListCampaign);
            }
        }
    }

}
