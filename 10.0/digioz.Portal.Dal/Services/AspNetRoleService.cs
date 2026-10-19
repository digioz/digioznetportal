using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetRoleService : IAspNetRoleService
    {
        private readonly digiozPortalContext _context;

        public AspNetRoleService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetRole Get(string id)
        {
            return _context.AspNetRoles.Find(id);
        }

        public List<AspNetRole> GetAll()
        {
            return _context.AspNetRoles.ToList();
        }

        public void Add(AspNetRole role)
        {
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();
        }

        public void Update(AspNetRole role)
        {
            _context.AspNetRoles.Update(role);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var role = _context.AspNetRoles.Find(id);
            if (role != null)
            {
                _context.AspNetRoles.Remove(role);
                _context.SaveChanges();
            }
        }
    }
}
