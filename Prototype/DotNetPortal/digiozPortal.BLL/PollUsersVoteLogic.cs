using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PollUsersVoteLogic
    {
        public PollUsersVote Get(int id) {
            var PollUsersVoteRepo = new PollUsersVoteRepo();
            PollUsersVote PollUsersVote = PollUsersVoteRepo.Get(id);

            return PollUsersVote;
        }

        public List<PollUsersVote> GetAll() {
            var PollUsersVoteRepo = new PollUsersVoteRepo();
            var PollUsersVotes = PollUsersVoteRepo.GetAll();

            return PollUsersVotes;
        }

        public void Add(PollUsersVote PollUsersVote) {
            var PollUsersVoteRepo = new PollUsersVoteRepo();
            PollUsersVoteRepo.Add(PollUsersVote);
        }

        public void Edit(PollUsersVote PollUsersVote) {
            var PollUsersVoteRepo = new PollUsersVoteRepo();

            PollUsersVoteRepo.Edit(PollUsersVote);
        }

        public void Delete(int id) {
            var PollUsersVoteRepo = new PollUsersVoteRepo();
            var PollUsersVote = PollUsersVoteRepo.Get(id); // Db.PollUsersVotes.Find(id);

            if (PollUsersVote != null) {
                PollUsersVoteRepo.Delete(PollUsersVote);
            }
        }
    }

}
