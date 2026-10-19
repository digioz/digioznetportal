using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class MailingListService : IMailingListService
    {
        private readonly digiozPortalContext _context;

        public MailingListService(digiozPortalContext context)
        {
            _context = context;
        }

        public MailingList Get(string id)
        {
            return _context.MailingLists.Find(id);
        }

        public List<MailingList> GetAll()
        {
            return _context.MailingLists.ToList();
        }

        public void Add(MailingList mailingList)
        {
            _context.MailingLists.Add(mailingList);
            _context.SaveChanges();
        }

        public void Update(MailingList mailingList)
        {
            _context.MailingLists.Update(mailingList);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var mailingList = _context.MailingLists.Find(id);
            if (mailingList != null)
            {
                _context.MailingLists.Remove(mailingList);
                _context.SaveChanges();
            }
        }
    }
}
