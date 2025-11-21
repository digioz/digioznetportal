using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Dal.Services
{
    public class ChatService : IChatService
    {
        private readonly digiozPortalContext _context;

        public ChatService(digiozPortalContext context)
        {
            _context = context;
        }

        public Chat Get(int id)
        {
            return _context.Chats.Find(id);
        }

        public List<Chat> GetAll()
        {
            return _context.Chats.ToList();
        }

        public List<Chat> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Chat>();

            return _context.Chats.Where(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == userId).ToList();
        }

        public int CountByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return _context.Chats.Count(c => !string.IsNullOrEmpty(c.UserId) && c.UserId == userId);
        }

        public void Add(Chat chat)
        {
            _context.Chats.Add(chat);
            _context.SaveChanges();
        }

        public void Update(Chat chat)
        {
            _context.Chats.Update(chat);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var chat = _context.Chats.Find(id);
            if (chat != null)
            {
                _context.Chats.Remove(chat);
                _context.SaveChanges();
            }
        }
    }
}
