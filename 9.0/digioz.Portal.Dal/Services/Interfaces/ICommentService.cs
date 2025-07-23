using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ICommentService
    {
        Comment Get(string id);
        List<Comment> GetAll();
        void Add(Comment comment);
        void Update(Comment comment);
        void Delete(string id);
    }
}
