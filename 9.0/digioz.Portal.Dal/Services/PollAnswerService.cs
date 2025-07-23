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

        public void Add(PollAnswer answer)
        {
            _context.PollAnswers.Add(answer);
            _context.SaveChanges();
        }

        public void Update(PollAnswer answer)
        {
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
    }
}
