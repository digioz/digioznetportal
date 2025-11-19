using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ProfileService : IProfileService
    {
        private readonly digiozPortalContext _context;

        public ProfileService(digiozPortalContext context)
        {
            _context = context;
        }

        public Profile Get(string id)
        {
            return _context.Profiles.Find(id);
        }

        public Profile GetByUserId(string userId)
        {
            return _context.Profiles.FirstOrDefault(p => p.UserId == userId);
        }

        public Profile GetByEmail(string email)
        {
            return _context.Profiles.FirstOrDefault(p => p.Email == email);
        }

        public List<Profile> GetAll()
        {
            return _context.Profiles.ToList();
        }

        public void Add(Profile profile)
        {
            _context.Profiles.Add(profile);
            _context.SaveChanges();
        }

        public void Update(Profile profile)
        {
            _context.Profiles.Update(profile);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var profile = _context.Profiles.Find(id);
            if (profile != null)
            {
                _context.Profiles.Remove(profile);
                _context.SaveChanges();
            }
        }
    }
}
