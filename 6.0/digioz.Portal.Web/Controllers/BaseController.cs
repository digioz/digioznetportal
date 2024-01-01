using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace digioz.Portal.Web.Controllers
{
    public class BaseController : Controller
    {
        public string ConnectionString;

        public override void OnActionExecuting(ActionExecutingContext filterContext) {


        }
    }
}
