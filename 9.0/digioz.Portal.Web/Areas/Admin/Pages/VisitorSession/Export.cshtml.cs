using System;
using System.Linq;
using System.Text;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.VisitorSession
{
    [Authorize(Roles = "Administrator")]
    public class ExportModel : PageModel
    {
        private readonly IVisitorSessionService _service;
        public ExportModel(IVisitorSessionService service) { _service = service; }

        [BindProperty]
        public DateTime? StartDate { get; set; }
        [BindProperty]
        public DateTime? EndDate { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (StartDate == null && EndDate == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a date (either an end date to export all records before that date, or both start and end dates for a range).");
                return Page();
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            var records = _service.GetByDateRange(StartDate, EndDate)
                .OrderBy(r => r.DateModified).ThenBy(r => r.Id).ToList();

            if (!records.Any())
            {
                ModelState.AddModelError(string.Empty, "No records found for the selected date criteria.");
                return Page();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,DateModified,IpAddress,SessionId,Username,PageUrl");

            foreach (var r in records)
            {
                var id = r.Id.ToString();
                var timestamp = r.DateModified.ToString("yyyy-MM-dd HH:mm:ss");
                sb.AppendLine($"{id},{CsvHelper.Escape(timestamp)},{CsvHelper.Escape(r.IpAddress)},{CsvHelper.Escape(r.SessionId)},{CsvHelper.Escape(r.Username)},{CsvHelper.Escape(r.PageUrl)}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"visitor_session_export_{(StartDate.HasValue ? StartDate.Value: DateTime.MinValue):yyyyMMdd}_{(EndDate.HasValue ? EndDate.Value: DateTime.Now):yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}
