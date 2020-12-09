using System.Collections.Generic;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;

namespace digioz.Portal.Bll
{
    public class ChatLogic : AbstractLogic<Chat>, IChatLogic
    {
        IChatRepo _repo;
        public ChatLogic(IChatRepo repo) : base(repo) 
        {
            _repo = repo;
        }

        public List<Chat> GetLatestChats() 
        {
            return _repo.GetLatestChats(10,"Id","DESC");
        }
    }
}
