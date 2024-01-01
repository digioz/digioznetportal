using digiozPortal.BLL.Interfaces;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL
{
    public class BaseLogic<T> : AbstractLogic<T>
    {
        public BaseLogic(IRepo<T> repo):base(repo) { }
    }
}
