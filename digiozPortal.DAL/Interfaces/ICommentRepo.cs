using System.Collections.Generic;
using digiozPortal.BO;

namespace digiozPortal.DAL.Interfaces
{
    public interface ICommentRepo : IRepo<Comment>
    {
        List<Comment> GetCommentPostsByReference(int referenceId, string referenceType);
    }
}
