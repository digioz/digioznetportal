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
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;

            // Get distinct reference types for dropdown (database-level)
            ReferenceTypes = _service.GetDistinctReferenceTypes();

            // Parse filter parameters
            bool? visibleFilterValue = null;
            if (VisibleFilter == "Visible")
                visibleFilterValue = true;
            else if (VisibleFilter == "NotVisible")
                visibleFilterValue = false;

            bool? approvedFilterValue = null;
            if (ApprovedFilter == "Approved")
                approvedFilterValue = true;
            else if (ApprovedFilter == "NotApproved")
                approvedFilterValue = false;

            string? referenceTypeFilterValue = null;
            if (!string.IsNullOrEmpty(ReferenceTypeFilter) && ReferenceTypeFilter != "All")
                referenceTypeFilterValue = ReferenceTypeFilter;

            // Get filtered and paginated data (database-level)
            Items = _service.GetPagedFiltered(
                PageNumber, 
                PageSize, 
                visibleFilterValue, 
                approvedFilterValue, 
                referenceTypeFilterValue, 
                out var total
            );
            TotalCount = total;
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
