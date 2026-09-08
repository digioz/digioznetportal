using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class AspNetUserClaimService : IAspNetUserClaimService
    {
        private readonly digiozPortalContext _context;

        public AspNetUserClaimService(digiozPortalContext context)
        {
            _context = context;
        }

        public AspNetUserClaim Get(int id)
        {
            return _context.AspNetUserClaims.Find(id);
        }

        public List<AspNetUserClaim> GetAll()
        {
            return _context.AspNetUserClaims.ToList();
        }

        public void Add(AspNetUserClaim claim)
        {
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();
        }

        public void Update(AspNetUserClaim claim)
        {
            _context.AspNetUserClaims.Update(claim);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var claim = _context.AspNetUserClaims.Find(id);
            if (claim != null)
            {
                _context.AspNetUserClaims.Remove(claim);
                _context.SaveChanges();
            }
        }
    }
}
