using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class MailingListSubscriberService : IMailingListSubscriberService
    {
        private readonly digiozPortalContext _context;

        public MailingListSubscriberService(digiozPortalContext context)
        {
            _context = context;
        }

        public MailingListSubscriber Get(string id)
        {
            return _context.MailingListSubscribers.Find(id);
        }

        public List<MailingListSubscriber> GetAll()
        {
            return _context.MailingListSubscribers.ToList();
        }

        public void Add(MailingListSubscriber subscriber)
        {
            _context.MailingListSubscribers.Add(subscriber);
            _context.SaveChanges();
        }

        public void Update(MailingListSubscriber subscriber)
        {
            _context.MailingListSubscribers.Update(subscriber);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var subscriber = _context.MailingListSubscribers.Find(id);
            if (subscriber != null)
            {
                _context.MailingListSubscribers.Remove(subscriber);
                _context.SaveChanges();
            }
        }
    }
}
