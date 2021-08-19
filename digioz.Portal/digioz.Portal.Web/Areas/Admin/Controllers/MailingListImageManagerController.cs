using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MailingListImageManagerController : Controller
    {
        private readonly ILogger<MailingListImageManagerController> _logger;
        private readonly ILogic<Log> _logLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<MailingList> _mailingListLogic;
        private readonly ILogic<MailingListCampaign> _mailingListCampaignLogic;
        private readonly ILogic<MailingListSubscriber> _mailingListSubscriberLogic;
        private readonly ILogic<MailingListSubscriberRelation> _mailingListSubscriberRelationLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailingListImageManagerController(
            ILogger<MailingListImageManagerController> logger,
            ILogic<Log> logLogic,
            IConfigLogic configLogic,
            ILogic<MailingList> mailingListLogic,
            ILogic<MailingListCampaign> mailingListCampaignLogic,
            ILogic<MailingListSubscriber> mailingListSubscriberLogic,
            ILogic<MailingListSubscriberRelation> mailingListSubscriberRelationLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _logger = logger;
            _logLogic = logLogic;
            _configLogic = configLogic;
            _mailingListLogic = mailingListLogic;
            _mailingListCampaignLogic = mailingListCampaignLogic;
            _mailingListSubscriberLogic = mailingListSubscriberLogic;
            _mailingListSubscriberRelationLogic = mailingListSubscriberRelationLogic;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetImageFolderPath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(IFormFile file, string path, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        // GET: Admin/MailingListImageManager
        public ActionResult Index()
        {
            List<MailingListImageViewModel> imageList = new List<MailingListImageViewModel>();

            var imgFolder = GetImageFolderPath();
            var searchFolder = Path.Combine(imgFolder, "Emails", "uploads", "Full");
            var filters = new String[] {"jpg", "jpeg", "png", "gif", "tiff", "bmp"};
            var files = Utility.GetFilesFrom(searchFolder, filters, false);

            foreach (var item in files)
            {
                MailingListImageViewModel file = new MailingListImageViewModel();
                file.Name = item;

                imageList.Add(file);
            }

            return View(imageList);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null)
            {
                return RedirectToAction("Create");
            }

            if (file != null && Utility.IsFileAnImage(file.FileName))
            {
                Guid guidName = Guid.NewGuid();
                var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                var imgFolder = GetImageFolderPath();
                var pathFull = Path.Combine(imgFolder, "Emails", "uploads", "Full", fileName);
                var pathThumb = Path.Combine(imgFolder, "Emails", "uploads", "Thumb", fileName);

                // Save Original Image
                await CropImageAndSave(file, pathFull, 600, 150);

                // Save Thumbnail Image
                await CropImageAndSave(file, pathThumb, 120, 120);

                return RedirectToAction(("Index"));
            }

            return View();
        }

        [Route("/admin/mailinglistimagemanager/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id != null)
            {
                var imgFolder = GetImageFolderPath();
                var pathFull = Path.Combine(imgFolder, "Emails", "uploads", "Full", id);
                var pathThumb = Path.Combine(imgFolder, "Emails", "uploads", "Thumb", id);

                try
                {
                    if (System.IO.File.Exists(pathFull))
                    {
                        System.IO.File.Delete(pathFull);
                    }
                    if (System.IO.File.Exists(pathThumb))
                    {
                        System.IO.File.Delete(pathThumb);
                    }
                }
                catch (Exception ex)
                {
                    string logEntry = "Error: " + ex.Message + Environment.NewLine + "Stack Trace: " + ex.StackTrace;
                    Utility.AddLogEntry(logEntry, _logLogic);
                }
            }

            return RedirectToAction(("Index"));
        }
    }
}