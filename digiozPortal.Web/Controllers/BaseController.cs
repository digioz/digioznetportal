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
            //ConnectionString = _configuration.GetConnectionString("DefaultConnection");

            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json")
                .Build();
            ConnectionString = configuration.GetConnectionString("DefaultConnection");


        }
    }
}
