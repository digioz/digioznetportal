using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IProfileService
    {
        Profile Get(int id);
        Profile GetByUserId(string userId);
        Profile GetByEmail(string email);
        Profile GetByDisplayName(string displayName);
        List<Profile> GetAll();
        List<Profile> GetByUserIds(List<string> userIds);
        void Add(Profile profile);
        void Update(Profile profile);
        void Delete(int id);
        void IncrementViews(int id);
    }
}
