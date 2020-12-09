using digiozPortal.DAL.Interfaces;
using digiozPortal.Utilities;

namespace digiozPortal.DAL
{
    public class BaseRepo<T> : AbstractRepo<T> where T : class
    {
        public BaseRepo(IConfigHelper config):base(config){}
    }
}
