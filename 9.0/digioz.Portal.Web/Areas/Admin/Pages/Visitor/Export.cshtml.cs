using System;
using System.Linq;
using System.Text;
using digioz.Portal.Dal.Services.Interfaces;
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
                ModelState.AddModelError(string.Empty, "Please select a date (either an end date to export all records before that date, or both start and end dates for a range).);");
                return Page();
            }

            DateTime? start = StartDate?.Date;
            DateTime? end = EndDate?.Date;

            if (start.HasValue && end.HasValue && end < start)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            // Normalize end to inclusive end of day when present
            if (end.HasValue)
            {
                end = end.Value.Date.AddDays(1).AddTicks(-1);
            }

            var records = _service.GetAll();

            var filtered = records.Where(r => (r.Timestamp ?? DateTime.MinValue) != DateTime.MinValue);

            if (start.HasValue && end.HasValue)
            {
                var s = start.Value;
                var e = end.Value;
                filtered = filtered.Where(r => (r.Timestamp ?? DateTime.MinValue) >= s && (r.Timestamp ?? DateTime.MinValue) <= e);
            }
            else if (!start.HasValue && end.HasValue)
            {
                var e = end.Value;
                filtered = filtered.Where(r => (r.Timestamp ?? DateTime.MinValue) <= e);
            }

            var list = filtered.OrderBy(r => r.Timestamp ?? DateTime.MinValue).ThenBy(r => r.Id).ToList();

            if (!list.Any())
            {
                ModelState.AddModelError(string.Empty, "No records found for the selected date criteria.");
                return Page();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Timestamp,IpAddress,Host,HostName,Platform,Referrer,Href,UserAgent,UserLanguage,SessionId");

            foreach (var r in list)
            {
                var id = r.Id.ToString();
                var timestamp = (r.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss")) ?? string.Empty;
                sb.AppendLine($"{id},{Escape(timestamp)},{Escape(r.IpAddress)},{Escape(r.Host)},{Escape(r.HostName)},{Escape(r.Platform)},{Escape(r.Referrer)},{Escape(r.Href)},{Escape(r.UserAgent)},{Escape(r.UserLanguage)},{Escape(r.SessionId)}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"visitor_export_{(StartDate.HasValue ? StartDate.Value: DateTime.MinValue):yyyyMMdd}_{(EndDate.HasValue ? EndDate.Value: DateTime.Now):yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r');
            var value = input.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{value}\"" : value;
        }
    }
}
