using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IMailingListService
    {
        MailingList Get(string id);
        List<MailingList> GetAll();
        void Add(MailingList mailingList);
        void Update(MailingList mailingList);
        void Delete(string id);
    }
}
