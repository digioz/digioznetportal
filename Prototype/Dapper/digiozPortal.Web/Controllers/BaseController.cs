using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace digiozPortal.Web.Controllers
{
    public class BaseController : Controller
    {
        public string ConnectionString;

        public override void OnActionExecuting(ActionExecutingContext filterContext) {


        }
    }
}
