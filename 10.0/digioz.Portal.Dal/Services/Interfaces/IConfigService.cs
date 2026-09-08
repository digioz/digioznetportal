using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IConfigService
    {
        Config Get(string id);
        Config GetByKey(string configKey);
        List<Config> GetAll();
        void Add(Config config);
        void Update(Config config);
        void Delete(string id);
    }
}
