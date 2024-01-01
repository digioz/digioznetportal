using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PollLogic
    {
        public Poll Get(string id) {
            var PollRepo = new PollRepo();
            Poll Poll = PollRepo.Get(id);

            return Poll;
        }

        public List<Poll> GetAll() {
            var PollRepo = new PollRepo();
            var Polls = PollRepo.GetAll();

            return Polls;
        }

        public void Add(Poll Poll) {
            var PollRepo = new PollRepo();
            PollRepo.Add(Poll);
        }

        public void Edit(Poll Poll) {
            var PollRepo = new PollRepo();

            PollRepo.Edit(Poll);
        }

        public void Delete(string id) {
            var PollRepo = new PollRepo();
            var Poll = PollRepo.Get(id); // Db.Polls.Find(id);

            if (Poll != null) {
                PollRepo.Delete(Poll);
            }
        }
    }

}
