using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetRoleClaimService : IAspNetRoleClaimService
    {
        private readonly digiozPortalContext _context;

        public AspNetRoleClaimService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetRoleClaim Get(int id)
        {
            return _context.AspNetRoleClaims.Find(id);
        }

        public List<AspNetRoleClaim> GetAll()
        {
            return _context.AspNetRoleClaims.ToList();
        }

        public void Add(AspNetRoleClaim claim)
        {
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();
        }

        public void Update(AspNetRoleClaim claim)
        {
            _context.AspNetRoleClaims.Update(claim);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var claim = _context.AspNetRoleClaims.Find(id);
            if (claim != null)
            {
                _context.AspNetRoleClaims.Remove(claim);
                _context.SaveChanges();
            }
        }
    }
}
