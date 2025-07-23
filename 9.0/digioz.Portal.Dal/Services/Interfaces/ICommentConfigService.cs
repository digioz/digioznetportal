using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface ICommentConfigService
    {
        CommentConfig Get(string id);
        List<CommentConfig> GetAll();
        void Add(CommentConfig config);
        void Update(CommentConfig config);
        void Delete(string id);
    }
}
