using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MailingListCampaignRelationLogic
    {
        public MailingListCampaignRelation Get(string id) {
            var MailingListCampaignRelationRepo = new MailingListCampaignRelationRepo();
            MailingListCampaignRelation MailingListCampaignRelation = MailingListCampaignRelationRepo.Get(id);

            return MailingListCampaignRelation;
        }

        public List<MailingListCampaignRelation> GetAll() {
            var MailingListCampaignRelationRepo = new MailingListCampaignRelationRepo();
            var MailingListCampaignRelations = MailingListCampaignRelationRepo.GetAll();

            return MailingListCampaignRelations;
        }

        public void Add(MailingListCampaignRelation MailingListCampaignRelation) {
            var MailingListCampaignRelationRepo = new MailingListCampaignRelationRepo();
            MailingListCampaignRelationRepo.Add(MailingListCampaignRelation);
        }

        public void Edit(MailingListCampaignRelation MailingListCampaignRelation) {
            var MailingListCampaignRelationRepo = new MailingListCampaignRelationRepo();

            MailingListCampaignRelationRepo.Edit(MailingListCampaignRelation);
        }

        public void Delete(string id) {
            var MailingListCampaignRelationRepo = new MailingListCampaignRelationRepo();
            var MailingListCampaignRelation = MailingListCampaignRelationRepo.Get(id); // Db.MailingListCampaignRelations.Find(id);

            if (MailingListCampaignRelation != null) {
                MailingListCampaignRelationRepo.Delete(MailingListCampaignRelation);
            }
        }
    }

}
