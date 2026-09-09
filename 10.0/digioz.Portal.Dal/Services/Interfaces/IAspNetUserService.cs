using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserService
    {
        AspNetUser Get(string id);
        string GetIdByEmail(string email);
        List<AspNetUser> GetAll();
        void Add(AspNetUser user);
        void Update(AspNetUser user);
        void Delete(string id);
    }
}
