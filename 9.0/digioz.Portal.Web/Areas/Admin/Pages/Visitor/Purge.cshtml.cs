using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Visitor
{
    [Authorize(Roles = "Administrator")]
    public class PurgeModel : PageModel
    {
        private readonly IVisitorInfoService _service;
        public PurgeModel(IVisitorInfoService service) { _service = service; }

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

            // Use service method to retrieve only matching records
            var toDelete = _service.GetByDateRange(StartDate, EndDate);

            if (toDelete == null || !toDelete.Any())
            {
                ModelState.AddModelError(string.Empty, "No records found for the selected date criteria.");
                return Page();
            }

            foreach (var r in toDelete)
            {
                _service.Delete(r.Id);
            }

            return RedirectToPage("/Visitor/Index", new { area = "Admin" });
        }
    }
}
