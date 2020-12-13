using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class CommentLogic
    {
        public Comment Get(string id) {
            var CommentRepo = new CommentRepo();
            Comment Comment = CommentRepo.Get(id);

            return Comment;
        }

        public List<Comment> GetAll() {
            var CommentRepo = new CommentRepo();
            var Comments = CommentRepo.GetAll();

            return Comments;
        }

        public void Add(Comment Comment) {
            var CommentRepo = new CommentRepo();
            CommentRepo.Add(Comment);
        }

        public void Edit(Comment Comment) {
            var CommentRepo = new CommentRepo();

            CommentRepo.Edit(Comment);
        }

        public void Delete(string id) {
            var CommentRepo = new CommentRepo();
            var Comment = CommentRepo.Get(id); // Db.Comments.Find(id);

            if (Comment != null) {
                CommentRepo.Delete(Comment);
            }
        }
    }

}
