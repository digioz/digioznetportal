using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class MailingListLogic
    {
        public MailingList Get(string id) {
            var MailingListRepo = new MailingListRepo();
            MailingList MailingList = MailingListRepo.Get(id);

            return MailingList;
        }

        public List<MailingList> GetAll() {
            var MailingListRepo = new MailingListRepo();
            var MailingLists = MailingListRepo.GetAll();

            return MailingLists;
        }

        public void Add(MailingList MailingList) {
            var MailingListRepo = new MailingListRepo();
            MailingListRepo.Add(MailingList);
        }

        public void Edit(MailingList MailingList) {
            var MailingListRepo = new MailingListRepo();

            MailingListRepo.Edit(MailingList);
        }

        public void Delete(string id) {
            var MailingListRepo = new MailingListRepo();
            var MailingList = MailingListRepo.Get(id); // Db.MailingLists.Find(id);

            if (MailingList != null) {
                MailingListRepo.Delete(MailingList);
            }
        }
    }

}
