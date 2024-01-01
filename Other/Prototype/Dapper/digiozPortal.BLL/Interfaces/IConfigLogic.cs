using System.Collections.Generic;
using digiozPortal.BO;

namespace digiozPortal.BLL.Interfaces
{
    public interface IConfigLogic : ILogic<Config> 
    {
        Dictionary<string, string> GetConfig();
    }
}
