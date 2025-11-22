using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IThemeService
    {
        Theme Get(int id);
        Theme GetDefault();
        List<Theme> GetAll();
        void Add(Theme theme);
        void Update(Theme theme);
        void Delete(int id);
    }
}
