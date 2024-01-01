using System.Collections.Generic;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL
{
    public class ConfigLogic : AbstractLogic<Config>, IConfigLogic
    {
        public ConfigLogic(IRepo<Config> repo) : base(repo) { }
        public Dictionary<string, string> GetConfig() {
            var models = GetAll();
            var configs = new Dictionary<string, string>();

            foreach (var model in models) {
                configs.Add(model.ConfigKey, model.ConfigValue);
            }

            return configs;
        }

    }
}
