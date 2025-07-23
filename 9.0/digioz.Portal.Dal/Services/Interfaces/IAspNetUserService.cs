using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserService
    {
        AspNetUser Get(string id);
        List<AspNetUser> GetAll();
        void Add(AspNetUser user);
        void Update(AspNetUser user);
        void Delete(string id);
    }
}
