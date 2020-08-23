using System.Linq;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using Microsoft.AspNetCore.Mvc;

namespace digiozPortal.Web.Areas.Admin.ViewComponents
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
