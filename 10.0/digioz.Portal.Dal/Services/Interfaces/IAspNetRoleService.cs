using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetRoleService
    {
        AspNetRole Get(string id);
        List<AspNetRole> GetAll();
        void Add(AspNetRole role);
        void Update(AspNetRole role);
        void Delete(string id);
    }
}
