using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace digioz.Portal.Web.Areas.Admin.Pages.Config
{
    public class EditModel : PageModel
    {
        private readonly IConfigService _service;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        public EditModel(IConfigService service, IConfiguration configuration, IMemoryCache cache)
        {
            _service = service;
            _configuration = configuration;
            _cache = cache;
        }

        [BindProperty] public digioz.Portal.Bo.Config? Item { get; set; }
        [BindProperty] public string? OriginalEncryptedValue { get; set; }
        [BindProperty] public bool WasEncryptedOnLoad { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Config/Index", new { area = "Admin" });

            if (Item.IsEncrypted)
            {
                OriginalEncryptedValue = Item.ConfigValue;
                WasEncryptedOnLoad = true;
                Item.ConfigValue = string.Empty; // hide encrypted value
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/Config/Index", new { area = "Admin" });

            if (WasEncryptedOnLoad)
            {
                if (string.IsNullOrEmpty(Item.ConfigValue))
                {
                    // Keep the original encrypted value and preserve encryption flag
                    Item.ConfigValue = OriginalEncryptedValue;
                    Item.IsEncrypted = true;
                }
                else
                {
                    // New value provided; encrypt if the user wants it encrypted, else store as plain
                    if (Item.IsEncrypted)
                    {
                        var key = _configuration["SiteEncryptionKey"];
                        if (string.IsNullOrWhiteSpace(key) || !IsValidAesKeyLength(key))
                        {
                            ModelState.AddModelError("Item.IsEncrypted", "Encryption key is not configured or has invalid length. Set 'SiteEncryptionKey' in appsettings.json to16,24, or32 characters.");
                            return Page();
                        }
                        var enc = new EncryptString();
                        Item.ConfigValue = enc.Encrypt(key, Item.ConfigValue);
                    }
                    // else: leave as plain text
                }
            }
            else
            {
                // Not originally encrypted; if user marked as encrypted, encrypt the provided value
                if (Item.IsEncrypted && !string.IsNullOrEmpty(Item.ConfigValue))
                {
                    var key = _configuration["SiteEncryptionKey"];
                    if (string.IsNullOrWhiteSpace(key) || !IsValidAesKeyLength(key))
                    {
                        ModelState.AddModelError("Item.IsEncrypted", "Encryption key is not configured or has invalid length. Set 'SiteEncryptionKey' in appsettings.json to16,24, or32 characters.");
                        return Page();
                    }
                    var enc = new EncryptString();
                    Item.ConfigValue = enc.Encrypt(key, Item.ConfigValue);
                }
            }

            _service.Update(Item);
            CacheKeys.InvalidateConfig(_cache);
            return RedirectToPage("/Config/Index", new { area = "Admin" });
        }

        private static bool IsValidAesKeyLength(string key)
        {
            var len = System.Text.Encoding.UTF8.GetByteCount(key);
            return len ==16 || len ==24 || len ==32;
        }
    }
}
