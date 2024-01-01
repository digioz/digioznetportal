using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Dal
{
    public class BaseRepo<T> : AbstractRepo<T> where T : class
    {
        public BaseRepo(IConfigHelper config):base(config){}
    }
}
