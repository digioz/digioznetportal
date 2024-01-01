using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ModuleLogic
    {
        public Module Get(int id) {
            var ModuleRepo = new ModuleRepo();
            Module Module = ModuleRepo.Get(id);

            return Module;
        }

        public List<Module> GetAll() {
            var ModuleRepo = new ModuleRepo();
            var Modules = ModuleRepo.GetAll();

            return Modules;
        }

        public void Add(Module Module) {
            var ModuleRepo = new ModuleRepo();
            ModuleRepo.Add(Module);
        }

        public void Edit(Module Module) {
            var ModuleRepo = new ModuleRepo();

            ModuleRepo.Edit(Module);
        }

        public void Delete(int id) {
            var ModuleRepo = new ModuleRepo();
            var Module = ModuleRepo.Get(id); // Db.Modules.Find(id);

            if (Module != null) {
                ModuleRepo.Delete(Module);
            }
        }
    }

}
