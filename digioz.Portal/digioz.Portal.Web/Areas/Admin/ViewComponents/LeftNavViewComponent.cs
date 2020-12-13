using System.Linq;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Areas.Admin.ViewComponents
{
    public class LeftNavViewComponent : ViewComponent
    {
        ILogic<Menu> _menuLogic;

        public LeftNavViewComponent(ILogic<Menu> menuLogic) {
            _menuLogic = menuLogic;
        }

        public IViewComponentResult Invoke(int referenceId, string referenceType) {
            return View();
        }
    }
}
