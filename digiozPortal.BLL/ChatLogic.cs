using System.Collections.Generic;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL
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
