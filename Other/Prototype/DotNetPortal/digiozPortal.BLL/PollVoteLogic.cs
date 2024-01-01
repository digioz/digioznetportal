using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PollVoteLogic
    {
        public PollVote Get(string id) {
            var PollVoteRepo = new PollVoteRepo();
            PollVote PollVote = PollVoteRepo.Get(id);

            return PollVote;
        }

        public List<PollVote> GetAll() {
            var PollVoteRepo = new PollVoteRepo();
            var PollVotes = PollVoteRepo.GetAll();

            return PollVotes;
        }

        public void Add(PollVote PollVote) {
            var PollVoteRepo = new PollVoteRepo();
            PollVoteRepo.Add(PollVote);
        }

        public void Edit(PollVote PollVote) {
            var PollVoteRepo = new PollVoteRepo();

            PollVoteRepo.Edit(PollVote);
        }

        public void Delete(string id) {
            var PollVoteRepo = new PollVoteRepo();
            var PollVote = PollVoteRepo.Get(id); // Db.PollVotes.Find(id);

            if (PollVote != null) {
                PollVoteRepo.Delete(PollVote);
            }
        }
    }

}
