using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class PollAnswerService : IPollAnswerService
    {
        private readonly digiozPortalContext _context;

        public PollAnswerService(digiozPortalContext context)
        {
            _context = context;
        }

        public PollAnswer Get(string id)
        {
            return _context.PollAnswers.Find(id);
        }

        public List<PollAnswer> GetAll()
        {
            return _context.PollAnswers.ToList();
        }

        public List<PollAnswer> GetByPollId(string pollId)
        {
            return _context.PollAnswers.Where(a => a.PollId == pollId).ToList();
        }

        public List<string> GetIdsByPollId(string pollId)
        {
            return _context.PollAnswers.Where(a => a.PollId == pollId).Select(a => a.Id).ToList();
        }

        public void Add(PollAnswer answer)
        {
            if (answer == null) return;
            answer.Answer = (answer.Answer ?? string.Empty).Trim();

            // Prevent duplicates (case-insensitive) within the same poll
            bool exists = _context.PollAnswers
                .Any(a => a.PollId == answer.PollId && a.Answer != null && a.Answer.ToLower() == answer.Answer.ToLower());
            if (exists) return;

            _context.PollAnswers.Add(answer);
            _context.SaveChanges();
        }

        public void Update(PollAnswer answer)
        {
            if (answer == null) return;
            answer.Answer = (answer.Answer ?? string.Empty).Trim();

            // Prevent making a duplicate on update
            bool exists = _context.PollAnswers
                .Any(a => a.PollId == answer.PollId && a.Id != answer.Id && a.Answer != null && a.Answer.ToLower() == answer.Answer.ToLower());
            if (exists) return;

            _context.PollAnswers.Update(answer);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var answer = _context.PollAnswers.Find(id);
            if (answer != null)
            {
                _context.PollAnswers.Remove(answer);
                _context.SaveChanges();
            }
        }

        public void DeleteByPollId(string pollId)
        {
            var answers = _context.PollAnswers.Where(a => a.PollId == pollId).ToList();
            if (answers.Count > 0)
            {
                _context.PollAnswers.RemoveRange(answers);
                _context.SaveChanges();
            }
        }
    }
}
