﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;
using digiozPortal.Utilities;

namespace digiozPortal.DAL
{
    public class CommentRepo : AbstractRepo<Comment>, ICommentRepo
    {
        public CommentRepo(IConfigHelper config) : base(config) { }

        public List<Comment> GetCommentPostsByReference(int referenceId, string referenceType) {
            string sqlChats = $"SELECT * FROM Comment WHERE ReferenceId = {referenceId} AND ReferenceType = '{referenceType}';";
            using (var connection = new SqlConnection(_connectionString)) {
                return connection.Query<Comment>(sqlChats).OrderBy(x => x.ModifiedDate).ToList();
            }
        }

    }
}
