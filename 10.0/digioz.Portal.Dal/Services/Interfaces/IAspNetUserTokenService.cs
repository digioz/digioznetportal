using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserTokenService
    {
        AspNetUserToken Get(string userId, string loginProvider, string name);
        List<AspNetUserToken> GetAll();
        void Add(AspNetUserToken userToken);
        void Update(AspNetUserToken userToken);
        void Delete(string userId, string loginProvider, string name);
    }
}
