using System;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace digioz.Portal.Web.Areas.Admin.Pages.Config
{
    public class AddModel : PageModel
    {
        private readonly IConfigService _service;
        private readonly IConfiguration _configuration;
        public AddModel(IConfigService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [BindProperty] public Bo.Config Item { get; set; } = new Bo.Config();

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            if (Item.IsEncrypted)
            {
                var key = _configuration["SiteEncryptionKey"];
                if (string.IsNullOrWhiteSpace(key) || !IsValidAesKeyLength(key))
                {
                    ModelState.AddModelError("Item.IsEncrypted", "Encryption key is not configured or has invalid length. Set 'SiteEncryptionKey' in appsettings.json to16,24, or32 characters.");
                    return Page();
                }
                if (!string.IsNullOrEmpty(Item.ConfigValue))
                {
                    var enc = new EncryptString();
                    Item.ConfigValue = enc.Encrypt(key, Item.ConfigValue);
                }
            }

            // Ensure Id
            Item.Id ??= Guid.NewGuid().ToString();

            _service.Add(Item);
            return RedirectToPage("/Config/Index", new { area = "Admin" });
        }

        private static bool IsValidAesKeyLength(string key)
        {
            var len = System.Text.Encoding.UTF8.GetByteCount(key);
            return len ==16 || len ==24 || len ==32;
        }
    }
}
