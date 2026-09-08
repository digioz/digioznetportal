using System;
using System.Linq;
using System.Text;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Visitor
{
    [Authorize(Roles = "Administrator")]
    public class ExportModel : PageModel
    {
        private readonly IVisitorInfoService _service;
        public ExportModel(IVisitorInfoService service) { _service = service; }

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

            // Use service layer method for consistent date filtering
            var records = _service.GetByDateRange(StartDate, EndDate);

            if (records == null || !records.Any())
            {
                ModelState.AddModelError(string.Empty, "No records found for the selected date criteria.");
                return Page();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Timestamp,IpAddress,Host,HostName,Platform,Referrer,Href,UserAgent,UserLanguage,SessionId");

            foreach (var r in records)
            {
                var id = r.Id.ToString();
                var timestamp = (r.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss")) ?? string.Empty;
                sb.AppendLine($"{id},{CsvHelper.Escape(timestamp)},{CsvHelper.Escape(r.IpAddress)},{CsvHelper.Escape(r.Host)},{CsvHelper.Escape(r.HostName)},{CsvHelper.Escape(r.Platform)},{CsvHelper.Escape(r.Referrer)},{CsvHelper.Escape(r.Href)},{CsvHelper.Escape(r.UserAgent)},{CsvHelper.Escape(r.UserLanguage)},{CsvHelper.Escape(r.SessionId)}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"visitor_export_{(StartDate.HasValue ? StartDate.Value: DateTime.MinValue):yyyyMMdd}_{(EndDate.HasValue ? EndDate.Value: DateTime.Now):yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}
