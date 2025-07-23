using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Dal.Services.Interfaces
{
    public interface IPollAnswerService
    {
        PollAnswer Get(string id);
        List<PollAnswer> GetAll();
        void Add(PollAnswer answer);
        void Update(PollAnswer answer);
        void Delete(string id);
    }
}
