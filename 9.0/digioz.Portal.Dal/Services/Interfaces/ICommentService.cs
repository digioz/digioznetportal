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
    }
}
