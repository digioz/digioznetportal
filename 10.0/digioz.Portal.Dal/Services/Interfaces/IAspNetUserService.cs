using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IAspNetUserService
    {
        AspNetUser Get(string id);
#nullable enable
        /// <summary>Returns the user id for the given email, or null when no user matches.</summary>
        string? GetIdByEmail(string? email);
#nullable restore
        List<AspNetUser> GetAll();
        void Add(AspNetUser user);
        void Update(AspNetUser user);
        void Delete(string id);
    }
}
