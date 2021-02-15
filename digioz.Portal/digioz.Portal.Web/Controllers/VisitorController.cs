using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Controllers
{
	public class VisitorController : Controller
	{
        private readonly ILogic<VisitorInfo> _visitorInfoLogic;
        private readonly ILogic<VisitorSession> _visitorSessionLogic;

        public VisitorController(
            ILogic<VisitorInfo> visitorInfoLogic,
            ILogic<VisitorSession> visitorSessionLogic
        )
        {
            _visitorInfoLogic = visitorInfoLogic;
            _visitorSessionLogic = visitorSessionLogic;
        }

        // GET: Visitor
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public void Process(string language, string appPlatform, string userAgent, string javaEnabled, string browserVersion, string browserType,
                            string screenWidth, string screenHeight, string host, string hostName, string referrer, string href, string engineName, 
                            string sessionId, string operatingSystem)
        {
            var context = HttpContext;
            var ipAddress = IpAddressHelper.GetUserIPAddress(context);

            if (browserVersion == "undefined")
            {
                browserVersion = null;
            }

            if (screenWidth == "undefined")
            {
                screenWidth = null;
            }

            if (screenHeight == "undefined")
            {
                screenHeight = null;
            }

            if (browserType == "Opera-Edge")
            {
                if (userAgent.Contains("OPR"))
                {
                    browserType = "Opera";
                }
                else
                {
                    browserType = "Edge";
                }
            }

            try
            {
                // Log Visitor Info
                VisitorInfo visitorInfo = new VisitorInfo
                {
                    UserLanguage = language,
                    Platform = appPlatform,
                    UserAgent = userAgent,
                    JavaEnabled = Convert.ToBoolean(javaEnabled),
                    Browser = browserType,
                    BrowserVersion = browserVersion,
                    ScreenWidth = Convert.ToInt32(screenWidth),
                    ScreenHeight = Convert.ToInt32(screenHeight),
                    Host = host,
                    HostName = hostName,
                    Referrer = referrer,
                    Href = href,
                    BrowserEngineName = engineName,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.Now,
                    SessionId = sessionId,
                    OperatingSystem = operatingSystem
                };

                
                _visitorInfoLogic.Add(visitorInfo);

                // Log Visitor Session
                var userName = User.Identity.Name;
                Helpers.Utility.WriteVisitorSession(_visitorSessionLogic, sessionId, href, userName, ipAddress);
            }
            catch (Exception ex)
            {
                // Ignore
            }
        }

    }
}
