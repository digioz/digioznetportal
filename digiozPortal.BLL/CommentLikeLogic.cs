using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class CommentLikeLogic
    {
        public CommentLike Get(string id) {
            var CommentLikeRepo = new CommentLikeRepo();
            CommentLike CommentLike = CommentLikeRepo.Get(id);

            return CommentLike;
        }

        public List<CommentLike> GetAll() {
            var CommentLikeRepo = new CommentLikeRepo();
            var CommentLikes = CommentLikeRepo.GetAll();

            return CommentLikes;
        }

        public void Add(CommentLike CommentLike) {
            var CommentLikeRepo = new CommentLikeRepo();
            CommentLikeRepo.Add(CommentLike);
        }

        public void Edit(CommentLike CommentLike) {
            var CommentLikeRepo = new CommentLikeRepo();

            CommentLikeRepo.Edit(CommentLike);
        }

        public void Delete(string id) {
            var CommentLikeRepo = new CommentLikeRepo();
            var CommentLike = CommentLikeRepo.Get(id); // Db.CommentLikes.Find(id);

            if (CommentLike != null) {
                CommentLikeRepo.Delete(CommentLike);
            }
        }
    }

}
