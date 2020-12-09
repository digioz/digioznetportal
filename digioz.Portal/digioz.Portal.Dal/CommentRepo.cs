using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Dal
{
    public class CommentRepo : AbstractRepo<Comment>, ICommentRepo
    {
        public CommentRepo(IConfigHelper config) : base(config) { }

        public List<Comment> GetCommentPostsByReference(int referenceId, string referenceType) {
            string sqlChats = $"SELECT * FROM Comment WHERE ReferenceId = {referenceId} AND ReferenceType = '{referenceType}';";

            return base.Get(sqlChats).ToList();

            //using (var connection = new SqlConnection(_connectionString)) {
            //    return connection.Query<Comment>(sqlChats).OrderBy(x => x.ModifiedDate).ToList();
            //}
        }

    }
}
