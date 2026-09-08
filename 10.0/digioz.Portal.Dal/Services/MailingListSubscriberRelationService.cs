using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class MailingListSubscriberRelationService : IMailingListSubscriberRelationService
    {
        private readonly digiozPortalContext _context;

        public MailingListSubscriberRelationService(digiozPortalContext context)
        {
            _context = context;
        }

        public MailingListSubscriberRelation Get(string id)
        {
            return _context.MailingListSubscriberRelations.Find(id);
        }

        public List<MailingListSubscriberRelation> GetAll()
        {
            return _context.MailingListSubscriberRelations.ToList();
        }

        public List<MailingListSubscriberRelation> GetByMailingListId(string mailingListId)
        {
            return _context.MailingListSubscriberRelations
                .Where(r => r.MailingListId == mailingListId)
                .ToList();
        }

        public List<MailingListSubscriberRelation> GetBySubscriberId(string subscriberId)
        {
            return _context.MailingListSubscriberRelations
                .Where(r => r.MailingListSubscriberId == subscriberId)
                .ToList();
        }

        public MailingListSubscriberRelation GetByMailingListAndSubscriber(string mailingListId, string subscriberId)
        {
            return _context.MailingListSubscriberRelations
                .FirstOrDefault(r => r.MailingListId == mailingListId && r.MailingListSubscriberId == subscriberId);
        }

        public void Add(MailingListSubscriberRelation relation)
        {
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();
        }

        public void Update(MailingListSubscriberRelation relation)
        {
            _context.MailingListSubscriberRelations.Update(relation);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var relation = _context.MailingListSubscriberRelations.Find(id);
            if (relation != null)
            {
                _context.MailingListSubscriberRelations.Remove(relation);
                _context.SaveChanges();
            }
        }

        public void DeleteByMailingListAndSubscriber(string mailingListId, string subscriberId)
        {
            var relation = GetByMailingListAndSubscriber(mailingListId, subscriberId);
            if (relation != null)
            {
                _context.MailingListSubscriberRelations.Remove(relation);
                _context.SaveChanges();
            }
        }
    }
}
