using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Chat
{
    public class PurgeModel : PageModel
    {
        private readonly IChatService _service;
        public PurgeModel(IChatService service) { _service = service; }

        [BindProperty]
        public DateTime? StartDate { get; set; }
        [BindProperty]
        public DateTime? EndDate { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (StartDate == null || EndDate == null || EndDate < StartDate)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            // Normalize to inclusive date range (end of day)
            var start = StartDate.Value.Date;
            var end = EndDate.Value.Date.AddDays(1).AddTicks(-1);

            var all = _service.GetAll();
            var toDelete = all.Where(c => (c.Timestamp ?? DateTime.MinValue) >= start && (c.Timestamp ?? DateTime.MinValue) <= end).ToList();
            foreach (var chat in toDelete)
            {
                _service.Delete(chat.Id);
            }

            return RedirectToPage("/Chat/Index", new { area = "Admin" });
        }
    }
}
