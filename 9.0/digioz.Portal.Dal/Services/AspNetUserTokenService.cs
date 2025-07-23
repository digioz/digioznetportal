using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetUserTokenService : IAspNetUserTokenService
    {
        private readonly digiozPortalContext _context;

        public AspNetUserTokenService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetUserToken Get(string userId, string loginProvider, string name)
        {
            return _context.AspNetUserTokens.Find(userId, loginProvider, name);
        }

        public List<AspNetUserToken> GetAll()
        {
            return _context.AspNetUserTokens.ToList();
        }

        public void Add(AspNetUserToken userToken)
        {
            _context.AspNetUserTokens.Add(userToken);
            _context.SaveChanges();
        }

        public void Update(AspNetUserToken userToken)
        {
            _context.AspNetUserTokens.Update(userToken);
            _context.SaveChanges();
        }

        public void Delete(string userId, string loginProvider, string name)
        {
            var userToken = _context.AspNetUserTokens.Find(userId, loginProvider, name);
            if (userToken != null)
            {
                _context.AspNetUserTokens.Remove(userToken);
                _context.SaveChanges();
            }
        }
    }
}
