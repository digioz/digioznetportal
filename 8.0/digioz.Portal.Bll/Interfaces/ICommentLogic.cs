using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Bll.Interfaces
{
    public interface ICommentLogic : ILogic<Comment>
    {
        List<Comment> GetCommentPostsByReference(int referenceId, string referenceType);
    }
}
