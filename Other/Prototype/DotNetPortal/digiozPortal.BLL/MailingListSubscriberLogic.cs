using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MailingListSubscriberLogic
    {
        public MailingListSubscriber Get(string id) {
            var MailingListSubscriberRepo = new MailingListSubscriberRepo();
            MailingListSubscriber MailingListSubscriber = MailingListSubscriberRepo.Get(id);

            return MailingListSubscriber;
        }

        public List<MailingListSubscriber> GetAll() {
            var MailingListSubscriberRepo = new MailingListSubscriberRepo();
            var MailingListSubscribers = MailingListSubscriberRepo.GetAll();

            return MailingListSubscribers;
        }

        public void Add(MailingListSubscriber MailingListSubscriber) {
            var MailingListSubscriberRepo = new MailingListSubscriberRepo();
            MailingListSubscriberRepo.Add(MailingListSubscriber);
        }

        public void Edit(MailingListSubscriber MailingListSubscriber) {
            var MailingListSubscriberRepo = new MailingListSubscriberRepo();

            MailingListSubscriberRepo.Edit(MailingListSubscriber);
        }

        public void Delete(string id) {
            var MailingListSubscriberRepo = new MailingListSubscriberRepo();
            var MailingListSubscriber = MailingListSubscriberRepo.Get(id); // Db.MailingListSubscribers.Find(id);

            if (MailingListSubscriber != null) {
                MailingListSubscriberRepo.Delete(MailingListSubscriber);
            }
        }
    }

}
