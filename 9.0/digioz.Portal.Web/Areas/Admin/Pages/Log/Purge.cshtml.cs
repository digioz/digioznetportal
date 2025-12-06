using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Log
{
    [Authorize(Roles = "Administrator")]
    public class PurgeModel : PageModel
    {
        private readonly ILogService _service;
        public PurgeModel(ILogService service) { _service = service; }

        [BindProperty]
        public DateTime? StartDate { get; set; }
        [BindProperty]
        public DateTime? EndDate { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (StartDate == null && EndDate == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a date (either an end date to purge all records before that date, or both start and end dates for a range).");
                return Page();
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            var toDelete = _service.GetByDateRange(StartDate, EndDate);

            if (toDelete == null || !toDelete.Any())
            {
                ModelState.AddModelError(string.Empty, "No records found for the selected date criteria.");
                return Page();
            }

            // Bulk delete for better performance
            var ids = toDelete.Select(r => r.Id).ToList();
            var count = _service.DeleteRange(ids);

            TempData["SuccessMessage"] = $"Successfully purged {count} log record(s).";
            return RedirectToPage("/Log/Index", new { area = "Admin" });
        }
    }
}
