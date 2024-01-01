using System.Collections.Generic;
using System.Linq;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL
{
    public class CommentLogic : AbstractLogic<Comment>, ICommentLogic
    {
        ICommentRepo _repo;
        public CommentLogic(ICommentRepo repo) : base(repo) {
            _repo = repo;
        }

        public List<Comment> GetCommentPostsByReference(int referenceId, string referenceType) {
            return _repo.GetCommentPostsByReference(referenceId, referenceType);
            //var query = new Query();
            //query.Where = $" ReferenceId = {referenceId} AND ReferenceType = '{referenceType}' ";
            //query.OrderBy = " ModifiedDate ";
            //return _repo.Get(query).ToList();
        }
    }
}
