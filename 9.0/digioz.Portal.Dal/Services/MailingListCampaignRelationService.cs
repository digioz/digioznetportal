using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class MailingListCampaignRelationService : IMailingListCampaignRelationService
    {
        private readonly digiozPortalContext _context;

        public MailingListCampaignRelationService(digiozPortalContext context)
        {
            _context = context;
        }

        public MailingListCampaignRelation Get(string id)
        {
            return _context.MailingListCampaignRelations.Find(id);
        }

        public List<MailingListCampaignRelation> GetAll()
        {
            return _context.MailingListCampaignRelations.ToList();
        }

        public void Add(MailingListCampaignRelation relation)
        {
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();
        }

        public void Update(MailingListCampaignRelation relation)
        {
            _context.MailingListCampaignRelations.Update(relation);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var relation = _context.MailingListCampaignRelations.Find(id);
            if (relation != null)
            {
                _context.MailingListCampaignRelations.Remove(relation);
                _context.SaveChanges();
            }
        }
    }
}
