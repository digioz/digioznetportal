using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserRoleService
    {
        AspNetUserRole Get(string userId, string roleId);
        List<AspNetUserRole> GetAll();
        void Add(AspNetUserRole userRole);
        void Update(AspNetUserRole userRole);
        void Delete(string userId, string roleId);
    }
}
