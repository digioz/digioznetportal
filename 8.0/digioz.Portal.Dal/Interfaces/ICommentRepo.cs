using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Interfaces
{
    public interface ICommentRepo : IRepo<Comment>
    {
        List<Comment> GetCommentPostsByReference(int referenceId, string referenceType);
    }
}
