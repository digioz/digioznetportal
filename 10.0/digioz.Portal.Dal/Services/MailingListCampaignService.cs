using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class MailingListCampaignService : IMailingListCampaignService
    {
        private readonly digiozPortalContext _context;

        public MailingListCampaignService(digiozPortalContext context)
        {
            _context = context;
        }

        public MailingListCampaign Get(string id)
        {
            return _context.MailingListCampaigns.Find(id);
        }

        public List<MailingListCampaign> GetAll()
        {
            return _context.MailingListCampaigns.ToList();
        }

        public void Add(MailingListCampaign campaign)
        {
            _context.MailingListCampaigns.Add(campaign);
            _context.SaveChanges();
        }

        public void Update(MailingListCampaign campaign)
        {
            _context.MailingListCampaigns.Update(campaign);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var campaign = _context.MailingListCampaigns.Find(id);
            if (campaign != null)
            {
                _context.MailingListCampaigns.Remove(campaign);
                _context.SaveChanges();
            }
        }
    }
}
