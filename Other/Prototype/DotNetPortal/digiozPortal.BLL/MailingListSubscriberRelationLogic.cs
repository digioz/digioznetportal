using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MailingListSubscriberRelationLogic
    {
        public MailingListSubscriberRelation Get(string id) {
            var MailingListSubscriberRelationRepo = new MailingListSubscriberRelationRepo();
            MailingListSubscriberRelation MailingListSubscriberRelation = MailingListSubscriberRelationRepo.Get(id);

            return MailingListSubscriberRelation;
        }

        public List<MailingListSubscriberRelation> GetAll() {
            var MailingListSubscriberRelationRepo = new MailingListSubscriberRelationRepo();
            var MailingListSubscriberRelations = MailingListSubscriberRelationRepo.GetAll();

            return MailingListSubscriberRelations;
        }

        public void Add(MailingListSubscriberRelation MailingListSubscriberRelation) {
            var MailingListSubscriberRelationRepo = new MailingListSubscriberRelationRepo();
            MailingListSubscriberRelationRepo.Add(MailingListSubscriberRelation);
        }

        public void Edit(MailingListSubscriberRelation MailingListSubscriberRelation) {
            var MailingListSubscriberRelationRepo = new MailingListSubscriberRelationRepo();

            MailingListSubscriberRelationRepo.Edit(MailingListSubscriberRelation);
        }

        public void Delete(string id) {
            var MailingListSubscriberRelationRepo = new MailingListSubscriberRelationRepo();
            var MailingListSubscriberRelation = MailingListSubscriberRelationRepo.Get(id); // Db.MailingListSubscriberRelations.Find(id);

            if (MailingListSubscriberRelation != null) {
                MailingListSubscriberRelationRepo.Delete(MailingListSubscriberRelation);
            }
        }
    }

}
