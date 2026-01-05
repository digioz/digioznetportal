using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class IndexModel : PageModel
    {
        private readonly ICommentService _service;
        public IndexModel(ICommentService service) { _service = service; }

        public IReadOnlyList<digioz.Portal.Bo.Comment> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Comment>();
        
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        
        [BindProperty(SupportsGet = true)]
        public string? VisibleFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? ApprovedFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? ReferenceTypeFilter { get; set; }
        
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public List<string> ReferenceTypes { get; private set; } = new List<string>();

        public void OnGet()
        {
            var all = _service.GetAll();
            
            // Get unique reference types for dropdown
            ReferenceTypes = all
                .Where(c => !string.IsNullOrEmpty(c.ReferenceType))
                .Select(c => c.ReferenceType!)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            // Apply filters
            var filtered = all.AsQueryable();

            if (!string.IsNullOrEmpty(VisibleFilter))
            {
                if (VisibleFilter == "Visible")
                    filtered = filtered.Where(c => c.Visible == true);
                else if (VisibleFilter == "NotVisible")
                    filtered = filtered.Where(c => c.Visible == false || c.Visible == null);
            }

            if (!string.IsNullOrEmpty(ApprovedFilter))
            {
                if (ApprovedFilter == "Approved")
                    filtered = filtered.Where(c => c.Approved == true);
                else if (ApprovedFilter == "NotApproved")
                    filtered = filtered.Where(c => c.Approved == false || c.Approved == null);
            }

            if (!string.IsNullOrEmpty(ReferenceTypeFilter) && ReferenceTypeFilter != "All")
            {
                filtered = filtered.Where(c => c.ReferenceType == ReferenceTypeFilter);
            }

            var result = filtered.OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate).ToList();

            TotalCount = result.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;

            var skip = (PageNumber - 1) * PageSize;
            Items = result.Skip(skip).Take(PageSize).ToList();
        }

        public IActionResult OnPostToggleVisible(string id)
        {
            var comment = _service.Get(id);
            if (comment != null)
            {
                comment.Visible = !(comment.Visible ?? false);
                comment.ModifiedDate = DateTime.UtcNow;
                _service.Update(comment);
            }
            return RedirectToPage(new { PageNumber, PageSize, VisibleFilter, ApprovedFilter, ReferenceTypeFilter });
        }

        public IActionResult OnPostToggleApproved(string id)
        {
            var comment = _service.Get(id);
            if (comment != null)
            {
                comment.Approved = !(comment.Approved ?? false);
                comment.ModifiedDate = DateTime.UtcNow;
                _service.Update(comment);
            }
            return RedirectToPage(new { PageNumber, PageSize, VisibleFilter, ApprovedFilter, ReferenceTypeFilter });
        }
    }
}
