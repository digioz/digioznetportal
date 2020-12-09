using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Dal.Interfaces;

namespace digioz.Portal.Bll
{
    public class BaseLogic<T> : AbstractLogic<T>
    {
        public BaseLogic(IRepo<T> repo):base(repo) { }
    }
}
