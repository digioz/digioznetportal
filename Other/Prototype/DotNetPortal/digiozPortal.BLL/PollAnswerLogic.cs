using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class PollAnswerLogic
    {
        public PollAnswer Get(string id) {
            var PollAnswerRepo = new PollAnswerRepo();
            PollAnswer PollAnswer = PollAnswerRepo.Get(id);

            return PollAnswer;
        }

        public List<PollAnswer> GetAll() {
            var PollAnswerRepo = new PollAnswerRepo();
            var PollAnswers = PollAnswerRepo.GetAll();

            return PollAnswers;
        }

        public void Add(PollAnswer PollAnswer) {
            var PollAnswerRepo = new PollAnswerRepo();
            PollAnswerRepo.Add(PollAnswer);
        }

        public void Edit(PollAnswer PollAnswer) {
            var PollAnswerRepo = new PollAnswerRepo();

            PollAnswerRepo.Edit(PollAnswer);
        }

        public void Delete(string id) {
            var PollAnswerRepo = new PollAnswerRepo();
            var PollAnswer = PollAnswerRepo.Get(id); // Db.PollAnswers.Find(id);

            if (PollAnswer != null) {
                PollAnswerRepo.Delete(PollAnswer);
            }
        }
    }

}
