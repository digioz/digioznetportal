using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ICommentLikeService
    {
        CommentLike Get(string id);
        List<CommentLike> GetAll();
        void Add(CommentLike like);
        void Update(CommentLike like);
        void Delete(string id);
    }
}
