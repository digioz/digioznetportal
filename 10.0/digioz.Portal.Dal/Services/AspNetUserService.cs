using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetUserService : IAspNetUserService
    {
        private readonly digiozPortalContext _context;

        public AspNetUserService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetUser Get(string id)
        {
            return _context.AspNetUsers.Find(id);
        }

        public List<AspNetUser> GetAll()
        {
            return _context.AspNetUsers.ToList();
        }

        public void Add(AspNetUser user)
        {
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();
        }

        public void Update(AspNetUser user)
        {
            _context.AspNetUsers.Update(user);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var user = _context.AspNetUsers.Find(id);
            if (user != null)
            {
                _context.AspNetUsers.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
