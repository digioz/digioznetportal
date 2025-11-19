using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IProfileService
    {
        Profile Get(string id);
        Profile GetByUserId(string userId);
        Profile GetByEmail(string email);
        List<Profile> GetAll();
        void Add(Profile profile);
        void Update(Profile profile);
        void Delete(string id);
    }
}
