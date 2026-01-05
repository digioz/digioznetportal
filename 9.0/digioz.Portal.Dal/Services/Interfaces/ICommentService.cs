using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ICommentService
    {
        Comment Get(string id);
        List<Comment> GetAll();
        List<Comment> GetByReferenceType(string referenceType);
        List<Comment> GetByUserId(string userId);
        int CountByUserId(string userId);
        int CountApprovedByUserId(string userId);
        void Add(Comment comment);
        void Update(Comment comment);
        void Delete(string id);
        
        // Bulk operations for performance
        int DeleteByUserId(string userId);
        int ReassignByUserId(string fromUserId, string toUserId);
        
        /// <summary>
        /// Searches comments by term in body field.
        /// </summary>
        /// <param name="term">Search term</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="totalCount">Output parameter for total matching count</param>
        /// <returns>List of matching comments</returns>
        List<Comment> Search(string term, int skip, int take, out int totalCount);
        
        /// <summary>
        /// Gets paginated and filtered comments with database-level filtering.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="visibleFilter">Filter by visible status: null=all, true=visible only, false=not visible only</param>
        /// <param name="approvedFilter">Filter by approved status: null=all, true=approved only, false=not approved only</param>
        /// <param name="referenceTypeFilter">Filter by reference type (null or empty for all)</param>
        /// <param name="totalCount">Output parameter for total matching count</param>
        /// <returns>Filtered and paginated list of comments</returns>
        List<Comment> GetPagedFiltered(int pageNumber, int pageSize, bool? visibleFilter, bool? approvedFilter, string? referenceTypeFilter, out int totalCount);
        
        /// <summary>
        /// Gets distinct reference types for filtering purposes.
        /// </summary>
        /// <returns>List of unique reference types</returns>
        List<string> GetDistinctReferenceTypes();
    }
}
