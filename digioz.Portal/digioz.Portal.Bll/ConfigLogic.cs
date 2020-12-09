using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;

namespace digioz.Portal.Bll
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
