using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetUserRoleService : IAspNetUserRoleService
    {
        private readonly digiozPortalContext _context;

        public AspNetUserRoleService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetUserRole Get(string userId, string roleId)
        {
            return _context.AspNetUserRoles.Find(userId, roleId);
        }

        public List<AspNetUserRole> GetAll()
        {
            return _context.AspNetUserRoles.ToList();
        }

        public void Add(AspNetUserRole userRole)
        {
            _context.AspNetUserRoles.Add(userRole);
            _context.SaveChanges();
        }

        public void Update(AspNetUserRole userRole)
        {
            _context.AspNetUserRoles.Update(userRole);
            _context.SaveChanges();
        }

        public void Delete(string userId, string roleId)
        {
            var userRole = _context.AspNetUserRoles.Find(userId, roleId);
            if (userRole != null)
            {
                _context.AspNetUserRoles.Remove(userRole);
                _context.SaveChanges();
            }
        }
    }
}
