using System;
using System.Linq;
using System.Text;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Chat
{
    public class ExportModel : PageModel
    {
        private readonly IChatService _service;
        public ExportModel(IChatService service) { _service = service; }

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
            var start = StartDate.Value.Date;
            var end = EndDate.Value.Date.AddDays(1).AddTicks(-1);

            var records = _service.GetAll()
            .Where(c => (c.Timestamp ?? DateTime.MinValue) >= start && (c.Timestamp ?? DateTime.MinValue) <= end)
            .OrderBy(c => c.Timestamp ?? DateTime.MinValue)
            .ThenBy(c => c.Id)
            .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Id,UserId,Timestamp,Message");
            foreach (var r in records)
            {
                var id = r.Id.ToString();
                var userId = Escape(r.UserId);
                var timestamp = (r.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss")) ?? string.Empty;
                var message = Escape(r.Message);
                sb.AppendLine($"{id},{userId},{timestamp},{message}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"chat_export_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
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
