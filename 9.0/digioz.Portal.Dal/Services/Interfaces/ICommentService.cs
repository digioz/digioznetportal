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
    }
}
