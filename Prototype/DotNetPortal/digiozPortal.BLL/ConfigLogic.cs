using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ConfigLogic
    {
        public Config Get(string id) {
            var ConfigRepo = new ConfigRepo();
            Config Config = ConfigRepo.Get(id);

            return Config;
        }

        public List<Config> GetAll() {
            var ConfigRepo = new ConfigRepo();
            var Configs = ConfigRepo.GetAll();

            return Configs;
        }

        public Dictionary<string, string> GetConfig() {
            var models = GetAll();
            var configs = new Dictionary<string, string>();
            
            foreach(var model in models) {
                configs.Add(model.ConfigKey, model.ConfigValue);
            }

            return configs;
        }

        public void Add(Config Config) {
            var ConfigRepo = new ConfigRepo();
            ConfigRepo.Add(Config);
        }

        public void Edit(Config Config) {
            var ConfigRepo = new ConfigRepo();

            ConfigRepo.Edit(Config);
        }

        public void Delete(string id) {
            var ConfigRepo = new ConfigRepo();
            var Config = ConfigRepo.Get(id); // Db.Configs.Find(id);

            if (Config != null) {
                ConfigRepo.Delete(Config);
            }
        }
    }

}
