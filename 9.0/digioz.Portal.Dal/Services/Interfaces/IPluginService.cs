using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPluginService
    {
        Plugin Get(int id);
        Plugin GetByName(string name);
        List<Plugin> GetAll();
        void Add(Plugin plugin);
        void Update(Plugin plugin);
        void Delete(int id);
    }
}
