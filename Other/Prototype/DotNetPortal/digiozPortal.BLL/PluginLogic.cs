using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PluginLogic
    {
        public Plugin Get(int id) {
            var PluginRepo = new PluginRepo();
            Plugin Plugin = PluginRepo.Get(id);

            return Plugin;
        }

        public List<Plugin> GetAll() {
            var PluginRepo = new PluginRepo();
            var Plugins = PluginRepo.GetAll();

            return Plugins;
        }

        public void Add(Plugin Plugin) {
            var PluginRepo = new PluginRepo();
            PluginRepo.Add(Plugin);
        }

        public void Edit(Plugin Plugin) {
            var PluginRepo = new PluginRepo();

            PluginRepo.Edit(Plugin);
        }

        public void Delete(int id) {
            var PluginRepo = new PluginRepo();
            var Plugin = PluginRepo.Get(id); // Db.Plugins.Find(id);

            if (Plugin != null) {
                PluginRepo.Delete(Plugin);
            }
        }
    }

}
