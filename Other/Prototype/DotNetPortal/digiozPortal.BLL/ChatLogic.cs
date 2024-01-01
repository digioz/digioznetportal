using System;
using System.Collections.Generic;
using System.Text;

using digiozPortal.BO;
using digiozPortal.DAL;

namespace digiozPortal.BLL
{
    public class ChatLogic
    {
        public Chat Get(int id) {
            var ChatRepo = new ChatRepo();
            Chat Chat = ChatRepo.Get(id);

            return Chat;
        }

        public List<Chat> GetAll() {
            var ChatRepo = new ChatRepo();
            var Chats = ChatRepo.GetAll();

            return Chats;
        }

        public void Add(Chat Chat) {
            var ChatRepo = new ChatRepo();
            ChatRepo.Add(Chat);
        }

        public void Edit(Chat Chat) {
            var ChatRepo = new ChatRepo();

            ChatRepo.Edit(Chat);
        }

        public void Delete(int id) {
            var ChatRepo = new ChatRepo();
            var Chat = ChatRepo.Get(id); // Db.Chats.Find(id);

            if (Chat != null) {
                ChatRepo.Delete(Chat);
            }
        }
    }

}
