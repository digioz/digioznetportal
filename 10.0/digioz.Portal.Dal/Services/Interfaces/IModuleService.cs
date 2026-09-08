using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IModuleService
    {
        Module Get(int id);
        List<Module> GetAll();
        void Add(Module module);
        void Update(Module module);
        void Delete(int id);
    }
}
