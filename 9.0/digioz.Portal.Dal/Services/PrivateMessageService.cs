using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Services
{
    public class PrivateMessageService : IPrivateMessageService
    {
        private readonly digiozPortalContext _context;

        public PrivateMessageService(digiozPortalContext context)
        {
            _context = context;
        }

        public PrivateMessage Get(int id) => _context.PrivateMessages.Find(id);

        public List<PrivateMessage> GetInbox(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.ToId == userId)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetOutbox(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.FromId == userId && !pm.IsRead)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetSent(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.FromId == userId && pm.IsRead)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetThread(int messageId)
        {
            var thread = new List<PrivateMessage>();
            var current = _context.PrivateMessages.AsNoTracking().FirstOrDefault(p => p.Id == messageId);

            if (current == null)
            {
                return thread;
            }

            // Find the root message ID by traversing up
            int rootId = messageId;
            var parentId = current.ParentId;
            
            while (parentId != null)
            {
                var parent = _context.PrivateMessages.AsNoTracking()
                    .Where(p => p.Id == parentId.Value)
                    .Select(p => new { p.Id, p.ParentId })
                    .FirstOrDefault();
                    
                if (parent == null)
                {
                    break; // Should not happen in consistent data
                }
                
                rootId = parent.Id;
                parentId = parent.ParentId;
            }

            // Collect all messages in the thread by querying level by level
            var allMessages = new List<PrivateMessage>();
            var currentLevelIds = new List<int> { rootId };

            while (currentLevelIds.Any())
            {
                // Fetch all messages at the current level
                var currentLevelMessages = _context.PrivateMessages.AsNoTracking()
                    .Where(m => currentLevelIds.Contains(m.Id))
                    .ToList();
                
                allMessages.AddRange(currentLevelMessages);
                
                // Get IDs of all children for the next level
                var currentLevelMessageIds = currentLevelMessages.Select(cm => cm.Id).ToList();
                currentLevelIds = _context.PrivateMessages.AsNoTracking()
                    .Where(m => m.ParentId.HasValue && currentLevelMessageIds.Contains(m.ParentId.Value))
                    .Select(m => m.Id)
                    .ToList();
            }

            return allMessages.OrderBy(m => m.SentDate).ToList();
        }


        public void Add(PrivateMessage message)
        {
            message.SentDate = DateTime.UtcNow;
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();
        }

        public void MarkRead(int id)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null) return;
            pm.IsRead = true;
            _context.SaveChanges();
        }

        public void Delete(int id, string userId)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null) return;
            if (pm.FromId != userId && pm.ToId != userId) return; // not authorized
            _context.PrivateMessages.Remove(pm);
            _context.SaveChanges();
        }
    }
}
