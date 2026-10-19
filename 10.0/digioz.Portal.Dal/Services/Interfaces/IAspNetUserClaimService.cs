using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserClaimService
    {
        AspNetUserClaim Get(int id);
        List<AspNetUserClaim> GetAll();
        void Add(AspNetUserClaim claim);
        void Update(AspNetUserClaim claim);
        void Delete(int id);
    }
}
