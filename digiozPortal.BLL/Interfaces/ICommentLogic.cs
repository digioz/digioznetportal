using System.Collections.Generic;
using digiozPortal.BO;

namespace digiozPortal.BLL.Interfaces
{
    public interface ICommentLogic : ILogic<Comment>
    {
        List<Comment> GetCommentPostsByReference(int referenceId, string referenceType);
    }
}
