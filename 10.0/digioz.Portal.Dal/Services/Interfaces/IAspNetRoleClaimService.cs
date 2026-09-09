using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetRoleClaimService
    {
        AspNetRoleClaim Get(int id);
        List<AspNetRoleClaim> GetAll();
        void Add(AspNetRoleClaim claim);
        void Update(AspNetRoleClaim claim);
        void Delete(int id);
    }
}
